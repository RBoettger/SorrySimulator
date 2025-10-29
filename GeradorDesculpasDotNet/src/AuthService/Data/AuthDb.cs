using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace AuthService.Data;

public class AuthDb : DbContext
{
    public AuthDb(DbContextOptions<AuthDb> options) : base(options) { }

    public DbSet<GdUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("gd");
        modelBuilder.Entity<GdUser>().ToTable("Users");
        modelBuilder.Entity<GdUser>().HasKey(u => u.UserId);
    }
}
