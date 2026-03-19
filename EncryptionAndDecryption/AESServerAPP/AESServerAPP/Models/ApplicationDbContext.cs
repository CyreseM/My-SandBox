using Microsoft.EntityFrameworkCore;

namespace AESServerAPP.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        // Configures the schema needed for the context.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure unique constraint on ClientId
            modelBuilder.Entity<ClientKeyIV>()
                .HasIndex(c => c.ClientId)
                .IsUnique();

            // Seed initial ClientKeyIV data using HasData.
            modelBuilder.Entity<ClientKeyIV>().HasData(
                //Key: Base64-encoded AES key (256-bit).
                //IV: Base64-encoded AES IV (128-bit).
                new ClientKeyIV { Id = 1, ClientId = "DefaultClient", Key = "Yyj9nVLtBLwPANTqZNFHrofcH/AbvJlaUbytoHT8Qd8=", IV = "/X9EAc4vBALd31ye7N3L1g==" },
                new ClientKeyIV { Id = 2, ClientId = "Client1", Key = "gi1D2eDd8Tg565ZbfRWc00j9xKtBka4ZHu0Sen+Drgc=", IV = "Qb4nTgWS7UBo2YU7G/gJCg==" },
                new ClientKeyIV { Id = 3, ClientId = "Client2", Key = "mPjeDLj4jq5AnX/0WeDXBewm05AIOqbV83MfNTWap7A=", IV = "3Y/S5SC3qFNaSbfSKEKxxA==" },
                new ClientKeyIV { Id = 4, ClientId = "Client3", Key = "J0N55pEAha+B0Oyggc4zWV1GE9iWiW/m7W5DuUo0W3M=", IV = "8PiYfRaj4e5JumnpLh0FzA==" }
            );

            // Seed initial Employee data using HasData.
            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, Name = "Alice Smith", Salary = 75000m },
                new Employee { Id = 2, Name = "Bob Johnson", Salary = 60000m },
                new Employee { Id = 3, Name = "Carol White", Salary = 55000m }
            );
        }

        // DbSet representing Employees table.
        public DbSet<Employee> Employees { get; set; }

        // DbSet representing ClientKeyIV table.
        public DbSet<ClientKeyIV> ClientKeyIVs { get; set; }
}
}
