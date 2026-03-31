namespace AuthenticationServer.Models
{
    public class SSOToken
    {
        public int Id { get; set; }                   // Unique ID for the token record
        public string UserId { get; set; } = null!;   // Links token to a specific user
        public string Token { get; set; } = null!;    // The SSO token string (usually a GUID)
        public DateTime ExpiryDate { get; set; }      // When the token expires
        public bool IsUsed { get; set; }              // Whether the token has been used
        public bool IsExpired => DateTime.UtcNow > ExpiryDate; // Computed property to check if the token is expired.
    }
}