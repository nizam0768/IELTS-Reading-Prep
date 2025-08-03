using Microsoft.EntityFrameworkCore;
using IELTSReadingApp.Models;
using System.Text.Json;

namespace IELTSReadingApp.Data
{
    public class IELTSDbContext : DbContext
    {
        public IELTSDbContext(DbContextOptions<IELTSDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<ReadingTest> ReadingTests { get; set; }
        public DbSet<ReadingSection> ReadingSections { get; set; }
        public DbSet<ReadingPassage> ReadingPassages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<TestSection> TestSections { get; set; }
        public DbSet<TestSession> TestSessions { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships and constraints
            ConfigureReadingPassage(modelBuilder);
            ConfigureQuestion(modelBuilder);
            ConfigureReadingSection(modelBuilder);
            ConfigureReadingTest(modelBuilder);
            ConfigureTestSection(modelBuilder);
            ConfigureTestSession(modelBuilder);
            ConfigureUserAnswer(modelBuilder);
            ConfigureTestResult(modelBuilder);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void ConfigureReadingPassage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReadingPassage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Source).HasMaxLength(200);
                entity.Property(e => e.DiagramUrl).HasMaxLength(500);
                entity.Property(e => e.ParagraphsJson).HasDefaultValue("[]");
                entity.Property(e => e.VocabularyJson).HasDefaultValue("{}");

                // Configure one-to-many relationship with Questions
                entity.HasMany(e => e.Questions)
                      .WithOne(e => e.Passage)
                      .HasForeignKey(e => e.PassageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureQuestion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionText).IsRequired();
                entity.Property(e => e.CorrectAnswer).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ParagraphReference).HasMaxLength(10);
                entity.Property(e => e.OptionsJson).HasDefaultValue("[]");
                entity.Property(e => e.Type).HasConversion<string>();

                // Configure one-to-many relationship with UserAnswers
                entity.HasMany(e => e.UserAnswers)
                      .WithOne(e => e.Question)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureReadingSection(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReadingSection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TimeAllocation).HasDefaultValue(20);

