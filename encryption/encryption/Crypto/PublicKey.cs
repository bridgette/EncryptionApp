using SQLite;

namespace encryption.Crypto
{
    [Table("PublicKey")]
    public class PublicKey
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        public string Email { get; set; }

        public byte[] KeyBlob { get; set; }
    }
}
