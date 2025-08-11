/*
using Microsoft.EntityFrameworkCore;

namespace RequirementsAnalyzer.API.Data
{
    /// <summary>
    /// Database context for the Requirements Analyzer application
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the AppDbContext
        /// </summary>
        /// <param name="options">Database context options</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Database set for Requirements entities
        /// </summary>
        public DbSet<Requirement> Requirements { get; set; }

        /// <summary>
        /// Database set for QualityIssue entities
        /// </summary>
        public DbSet<QualityIssue> QualityIssues { get; set; }

        /// <summary>
        /// Database set for Enhancement entities
        /// </summary>
        public DbSet<Enhancement> Enhancements { get; set; }

        /// <summary>
        /// Configures the model relationships and constraints
        /// </summary>
        /// <param name="modelBuilder">Model builder instance</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Requirement entity
            modelBuilder.Entity<Requirement>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalText).IsRequired().HasMaxLength(5000);
                entity.Property(e => e.EnhancedText).HasMaxLength(5000);
                entity.Property(e => e.QualityScore).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure QualityIssue entity
            modelBuilder.Entity<QualityIssue>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ProblematicText).HasMaxLength(500);
                entity.Property(e => e.Suggestion).HasMaxLength(1000);

                // Configure relationship with Requirement
                entity.HasOne(e => e.Requirement)
                      .WithMany(r => r.Issues)
                      .HasForeignKey(e => e.RequirementId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Enhancement entity
            modelBuilder.Entity<Enhancement>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(5000);
                entity.Property(e => e.Changes).HasMaxLength(2000);
                entity.Property(e => e.Improvements).HasMaxLength(2000);
                entity.Property(e => e.QualityScore).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Configure relationship with Requirement
                entity.HasOne(e => e.Requirement)
                      .WithMany(r => r.Enhancements)
                      .HasForeignKey(e => e.RequirementId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Add indexes for better query performance
            modelBuilder.Entity<Requirement>()
                       .HasIndex(r => r.CreatedAt);

            modelBuilder.Entity<QualityIssue>()
                       .HasIndex(q => new { q.RequirementId, q.Type });

            modelBuilder.Entity<Enhancement>()
                       .HasIndex(e => new { e.RequirementId, e.QualityScore });
        }
    }
}

*/