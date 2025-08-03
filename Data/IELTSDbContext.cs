using Microsoft.EntityFrameworkCore;
using IELTSPracticeApp.Data.Models;

namespace IELTSPracticeApp.Data
{
    public class IELTSDbContext : DbContext
    {
        public IELTSDbContext(DbContextOptions<IELTSDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<ReadingPassage> ReadingPassages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<TestSession> TestSessions { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<UserProgress> UserProgress { get; set; }
        public DbSet<ReadingSection> ReadingSections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ReadingPassage
            modelBuilder.Entity<ReadingPassage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Source).HasMaxLength(500);
                entity.Property(e => e.WordCount).HasDefaultValue(2400);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                entity.HasIndex(e => e.Title);
                entity.HasIndex(e => e.DifficultyLevel);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Question
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionText).IsRequired();
                entity.Property(e => e.CorrectAnswer).IsRequired();
                entity.Property(e => e.Points).HasDefaultValue(1);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                entity.HasOne(e => e.ReadingPassage)
                      .WithMany(e => e.Questions)
                      .HasForeignKey(e => e.ReadingPassageId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.ReadingPassageId);
                entity.HasIndex(e => e.QuestionType);
                entity.HasIndex(e => e.QuestionOrder);
            });

            // Configure QuestionOption
            modelBuilder.Entity<QuestionOption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OptionText).IsRequired();
                entity.Property(e => e.OptionKey).HasMaxLength(10).IsRequired();
                
                entity.HasOne(e => e.Question)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.QuestionId);
                entity.HasIndex(e => new { e.QuestionId, e.OptionKey }).IsUnique();
            });

            // Configure TestSession
            modelBuilder.Entity<TestSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TimeAllowedSeconds).HasDefaultValue(3600);
                entity.Property(e => e.TimeSpentSeconds).HasDefaultValue(0);
                entity.Property(e => e.CorrectAnswers).HasDefaultValue(0);
                entity.Property(e => e.Score).HasDefaultValue(0).HasPrecision(3, 1);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                
                entity.HasOne(e => e.ReadingPassage)
                      .WithMany(e => e.TestSessions)
                      .HasForeignKey(e => e.ReadingPassageId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartTime);
            });

            // Configure UserAnswer
            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserResponse).IsRequired();
                entity.Property(e => e.PointsAwarded).HasDefaultValue(0);
                entity.Property(e => e.TimeSpentSeconds).HasDefaultValue(0);
                entity.Property(e => e.AnsweredAt).HasDefaultValueSql("datetime('now')");
                
                entity.HasOne(e => e.TestSession)
                      .WithMany(e => e.UserAnswers)
                      .HasForeignKey(e => e.TestSessionId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Question)
                      .WithMany(e => e.UserAnswers)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.TestSessionId);
                entity.HasIndex(e => e.QuestionId);
                entity.HasIndex(e => new { e.TestSessionId, e.QuestionId }).IsUnique();
            });

            // Configure UserProgress
            modelBuilder.Entity<UserProgress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TotalTestsCompleted).HasDefaultValue(0);
                entity.Property(e => e.AverageScore).HasDefaultValue(0).HasPrecision(3, 1);
                entity.Property(e => e.BestScore).HasDefaultValue(0).HasPrecision(3, 1);
                entity.Property(e => e.TotalTimeSpentMinutes).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");
                
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            // Configure ReadingSection
            modelBuilder.Entity<ReadingSection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                
                entity.HasIndex(e => e.SectionOrder);
                entity.HasIndex(e => e.IsActive);
            });

            // Seed initial data
            SeedData(modelBuilder);
            SeedData.SeedQuestions(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Reading Sections
            modelBuilder.Entity<ReadingSection>().HasData(
                new ReadingSection { Id = 1, Title = "Section 1", Description = "Academic Reading Section 1", SectionOrder = 1 },
                new ReadingSection { Id = 2, Title = "Section 2", Description = "Academic Reading Section 2", SectionOrder = 2 },
                new ReadingSection { Id = 3, Title = "Section 3", Description = "Academic Reading Section 3", SectionOrder = 3 }
            );

            // Seed sample reading passage
            modelBuilder.Entity<ReadingPassage>().HasData(
                new ReadingPassage
                {
                    Id = 1,
                    Title = "The Impact of Climate Change on Marine Ecosystems",
                    Content = @"Climate change represents one of the most significant environmental challenges of our time, with far-reaching consequences for marine ecosystems worldwide. Rising global temperatures have led to increased ocean temperatures, altered precipitation patterns, and rising sea levels, all of which have profound impacts on marine life and ocean chemistry.

Ocean acidification, often referred to as the 'other CO2 problem,' occurs when the ocean absorbs carbon dioxide from the atmosphere. As CO2 dissolves in seawater, it forms carbonic acid, which lowers the pH of the ocean. This process has accelerated dramatically since the Industrial Revolution, with ocean pH dropping by approximately 0.1 units, representing a 30% increase in acidity.

The consequences of ocean acidification are particularly severe for marine organisms that build shells or skeletons from calcium carbonate, such as corals, mollusks, and certain plankton species. As ocean acidity increases, it becomes more difficult for these organisms to extract the carbonate ions they need to build and maintain their calcium carbonate structures. This can lead to weaker shells, slower growth rates, and in extreme cases, the dissolution of existing structures.

Coral reefs, often called the 'rainforests of the sea,' are among the most vulnerable ecosystems to climate change. Rising ocean temperatures cause coral bleaching, a stress response in which corals expel the symbiotic algae that provide them with nutrients and their vibrant colors. While corals can recover from short-term bleaching events, prolonged exposure to elevated temperatures can lead to coral death. The Great Barrier Reef has experienced several mass bleaching events in recent years, with significant portions of the reef system affected.

Marine food webs are also being disrupted by climate change. Phytoplankton, the microscopic plants that form the base of most marine food chains, are sensitive to changes in temperature, light availability, and nutrient distribution. As ocean temperatures rise and currents shift, the distribution and abundance of phytoplankton communities are changing, with cascading effects throughout the food web.

Fish populations are responding to changing ocean conditions by shifting their geographic ranges. Many species are moving toward the poles as they follow their preferred temperature ranges. This redistribution of marine life has significant implications for fishing communities and economies that depend on specific fish stocks. Some regions may see new species arrive while traditional fishing grounds may become less productive.

Sea level rise, caused by thermal expansion of seawater and melting ice sheets, threatens coastal ecosystems such as salt marshes, mangroves, and estuaries. These environments serve as crucial nursery habitats for many marine species and provide important ecosystem services such as storm protection and water filtration.

The interconnected nature of marine ecosystems means that changes in one component can have far-reaching effects throughout the system. Scientists are working to understand these complex relationships and develop strategies to help marine ecosystems adapt to changing conditions. Conservation efforts, including the establishment of marine protected areas and reduction of other stressors such as pollution and overfishing, can help build resilience in marine ecosystems.

Addressing climate change requires global cooperation and immediate action to reduce greenhouse gas emissions. While the ocean has absorbed much of the excess heat and CO2 from human activities, this has come at a significant cost to marine life. Understanding and protecting marine ecosystems is not only crucial for the health of our oceans but also for the billions of people who depend on them for food, livelihoods, and climate regulation.",
                    Source = "Environmental Science Journal",
                    DifficultyLevel = DifficultyLevel.Intermediate,
                    WordCount = 2456,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}