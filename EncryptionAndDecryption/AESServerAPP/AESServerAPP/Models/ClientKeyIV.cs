using System.ComponentModel.DataAnnotations;

namespace AESServerAPP.Models
{
    public class ClientKeyIV
    {
        [Key]
        public int Id { get; set; }
        // Unique identifier for the client.
        [Required]
        [MaxLength(50)]
        public string ClientId { get; set; }
        // Base64-encoded AES key.
        [Required]
        public string Key { get; set; }
        // Base64-encoded AES Initialization Vector.
        [Required]
        public string IV { get; set; }
    }
}
