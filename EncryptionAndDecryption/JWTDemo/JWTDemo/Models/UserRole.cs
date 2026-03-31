namespace JWTDemo.Models
{
    public class UserRole
    {
        // Foreign key referencing User.
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        // Foreign key referencing Role.
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}
