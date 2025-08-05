// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using RequirementsAnalyzer.API.Models;

namespace RequirementsAnalyzer.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<QualityIssue> QualityIssues { get; set; }
        public DbSet<Enhancement> Enhancements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Requirement entity
            modelBuilder.Entity<Requirement>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalText).IsRequired().HasMaxLength(5000);
                entity.Property(e => e.EnhancedText).HasMaxLength(5000);
                entity.Property(e => e.CreatedAt).IsRequired();

                // One-to-many relationship with QualityIssues
                entity.HasMany(r => r.Issues)
                      .WithOne(i => i.Requirement)
                      .HasForeignKey(i => i.RequirementId)
                      .OnDelete(DeleteBehavior.Cascade);

                // One-to-many relationship with Enhancements
                entity.HasMany(r => r.Enhancements)
                      .WithOne(e => e.Requirement)
                      .HasForeignKey(e => e.RequirementId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure QualityIssue entity
            modelBuilder.Entity<QualityIssue>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ProblematicText).HasMaxLength(500);
                entity.Property(e => e.Suggestion).HasMaxLength(1000);
            });

            // Configure Enhancement entity
            modelBuilder.Entity<Enhancement>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(5000);
                entity.Property(e => e.Changes).HasMaxLength(2000);
                entity.Property(e => e.Improvements).HasMaxLength(2000);
                entity.Property(e => e.QualityScore).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }
}