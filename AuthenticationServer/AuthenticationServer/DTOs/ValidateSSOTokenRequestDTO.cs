using System.ComponentModel.DataAnnotations;
namespace AuthenticationServer.DTOs
{
    public class ValidateSSOTokenRequesDTO
    {
        [Required(ErrorMessage = "SSOToken is Required")]
        public string SSOToken { get; set; } = null!;
    }
}