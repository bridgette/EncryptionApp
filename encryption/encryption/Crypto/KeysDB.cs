using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;


namespace encryption.Crypto
{
    public class KeysDB
    {
        private const string DBFILE = "keystore.db";
        private const string PRIMARY_KEY_PAIR = "primary";

        SQLiteAsyncConnection connection;

        public async void Init()
        {
            if (await DoesDbExist())
            {
                connection = new SQLiteAsyncConnection(DBFILE);
            }
            else
            {
                CreateDatabase();
            }
        }

        public async Task<int> AddPublicKey(PublicKey key)
        {
            return await connection.InsertAsync(key);
        }

        public async Task<PublicKey> GetPublicKey(string email)
        {
            return await connection.Table<PublicKey>().Where(x => x.Email.StartsWith(email)).FirstOrDefaultAsync();

        }

        public async void StoreKeyPair(KeyPair key)
        {
            // Replace the the only primary key we have in DB
            var existingKey = await connection.Table<KeyPair>().Where(x => x.UserId.Equals(PRIMARY_KEY_PAIR)).FirstOrDefaultAsync();

            if (existingKey != null)
            {
                existingKey.PublicKeyBlob = key.PublicKeyBlob;
                existingKey.PrivateKeyBlob = key.PrivateKeyBlob;

                await connection.UpdateAsync(existingKey);
            }
            else
            {
                key.UserId = PRIMARY_KEY_PAIR;
                await connection.InsertAsync(key);
            }
        }

        public async Task<KeyPair> GetKeyPair()
        {
            return await connection.Table<KeyPair>().Where(x => x.UserId.Equals(PRIMARY_KEY_PAIR)).FirstOrDefaultAsync();
        }

        private async void CreateDatabase()
        {
            connection = new SQLiteAsyncConnection(DBFILE);
            await connection.CreateTableAsync<PublicKey>();
            await connection.CreateTableAsync<KeyPair>();
        }

        private async Task<bool> DoesDbExist()
        {
            bool dbexist = true;
            try
            {
                StorageFile storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(DBFILE);

            }
            catch
            {
                dbexist = false;
            }

            return dbexist;
        }
    }
}
