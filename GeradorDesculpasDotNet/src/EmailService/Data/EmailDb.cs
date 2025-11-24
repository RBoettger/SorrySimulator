using EmailService.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailService.Data
{
      public class EmailDb : DbContext
      {
            public EmailDb(DbContextOptions<EmailDb> options) : base(options) { }

            public DbSet<GdExcuseHistory> ExcuseHistory => Set<GdExcuseHistory>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  modelBuilder.HasDefaultSchema("gd");

                  modelBuilder.Entity<GdExcuseHistory>(entity =>
                  {
                        entity.ToTable("ExcuseHistory");

                        entity.HasKey(e => e.HistoryId);

                        entity.Property(e => e.SenderName)
                        .HasMaxLength(120)
                        .IsRequired();

                        entity.Property(e => e.ToEmail)
                        .HasMaxLength(254)
                        .IsRequired();

                        entity.Property(e => e.Subject)
                        .HasMaxLength(200);

                        entity.Property(e => e.Motive)
                        .HasMaxLength(200);

                        entity.Property(e => e.Tone)
                        .HasMaxLength(50);

                        entity.Property(e => e.SentAt)
                        .HasColumnType("datetime2(3)");

                        entity.Property(e => e.UserId).HasColumnName("UserId");
                  });
            }
      }
}