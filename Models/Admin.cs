using System.ComponentModel.DataAnnotations;

namespace YazlabBirSonProje.Models
{
    public class Admin
    {
        [Key]
        public int AdminID { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
