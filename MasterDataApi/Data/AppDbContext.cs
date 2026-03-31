// =============================================================
// Data/AppDbContext.cs
//
// DbContext is your "gateway" to the database.
// It contains DbSet<T> properties — one per table.
// Think of DbSet<Company> as a "handle" to the Companies table
// that lets you query, insert, update, and delete records.
//
// We're using EF Core InMemory for development so you can run
// the app without installing a real database. Swap to SQL Server,
// PostgreSQL, or SQLite by changing the registration in Program.cs.
// =============================================================

using Microsoft.EntityFrameworkCore;
using MasterDataApi.Models;

namespace MasterDataApi.Data;

public class AppDbContext : DbContext
{
    // This constructor takes DbContextOptions and passes it to the base class.
    // ASP.NET Core's dependency injection system provides the options
    // when it creates an AppDbContext for you.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Each DbSet = one database table
    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserAssignment> UserAssignments { get; set; }

    /// <summary>
    /// OnModelCreating is called once when EF Core builds its internal model.
    /// Here we configure table names, relationships, constraints, and seed data.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Company ──────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
        });

        // ── Company ──────────────────────────────────────────
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.Code).IsUnique(); // Code must be unique across all companies
            entity.Property(c => c.Name).IsRequired().HasMaxLength(255);
            entity.Property(c => c.Code).IsRequired().HasMaxLength(50);
        });

        // ── Department ───────────────────────────────────────
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(255);

            // IsGlobal is a computed property, not a real column — tell EF to ignore it
            entity.Ignore(d => d.IsGlobal);

            // A department MAY belong to a company (nullable FK)
            entity.HasOne(d => d.Company)
                  .WithMany(c => c.Departments)
                  .HasForeignKey(d => d.CompanyId)
                  .IsRequired(false)           // CompanyId is optional
                  .OnDelete(DeleteBehavior.SetNull); // If company deleted, set CompanyId to null
        });

        // ── Role ─────────────────────────────────────────────
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name).IsUnique(); // Role names must be unique
            entity.Property(r => r.Name).IsRequired().HasMaxLength(255);
        });

        // ── UserAssignment ───────────────────────────────────
        modelBuilder.Entity<UserAssignment>(entity =>
        {
            // Primary key is the UserId — one user = one assignment at a time
            entity.HasKey(ua => ua.UserId);

            entity.Property(ua => ua.CompanyCode).IsRequired().HasMaxLength(50);

            // Link to User (one-to-one: one user has one assignment)
            entity.HasOne(ua => ua.User)
                  .WithOne(u => u.Assignment)
                  .HasForeignKey<UserAssignment>(ua => ua.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Link to Department
            entity.HasOne(ua => ua.Department)
                  .WithMany()
                  .HasForeignKey(ua => ua.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Link to Role
            entity.HasOne(ua => ua.Role)
                  .WithMany()
                  .HasForeignKey(ua => ua.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Seed Data ────────────────────────────────────────
        // These records exist when the app starts — useful for testing in Swagger.
        // In production you'd remove this or use migrations with real seed data.

        var companyId1 = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        var companyId2 = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7");
        var deptId1    = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851");
        var deptId2    = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");
        var roleId1    = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
        var roleId2    = Guid.Parse("b1ccde00-ad1c-5f09-cc7e-7cc0ce491b22");
        var userId1    = Guid.Parse("e3d7c9a1-1b2e-4c5d-9f0a-7b8c6d5e4f3a");
        var userId2    = Guid.Parse("f4e8d0b2-0000-0000-0000-000000000002");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = userId1,
                FullName = "John Carter",
                Email = "john.carter@example.com",
                CreatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc)
            },
            new User
            {
                Id = userId2,
                FullName = "Priya Nair",
                Email = "priya.nair@example.com",
                CreatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<Company>().HasData(
            new Company { Id = companyId1, Name = "ABC Ltd",   Code = "ABC001",
                          CreatedAt = new DateTime(2024,1,15,9,0,0,DateTimeKind.Utc),
                          UpdatedAt = new DateTime(2024,3,20,14,30,0,DateTimeKind.Utc) },
            new Company { Id = companyId2, Name = "XYZ Corp",  Code = "XYZ002",
                          CreatedAt = new DateTime(2024,2,1,8,0,0,DateTimeKind.Utc),
                          UpdatedAt = new DateTime(2024,2,1,8,0,0,DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = deptId1, Name = "Human Resources", CompanyId = companyId1,
                             CreatedAt = new DateTime(2024,1,16,10,0,0,DateTimeKind.Utc),
                             UpdatedAt = new DateTime(2024,1,16,10,0,0,DateTimeKind.Utc) },
            new Department { Id = deptId2, Name = "IT Support",      CompanyId = null, // global
                             CreatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc),
                             UpdatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = roleId1, Name = "Manager",
                       CreatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc),
                       UpdatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc) },
            new Role { Id = roleId2, Name = "Analyst",
                       CreatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc),
                       UpdatedAt = new DateTime(2024,1,10,8,0,0,DateTimeKind.Utc) }
        );
    }
}
