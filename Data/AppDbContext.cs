using Microsoft.EntityFrameworkCore;
using RequirementsAnalyzer.API.Models.Entities;

namespace RequirementsAnalyzer.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Requirement> Requirements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Project entity
            modelBuilder.Entity<Project>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

                // Index for performance
                entity.HasIndex(e => e.Name);
            });

            // Configure Requirement entity
            modelBuilder.Entity<Requirement>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Title).HasMaxLength(255);
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

                // Configure JSONB columns for PostgreSQL
                entity.Property(e => e.AnalysisData)
                    .HasColumnType("jsonb");
                entity.Property(e => e.EnhancementData)
                    .HasColumnType("jsonb");

                // Foreign key relationship
                entity.HasOne(e => e.Project)
                    .WithMany(p => p.Requirements)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for performance
                entity.HasIndex(e => e.ProjectId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // FIXED: Use static dates for seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Use static datetime values to avoid the pending changes warning
            var seedDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            // Seed some sample projects for development
            modelBuilder.Entity<Project>().HasData(
                new Project {
                    Id = 1,
                    Name = "Sample Project",
                    Description = "A sample project for testing",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            modelBuilder.Entity<Requirement>().HasData(
                new Requirement {
                    Id = 1,
                    ProjectId = 1,
                    Title = "Sample Requirement",
                    Text = "The system shall provide user authentication functionality",
                    Status = RequirementStatus.Draft,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Project project)
                {
                    if (entry.State == EntityState.Added)
                        project.CreatedAt = DateTime.UtcNow;
                    project.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Requirement requirement)
                {
                    if (entry.State == EntityState.Added)
                        requirement.CreatedAt = DateTime.UtcNow;
                    requirement.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}