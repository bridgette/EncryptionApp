using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace encryption.Crypto
{
    public class KeyStore
    {
        private static KeyStore _instance = new KeyStore();
        public static KeyStore Instance 
        { 
            get 
            {
                return _instance;
            }
        }

        public void Init()
        {
            if (!initialized)
            {
                keysdb = new KeysDB();
                keysdb.Init();
                initialized = true;
            }
        }

        public void StoreKeyPair(PgpSecretKeyRing privateKey, PgpPublicKeyRing publicKey)
        {
            KeyPair keyPair = new KeyPair();
            MemoryStream stream = new MemoryStream();
            privateKey.Encode(stream);
            stream.Flush();

            keyPair.PrivateKeyBlob = stream.ToArray();

            stream = new MemoryStream();
            publicKey.Encode(stream);
            stream.Flush();
            keyPair.PublicKeyBlob = stream.ToArray();

            keysdb.StoreKeyPair(keyPair);
        }

        public async Task<byte[]> GetMyPrivateKey()
        {
            KeyPair keyPair = await keysdb.GetKeyPair();
            if (keyPair != null)
                return keyPair.PrivateKeyBlob;
            else
                return null;
        }

        public async Task<byte[]> GetMyPublicKey()
        {
            KeyPair keyPair = await keysdb.GetKeyPair();
            if (keyPair != null)
                return keyPair.PublicKeyBlob;
            else
                return null;
        }

        public async Task<bool> AddPublicKey(string email, byte[] key)
        {
            PublicKey publicKey = new PublicKey();
            publicKey.Email = email.ToLower();
            publicKey.KeyBlob = key;

            int i = await keysdb.AddPublicKey(publicKey);

            return i != 0;
        }

        public async Task<byte[]> GetPublicKey(string email)
        {
            PublicKey key = await keysdb.GetPublicKey(email.ToLower());
            if (key != null)
                return key.KeyBlob;
            else
                return null;
        }

        private KeysDB keysdb;
        private bool initialized = false;
    }
}
