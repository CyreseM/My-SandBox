namespace ClientApplicationTwo.Models
{
    // Represents the login response that contains the JWT token.
    public class LoginResponseModel
    {
        public string? Token { get; set; }
    }

    // Represents the response after SSO token validation, including a new access token and user details.
    public class ValidateSSOResponseModel
    {
        public string? Token { get; set; }
        public UserDetailsModel? UserDetails { get; set; }
    }

    public class UserDetailsModel
    {
        public string UserId { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    // Represents responses from the Resource Server endpoints (public/protected data).
    public class DataResponseModel
    {
        public string? Message { get; set; }
    }
}