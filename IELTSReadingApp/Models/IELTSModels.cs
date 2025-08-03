using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IELTSReadingApp.Models
{
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalseNotGiven,
        YesNoNotGiven,
        MatchingInformation,
        MatchingHeadings,
        MatchingFeatures,
        MatchingSentenceEndings,
        SentenceCompletion,
        SummaryCompletion,
        DiagramLabelCompletion,
        ShortAnswerQuestions
    }

    public enum DifficultyLevel
    {
        Beginner,
        Intermediate,
        Advanced
    }

    [Table("ReadingPassages")]
    public class ReadingPassage
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Source { get; set; } = string.Empty;
        
        public int WordCount { get; set; }
        
        [NotMapped]
        public List<string> Paragraphs { get; set; } = new();
        
        // Store paragraphs as JSON string in database
        [Column("ParagraphsJson")]
        public string ParagraphsJson { get; set; } = "[]";
        
        [MaxLength(500)]
        public string? DiagramUrl { get; set; }
        
        [NotMapped]
        public Dictionary<string, string> Vocabulary { get; set; } = new();
        
        // Store vocabulary as JSON string in database
        [Column("VocabularyJson")]
        public string VocabularyJson { get; set; } = "{}";
        
        // Navigation property
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }

    [Table("Questions")]
    public class Question
    {
        [Key]
        public int Id { get; set; }
        
        public int QuestionNumber { get; set; }
        
        [Required]
        public QuestionType Type { get; set; }
        
        [Required]
        public string QuestionText { get; set; } = string.Empty;
        
        [NotMapped]
        public List<string> Options { get; set; } = new();
        
        // Store options as JSON string in database
        [Column("OptionsJson")]
        public string OptionsJson { get; set; } = "[]";
        
        [Required]
        [MaxLength(500)]
        public string CorrectAnswer { get; set; } = string.Empty;
        
        public string Explanation { get; set; } = string.Empty;
        
        // Foreign key
        public int PassageId { get; set; }
        
        [MaxLength(10)]
        public string? ParagraphReference { get; set; }
        
        public int MaxWords { get; set; } = 0;
        
        // Navigation property
        [ForeignKey("PassageId")]
        public virtual ReadingPassage Passage { get; set; } = null!;
        
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }

    [Table("ReadingSections")]
    public class ReadingSection
    {
        [Key]
        public int Id { get; set; }
        
        public int SectionNumber { get; set; }
        
        // Foreign key
        public int PassageId { get; set; }
        
        public int TimeAllocation { get; set; } = 20;
        
        public string Instructions { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("PassageId")]
        public virtual ReadingPassage Passage { get; set; } = null!;
        
        [NotMapped]
        public List<Question> Questions { get; set; } = new();
    }

    [Table("ReadingTests")]
    public class ReadingTest
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DifficultyLevel Difficulty { get; set; }
        
        [NotMapped]
        public List<ReadingSection> Sections { get; set; } = new();
        
        public int TotalTimeMinutes { get; set; } = 60;
        
        public int TotalQuestions { get; set; } = 40;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<TestSection> TestSections { get; set; } = new List<TestSection>();
        public virtual ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
        public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }

    // Junction table for Many-to-Many relationship between Tests and Sections
    [Table("TestSections")]
    public class TestSection
    {
        [Key]
        public int Id { get; set; }
        
        public int TestId { get; set; }
        public int SectionId { get; set; }
        public int SectionOrder { get; set; }
        
        [ForeignKey("TestId")]
        public virtual ReadingTest Test { get; set; } = null!;
        
        [ForeignKey("SectionId")]
        public virtual ReadingSection Section { get; set; } = null!;
    }

    [Table("UserAnswers")]
    public class UserAnswer
    {
        [Key]
        public int Id { get; set; }
        
        public int QuestionId { get; set; }
        
        [MaxLength(1000)]
        public string Answer { get; set; } = string.Empty;
        
        public bool IsCorrect { get; set; }
        
        public DateTime AnsweredAt { get; set; }
        
        // Foreign key to test session
        public int? TestSessionId { get; set; }
        
        // Navigation properties
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;
        
        [ForeignKey("TestSessionId")]
        public virtual TestSession? TestSession { get; set; }
    }

    [Table("TestResults")]
    public class TestResult
    {
        [Key]
        public int Id { get; set; }
        
        public int TestId { get; set; }
        
        [NotMapped]
        public List<UserAnswer> Answers { get; set; } = new();
        
        public int CorrectAnswers { get; set; }
        
        public int TotalQuestions { get; set; }
        
        public double Score { get; set; }
        
        public double BandScore { get; set; }
        
        public TimeSpan TimeTaken { get; set; }
        
        public DateTime CompletedAt { get; set; }
        
        [NotMapped]
        public Dictionary<QuestionType, int> ScoresByType { get; set; } = new();
        
        // Store scores by type as JSON string in database
        [Column("ScoresByTypeJson")]
        public string ScoresByTypeJson { get; set; } = "{}";
        
        // Foreign key to test session
        public int? TestSessionId { get; set; }
        
        // Navigation properties
        [ForeignKey("TestId")]
        public virtual ReadingTest Test { get; set; } = null!;
        
        [ForeignKey("TestSessionId")]
        public virtual TestSession? TestSession { get; set; }
    }

    [Table("TestSessions")]
    public class TestSession
    {
        [Key]
        public int Id { get; set; }
        
        public int TestId { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        [NotMapped]
        public List<UserAnswer> CurrentAnswers { get; set; } = new();
        
        public int CurrentSectionIndex { get; set; } = 0;
        
        public bool IsCompleted { get; set; }
        
        public TimeSpan RemainingTime { get; set; }
        
        // User identifier (could be extended to support actual user authentication)
        [MaxLength(100)]
        public string UserId { get; set; } = "anonymous";
        
        // Navigation properties
        [ForeignKey("TestId")]
        public virtual ReadingTest Test { get; set; } = null!;
        
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        public virtual TestResult? TestResult { get; set; }
    }

    public static class BandScoreCalculator
    {
        private static readonly Dictionary<int, double> ScoreToBandMapping = new()
        {
            { 39, 9.0 }, { 38, 8.5 }, { 37, 8.5 }, { 36, 8.0 }, { 35, 8.0 },
            { 34, 7.5 }, { 33, 7.5 }, { 32, 7.0 }, { 31, 7.0 }, { 30, 6.5 },
            { 29, 6.5 }, { 28, 6.5 }, { 27, 6.0 }, { 26, 6.0 }, { 25, 6.0 },
            { 24, 5.5 }, { 23, 5.5 }, { 22, 5.5 }, { 21, 5.0 }, { 20, 5.0 },
            { 19, 5.0 }, { 18, 4.5 }, { 17, 4.5 }, { 16, 4.0 }, { 15, 4.0 },
            { 14, 4.0 }, { 13, 3.5 }, { 12, 3.5 }, { 11, 3.0 }, { 10, 3.0 },
            { 9, 2.5 }, { 8, 2.5 }, { 7, 2.0 }, { 6, 2.0 }, { 5, 1.5 },
            { 4, 1.0 }, { 3, 1.0 }, { 2, 1.0 }, { 1, 1.0 }, { 0, 1.0 }
        };

        public static double CalculateBandScore(int correctAnswers)
        {
            return ScoreToBandMapping.GetValueOrDefault(correctAnswers, 1.0);
        }
    }
}