using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IELTSPracticeApp.Data.Models
{
    // Enums for different question types and statuses
    public enum QuestionType
    {
        MultipleChoice = 1,
        TrueFalseNotGiven = 2,
        YesNoNotGiven = 3,
        MatchingInformation = 4,
        MatchingHeadings = 5,
        MatchingFeatures = 6,
        MatchingSentenceEndings = 7,
        SentenceCompletion = 8,
        SummaryCompletion = 9,
        DiagramLabelCompletion = 10,
        ShortAnswerQuestions = 11
    }

    public enum TestSessionStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Paused = 3
    }

    public enum DifficultyLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3
    }

    // Main reading passage entity
    public class ReadingPassage
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public string? Source { get; set; }
        
        public DifficultyLevel DifficultyLevel { get; set; }
        
        [Range(2150, 2750)]
        public int WordCount { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
    }

    // Question entity supporting all IELTS question types
    public class Question
    {
        public int Id { get; set; }
        
        public int ReadingPassageId { get; set; }
        
        [Required]
        public string QuestionText { get; set; } = string.Empty;
        
        public QuestionType QuestionType { get; set; }
        
        public int QuestionOrder { get; set; }
        
        public string? Instructions { get; set; }
        
        // For multiple choice, matching, etc.
        public string? Options { get; set; } // JSON serialized options
        
        // Correct answer(s)
        [Required]
        public string CorrectAnswer { get; set; } = string.Empty;
        
        // For questions with word limits
        public int? WordLimit { get; set; }
        
        // Points awarded for correct answer
        [Range(1, 5)]
        public int Points { get; set; } = 1;
        
        // Additional metadata for complex question types
        public string? Metadata { get; set; } // JSON for storing additional data
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ReadingPassage ReadingPassage { get; set; } = null!;
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }

    // Test session entity
    public class TestSession
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty; // Can be IP or session ID for anonymous users
        
        public int ReadingPassageId { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public TestSessionStatus Status { get; set; } = TestSessionStatus.NotStarted;
        
        // Time spent in seconds
        public int TimeSpentSeconds { get; set; } = 0;
        
        // Total time allowed (60 minutes = 3600 seconds)
        public int TimeAllowedSeconds { get; set; } = 3600;
        
        public int TotalQuestions { get; set; }
        
        public int CorrectAnswers { get; set; } = 0;
        
        public decimal Score { get; set; } = 0; // Band score (0-9)
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ReadingPassage ReadingPassage { get; set; } = null!;
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }

    // User answers entity
    public class UserAnswer
    {
        public int Id { get; set; }
        
        public int TestSessionId { get; set; }
        
        public int QuestionId { get; set; }
        
        [Required]
        public string UserResponse { get; set; } = string.Empty;
        
        public bool IsCorrect { get; set; }
        
        public int PointsAwarded { get; set; } = 0;
        
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
        
        // Time spent on this question in seconds
        public int TimeSpentSeconds { get; set; } = 0;
        
        // Navigation properties
        public virtual TestSession TestSession { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
    }

    // User progress tracking
    public class UserProgress
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        public int TotalTestsCompleted { get; set; } = 0;
        
        public decimal AverageScore { get; set; } = 0;
        
        public decimal BestScore { get; set; } = 0;
        
        public int TotalTimeSpentMinutes { get; set; } = 0;
        
        public DateTime LastTestDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Question type performance tracking (JSON)
        public string? QuestionTypePerformance { get; set; }
    }

    // Question options for multiple choice and matching questions
    public class QuestionOption
    {
        public int Id { get; set; }
        
        public int QuestionId { get; set; }
        
        [Required]
        public string OptionText { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string OptionKey { get; set; } = string.Empty; // A, B, C, D, etc.
        
        public int OptionOrder { get; set; }
        
        public bool IsCorrect { get; set; } = false;
        
        // Navigation properties
        public virtual Question Question { get; set; } = null!;
    }

    // Reading sections for organizing passages
    public class ReadingSection
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public int SectionOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<ReadingPassage> ReadingPassages { get; set; } = new List<ReadingPassage>();
    }
}