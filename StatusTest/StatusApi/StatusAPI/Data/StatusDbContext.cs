using System;
using Microsoft.EntityFrameworkCore;
using StatusAPI.Models;

namespace StatusAPI.Data;

public class StatusDbContext : DbContext
{
    public StatusDbContext(DbContextOptions<StatusDbContext> options) : base(options) { }

    public DbSet<Status> Statuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Status>()
            .HasIndex(s => s.ExpiresAt);

        modelBuilder.Entity<Status>()
            .HasIndex(s => s.UserId);
    }
}

