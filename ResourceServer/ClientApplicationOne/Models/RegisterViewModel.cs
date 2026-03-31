using System.ComponentModel.DataAnnotations;
namespace ClientApplicationOne.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is Required")]
        public string Username { get; set; } = null!;
        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}