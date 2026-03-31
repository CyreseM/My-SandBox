using AuthenticationServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationServer.Data
{
    // IdentityDbContext<IdentityUser> provides all the necessary tables for authentication, such as AspNetUsers, AspNetRoles, etc.
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<SSOToken> SSOTokens { get; set; } = null!;
    }
}