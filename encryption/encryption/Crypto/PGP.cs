using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace encryption.Crypto
{
    public class PGP
    {

        public static string ByteKeyToText(byte[] publicKey)
        {
            byte[] output;
            using(MemoryStream outstream = new MemoryStream())
            { 
                ArmoredOutputStream armor = new ArmoredOutputStream(outstream);
                armor.Write(publicKey, 0, publicKey.Length);
                armor.Flush();
                outstream.Flush();

                output = outstream.ToArray();
            }

            return Encoding.UTF8.GetString(output, 0, output.Length);
        }

        public static void GenerateKeyPair(string userid, string password, out PgpPublicKeyRing pkr, out PgpSecretKeyRing skr)
        {
            PgpKeyRingGenerator krgen = PGP.generateKeyRingGenerator(userid, password);

            // Generate public key ring, dump to file.
            pkr = krgen.GeneratePublicKeyRing();

            // Generate private key, dump to file.
            skr = krgen.GenerateSecretKeyRing();
        }

        public static byte[] Decrypt(byte[] inputData, byte[] privateKey, string passCode, out string debugMessage)
        {
            MemoryStream keyIn = new MemoryStream(privateKey);
            keyIn.Position = 0;

            debugMessage = string.Empty;

            byte[] error = Encoding.UTF8.GetBytes("ERROR");

            Stream inputStream = new MemoryStream(inputData);
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            MemoryStream decoded = new MemoryStream();

            try
            {
                PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
                PgpEncryptedDataList enc;
                PgpObject o = pgpF.NextPgpObject();

                //
                // the first object might be a PGP marker packet.
                //
                if (o is PgpEncryptedDataList)
                    enc = (PgpEncryptedDataList)o;
                else
                    enc = (PgpEncryptedDataList)pgpF.NextPgpObject();

                //
                // find the secret key
                //
                PgpPrivateKey sKey = null;
                PgpPublicKeyEncryptedData pbe = null;
                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(
                PgpUtilities.GetDecoderStream(keyIn));
                foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                {
                    sKey = FindSecretKey(pgpSec, pked.KeyId, passCode.ToCharArray());
                    if (sKey != null)
                    {
                        pbe = pked;
                        break;
                    }
                }
                if (sKey == null)
                    throw new ArgumentException("secret key for message not found.");

                Stream clear = pbe.GetDataStream(sKey);
                PgpObjectFactory plainFact = new PgpObjectFactory(clear);
                PgpObject message = plainFact.NextPgpObject();

                if (message is PgpCompressedData)
                {
                    PgpCompressedData cData = (PgpCompressedData)message;
                    PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());
                    message = pgpFact.NextPgpObject();
                }
                if (message is PgpLiteralData)
                {
                    PgpLiteralData ld = (PgpLiteralData)message;
                    Stream unc = ld.GetInputStream();
                    Streams.PipeAll(unc, decoded);
                }
                else if (message is PgpOnePassSignatureList)
                    throw new PgpException("encrypted message contains a signed message - not literal data.");
                else
                    throw new PgpException("message is not a simple encrypted file - type unknown.");

                if (pbe.IsIntegrityProtected())
                {
                    if (!pbe.Verify())
                        debugMessage = "Message failed integrity check.";
                    else
                        debugMessage = "Message integrity check passed.";
                }
                else
                {
                    debugMessage = "No message integrity check.";
                }

                return decoded.ToArray();
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith("Checksum mismatch"))
                    debugMessage = "Likely invalid passcode. Possible data corruption.";
                else if (e.Message.StartsWith("Object reference not"))
                    debugMessage = "PGP data does not exist.";
                else if (e.Message.StartsWith("Premature end of stream"))
                    debugMessage = "Partial PGP data found.";
                else
                    debugMessage = e.Message;
                Exception underlyingException = e.InnerException;
                if (underlyingException != null)
                    debugMessage = underlyingException.Message;

                return error;
            }
        }

        public static byte[] Encrypt(byte[] inputData, byte[] publickey, bool withIntegrityCheck, bool armor)
        {
            MemoryStream keyStream = new MemoryStream(publickey);            
            PgpPublicKey passPhrase = PGP.ReadPublicKey(keyStream);

            byte[] processedData = Compress(inputData, PgpLiteralData.Console, CompressionAlgorithmTag.Uncompressed);

            MemoryStream bOut = new MemoryStream();
            Stream output = bOut;

            if (armor)
                output = new ArmoredOutputStream(output);

            PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());
            encGen.AddMethod(passPhrase);

            Stream encOut = encGen.Open(output, processedData.Length);

            encOut.Write(processedData, 0, processedData.Length);
            encOut.Flush();
            encOut.Dispose(); // encOut.Close();

            if (armor)
            {
                output.Flush(); //output.Close();
                output.Dispose();
            }

            bOut.Flush();

            return bOut.ToArray();
        }

        private static byte[] Compress(byte[] clearData, string fileName, CompressionAlgorithmTag algorithm)
        {
            MemoryStream bOut = new MemoryStream();

            PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(algorithm);
            Stream cos = comData.Open(bOut); // open it with the final destination
            PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();

            // we want to Generate compressed data. This might be a user option later,
            // in which case we would pass in bOut.
            Stream pOut = lData.Open(
            cos,                    // the compressed output stream
            PgpLiteralData.Binary,
            fileName,               // "filename" to store
            clearData.Length,       // length of clear data
            DateTime.UtcNow         // current time
            );

            pOut.Write(clearData, 0, clearData.Length);
            pOut.Flush();
            pOut.Dispose();

            comData.Close();

            return bOut.ToArray();
        }

        private static PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);
            //
            // we just loop through the collection till we find a key suitable for encryption, in the real
            // world you would probably want to be a bit smarter about this.
            //
            //
            // iterate through the key rings.
            //
            foreach (PgpPublicKeyRing kRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey k in kRing.GetPublicKeys())
                {
                    if (k.IsEncryptionKey)
                        return k;
                }
            }

            throw new ArgumentException("Can't find encryption key in key ring.");
        }


        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
                return null;

            return pgpSecKey.ExtractPrivateKey(pass);
        }

        private static void ExportKeyPair(
                    Stream secretOut,
                    Stream publicOut,
                    AsymmetricKeyParameter publicKey,
                    AsymmetricKeyParameter privateKey,
                    string identity,
                    char[] passPhrase,
                    bool armor)
        {
            if (armor)
            {
                secretOut = new ArmoredOutputStream(secretOut);
            }

            PgpSecretKey secretKey = new PgpSecretKey(
                PgpSignature.DefaultCertification,
                PublicKeyAlgorithmTag.RsaGeneral,
                publicKey,
                privateKey,
                DateTime.Now,
                identity,
                SymmetricKeyAlgorithmTag.Aes256,
                passPhrase,
                null,
                null,
                new SecureRandom()
                //                ,"BC"
                );

            secretKey.Encode(secretOut);

            //secretOut.Close();

            if (armor)
            {
                publicOut = new ArmoredOutputStream(publicOut);
            }

            PgpPublicKey key = secretKey.PublicKey;

            key.Encode(publicOut);

            //publicOut.Close();
        }


        public static PgpKeyRingGenerator generateKeyRingGenerator(String identity, String password)
        {

            KeyRingParams keyRingParams = new KeyRingParams();
            keyRingParams.Password = password;
            keyRingParams.Identity = identity;
            keyRingParams.PrivateKeyEncryptionAlgorithm = SymmetricKeyAlgorithmTag.Aes256;
            keyRingParams.SymmetricAlgorithms = new SymmetricKeyAlgorithmTag[] {
            SymmetricKeyAlgorithmTag.Cast5
            /*SymmetricKeyAlgorithmTag.Aes256,
            SymmetricKeyAlgorithmTag.Aes192,
            SymmetricKeyAlgorithmTag.Aes128*/
        };

            keyRingParams.HashAlgorithms = new HashAlgorithmTag[] {            
            HashAlgorithmTag.Sha512,
        };

            IAsymmetricCipherKeyPairGenerator generator
                = GeneratorUtilities.GetKeyPairGenerator("RSA");
            generator.Init(keyRingParams.RsaParams);

            /* Create the master (signing-only) key. */
            PgpKeyPair masterKeyPair = new PgpKeyPair(
                PublicKeyAlgorithmTag.RsaSign,
                generator.GenerateKeyPair(),
                DateTime.UtcNow);
            //Debug.WriteLine("Generated master key with ID "
            //   + masterKeyPair.KeyId.ToString("X"));

            PgpSignatureSubpacketGenerator masterSubpckGen
                = new PgpSignatureSubpacketGenerator();
            masterSubpckGen.SetKeyFlags(false, PgpKeyFlags.CanSign
                | PgpKeyFlags.CanCertify);
            masterSubpckGen.SetPreferredSymmetricAlgorithms(false,
                (from a in keyRingParams.SymmetricAlgorithms
                 select (int)a).ToArray());
            masterSubpckGen.SetPreferredHashAlgorithms(false,
                (from a in keyRingParams.HashAlgorithms
                 select (int)a).ToArray());

            /* Create a signing and encryption key for daily use. */
            PgpKeyPair encKeyPair = new PgpKeyPair(
                PublicKeyAlgorithmTag.RsaGeneral,
                generator.GenerateKeyPair(),
                DateTime.UtcNow);


            PgpSignatureSubpacketGenerator encSubpckGen = new PgpSignatureSubpacketGenerator();
            encSubpckGen.SetKeyFlags(false, PgpKeyFlags.CanEncryptCommunications | PgpKeyFlags.CanEncryptStorage);

            masterSubpckGen.SetPreferredSymmetricAlgorithms(false,
                (from a in keyRingParams.SymmetricAlgorithms
                 select (int)a).ToArray());
            masterSubpckGen.SetPreferredHashAlgorithms(false,
                (from a in keyRingParams.HashAlgorithms
                 select (int)a).ToArray());

            /* Create the key ring. */
            PgpKeyRingGenerator keyRingGen = new PgpKeyRingGenerator(
                PgpSignature.DefaultCertification,
                masterKeyPair,
                keyRingParams.Identity,
                keyRingParams.PrivateKeyEncryptionAlgorithm.Value,
                keyRingParams.GetPassword(),
                true,
                masterSubpckGen.Generate(),
                null,
                new SecureRandom());

            /* Add encryption subkey. */
            keyRingGen.AddSubKey(encKeyPair, encSubpckGen.Generate(), null);

            return keyRingGen;

        }
    }

    // Define other methods and classes here
    class KeyRingParams
    {

        public SymmetricKeyAlgorithmTag? PrivateKeyEncryptionAlgorithm { get; set; }
        public SymmetricKeyAlgorithmTag[] SymmetricAlgorithms { get; set; }
        public HashAlgorithmTag[] HashAlgorithms { get; set; }
        public RsaKeyGenerationParameters RsaParams { get; set; }
        public string Identity { get; set; }
        public string Password { get; set; }
        //= EncryptionAlgorithm.NULL;

        public char[] GetPassword()
        {
            return Password.ToCharArray();
        }

        public KeyRingParams()
        {
            //Org.BouncyCastle.Crypto.Tls.EncryptionAlgorithm
            RsaParams = new RsaKeyGenerationParameters(BigInteger.ValueOf(0x10001), new SecureRandom(), 2048, 12);
        }
    }


}
