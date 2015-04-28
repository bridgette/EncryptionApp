using SQLite;

namespace encryption.Crypto
{
    [Table("KeyPair")]
    public class KeyPair
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string UserId { get; set; }
        public byte[] PublicKeyBlob { get; set; }
        public byte[] PrivateKeyBlob { get; set; }
    }
}
