
using Microsoft.EntityFrameworkCore;
using QuizHub.Models;

namespace QuizHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<QuizHubUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QuizHubUser>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id)
                    .HasColumnName("id");

                entity.Property(u => u.Username)
                    .HasColumnName("username")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasMaxLength(255)
                    .IsRequired();
            });
        }
    }
}