                // Configure one-to-one relationship with ReadingPassage
                entity.HasOne(e => e.Passage)
                      .WithMany()
                      .HasForeignKey(e => e.PassageId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureReadingTest(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReadingTest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Difficulty).HasConversion<string>();
                entity.Property(e => e.TotalTimeMinutes).HasDefaultValue(60);
                entity.Property(e => e.TotalQuestions).HasDefaultValue(40);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");

                // Configure one-to-many relationships
                entity.HasMany(e => e.TestSections)
                      .WithOne(e => e.Test)
                      .HasForeignKey(e => e.TestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.TestSessions)
                      .WithOne(e => e.Test)
                      .HasForeignKey(e => e.TestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.TestResults)
                      .WithOne(e => e.Test)
                      .HasForeignKey(e => e.TestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureTestSection(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestSection>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Configure composite unique index
                entity.HasIndex(e => new { e.TestId, e.SectionId, e.SectionOrder })
                      .IsUnique();

                // Configure relationships
                entity.HasOne(e => e.Test)
                      .WithMany(e => e.TestSections)
                      .HasForeignKey(e => e.TestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Section)
                      .WithMany()
                      .HasForeignKey(e => e.SectionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureTestSession(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(100).HasDefaultValue("anonymous");
                entity.Property(e => e.CurrentSectionIndex).HasDefaultValue(0);
                entity.Property(e => e.IsCompleted).HasDefaultValue(false);
                entity.Property(e => e.StartTime).HasDefaultValueSql("GETDATE()");

                // Configure one-to-many relationship with UserAnswers
                entity.HasMany(e => e.UserAnswers)
                      .WithOne(e => e.TestSession)
                      .HasForeignKey(e => e.TestSessionId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Configure one-to-one relationship with TestResult
                entity.HasOne(e => e.TestResult)
                      .WithOne(e => e.TestSession)
                      .HasForeignKey<TestResult>(e => e.TestSessionId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private void ConfigureUserAnswer(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Answer).HasMaxLength(1000);
                entity.Property(e => e.IsCorrect).HasDefaultValue(false);
                entity.Property(e => e.AnsweredAt).HasDefaultValueSql("GETDATE()");
            });
        }

        private void ConfigureTestResult(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ScoresByTypeJson).HasDefaultValue("{}");
                entity.Property(e => e.CompletedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Score).HasPrecision(5, 2);
                entity.Property(e => e.BandScore).HasPrecision(3, 1);
            });
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed sample reading passages
            modelBuilder.Entity<ReadingPassage>().HasData(
                new ReadingPassage
                {
                    Id = 1,
                    Title = "The Solar Revolution",
                    Source = "Environmental Science Journal",
                    WordCount = 850,
                    Content = @"The transition to renewable energy has become one of the most critical challenges of the 21st century. Solar power, in particular, has emerged as a leading solution in the fight against climate change. Over the past decade, the cost of solar panels has decreased by more than 80%, making solar energy increasingly competitive with traditional fossil fuels.

Solar photovoltaic (PV) technology converts sunlight directly into electricity through the photovoltaic effect. When photons from sunlight strike the solar cells, they knock electrons loose from their atoms, generating an electric current. This process is completely silent and produces no harmful emissions, making it an environmentally friendly alternative to coal and natural gas power plants.

The efficiency of modern solar panels has improved dramatically. While early solar panels achieved efficiency rates of only 6-8%, today's commercial panels routinely achieve 20-22% efficiency, with some laboratory demonstrations reaching over 40%. This improvement has been driven by advances in materials science and manufacturing techniques.

However, solar energy faces several challenges. The intermittent nature of sunlight means that solar power generation varies throughout the day and across seasons. Energy storage solutions, such as lithium-ion batteries, are becoming increasingly important to address this issue. Additionally, the manufacturing of solar panels requires significant energy input and involves the use of some toxic materials, though the environmental benefits far outweigh these concerns over the panel's 25-30 year lifespan.

Countries around the world are implementing various policies to promote solar energy adoption. Feed-in tariffs, tax incentives, and renewable energy standards have all contributed to the rapid growth of the solar industry. China has become the world's largest producer of solar panels, while countries like Germany and Denmark have achieved high levels of renewable energy integration in their electrical grids.",
                    ParagraphsJson = JsonSerializer.Serialize(new List<string>
                    {
                        "The transition to renewable energy has become one of the most critical challenges of the 21st century. Solar power, in particular, has emerged as a leading solution in the fight against climate change. Over the past decade, the cost of solar panels has decreased by more than 80%, making solar energy increasingly competitive with traditional fossil fuels.",
                        "Solar photovoltaic (PV) technology converts sunlight directly into electricity through the photovoltaic effect. When photons from sunlight strike the solar cells, they knock electrons loose from their atoms, generating an electric current. This process is completely silent and produces no harmful emissions, making it an environmentally friendly alternative to coal and natural gas power plants.",
                        "The efficiency of modern solar panels has improved dramatically. While early solar panels achieved efficiency rates of only 6-8%, today's commercial panels routinely achieve 20-22% efficiency, with some laboratory demonstrations reaching over 40%. This improvement has been driven by advances in materials science and manufacturing techniques.",
                        "However, solar energy faces several challenges. The intermittent nature of sunlight means that solar power generation varies throughout the day and across seasons. Energy storage solutions, such as lithium-ion batteries, are becoming increasingly important to address this issue. Additionally, the manufacturing of solar panels requires significant energy input and involves the use of some toxic materials, though the environmental benefits far outweigh these concerns over the panel's 25-30 year lifespan.",
                        "Countries around the world are implementing various policies to promote solar energy adoption. Feed-in tariffs, tax incentives, and renewable energy standards have all contributed to the rapid growth of the solar industry. China has become the world's largest producer of solar panels, while countries like Germany and Denmark have achieved high levels of renewable energy integration in their electrical grids."
                    }),
                    VocabularyJson = JsonSerializer.Serialize(new Dictionary<string, string>
                    {
                        {"photovoltaic", "relating to the production of electric current at the junction of two substances exposed to light"},
                        {"intermittent", "occurring at irregular intervals; not continuous or steady"},
                        {"feed-in tariffs", "policy mechanisms designed to accelerate investment in renewable energy technologies"}
                    })
                }
            );

            // Seed sample reading sections
            modelBuilder.Entity<ReadingSection>().HasData(
                new ReadingSection
                {
                    Id = 1,
                    SectionNumber = 1,
                    PassageId = 1,
                    Instructions = "Read the passage and answer questions 1-13.",
                    TimeAllocation = 20
                }
            );

            // Seed sample reading test
            modelBuilder.Entity<ReadingTest>().HasData(
                new ReadingTest
                {
                    Id = 1,
                    Title = "Climate Change and Renewable Energy",
                    Description = "Academic reading test focusing on environmental science and sustainable energy solutions",
                    Difficulty = DifficultyLevel.Intermediate,
                    Category = "Environment & Science",
                    TotalTimeMinutes = 60,
                    TotalQuestions = 40,
                    CreatedDate = DateTime.Now
                }
            );

            // Seed test sections relationship
            modelBuilder.Entity<TestSection>().HasData(
                new TestSection
                {
                    Id = 1,
                    TestId = 1,
                    SectionId = 1,
                    SectionOrder = 1
                }
            );

            // Seed sample questions
            modelBuilder.Entity<Question>().HasData(
                new Question
                {
                    Id = 1,
                    QuestionNumber = 1,
                    Type = QuestionType.MultipleChoice,
                    QuestionText = "According to the passage, over the past decade, the cost of solar panels has:",
                    OptionsJson = JsonSerializer.Serialize(new List<string> 
                    { 
                        "A) increased by 80%", 
                        "B) decreased by more than 80%", 
                        "C) remained stable", 
                        "D) fluctuated significantly" 
                    }),
                    CorrectAnswer = "B",
                    Explanation = "The passage states that 'Over the past decade, the cost of solar panels has decreased by more than 80%'.",
                    PassageId = 1
                },
                new Question
                {
                    Id = 2,
                    QuestionNumber = 2,
                    Type = QuestionType.TrueFalseNotGiven,
                    QuestionText = "Solar panels produce noise during operation.",
                    OptionsJson = JsonSerializer.Serialize(new List<string>()),
                    CorrectAnswer = "False",
                    Explanation = "The passage states that the solar process 'is completely silent'.",
                    PassageId = 1
                }
            );
        }

        // Override SaveChanges to handle JSON serialization
        public override int SaveChanges()
        {
            HandleJsonSerialization();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleJsonSerialization();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void HandleJsonSerialization()
        {
            var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Handle ReadingPassage JSON serialization
                if (entry.Entity is ReadingPassage passage)
                {
                    passage.ParagraphsJson = JsonSerializer.Serialize(passage.Paragraphs);
                    passage.VocabularyJson = JsonSerializer.Serialize(passage.Vocabulary);
                }

                // Handle Question JSON serialization
                if (entry.Entity is Question question)
                {
                    question.OptionsJson = JsonSerializer.Serialize(question.Options);
                }

                // Handle TestResult JSON serialization
                if (entry.Entity is TestResult result)
                {
                    result.ScoresByTypeJson = JsonSerializer.Serialize(result.ScoresByType);
                }
            }
        }

        // Helper method to deserialize JSON fields when loading entities
        public void DeserializeJsonFields()
        {
            // Deserialize ReadingPassage JSON fields
            foreach (var passage in ReadingPassages.Local)
            {
                if (!string.IsNullOrEmpty(passage.ParagraphsJson) && passage.ParagraphsJson != "[]")
                {
                    passage.Paragraphs = JsonSerializer.Deserialize<List<string>>(passage.ParagraphsJson) ?? new List<string>();
                }
                if (!string.IsNullOrEmpty(passage.VocabularyJson) && passage.VocabularyJson != "{}")
                {
                    passage.Vocabulary = JsonSerializer.Deserialize<Dictionary<string, string>>(passage.VocabularyJson) ?? new Dictionary<string, string>();
                }
            }

            // Deserialize Question JSON fields
            foreach (var question in Questions.Local)
            {
                if (!string.IsNullOrEmpty(question.OptionsJson) && question.OptionsJson != "[]")
                {
                    question.Options = JsonSerializer.Deserialize<List<string>>(question.OptionsJson) ?? new List<string>();
                }
            }

            // Deserialize TestResult JSON fields
            foreach (var result in TestResults.Local)
            {
                if (!string.IsNullOrEmpty(result.ScoresByTypeJson) && result.ScoresByTypeJson != "{}")
                {
                    result.ScoresByType = JsonSerializer.Deserialize<Dictionary<QuestionType, int>>(result.ScoresByTypeJson) ?? new Dictionary<QuestionType, int>();
                }
            }
        }
    }
}