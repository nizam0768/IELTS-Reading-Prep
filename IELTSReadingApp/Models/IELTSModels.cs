using System.ComponentModel.DataAnnotations;

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

    public class ReadingPassage
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public List<string> Paragraphs { get; set; } = new();
        public string? DiagramUrl { get; set; }
        public Dictionary<string, string> Vocabulary { get; set; } = new();
    }

    public class Question
    {
        public int Id { get; set; }
        public int QuestionNumber { get; set; }
        public QuestionType Type { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public int PassageId { get; set; }
        public string? ParagraphReference { get; set; }
        public int MaxWords { get; set; } = 0; // For completion questions
    }

    public class ReadingSection
    {
        public int Id { get; set; }
        public int SectionNumber { get; set; }
        public ReadingPassage Passage { get; set; } = new();
        public List<Question> Questions { get; set; } = new();
        public int TimeAllocation { get; set; } = 20; // minutes
        public string Instructions { get; set; } = string.Empty;
    }

    public class ReadingTest
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DifficultyLevel Difficulty { get; set; }
        public List<ReadingSection> Sections { get; set; } = new();
        public int TotalTimeMinutes { get; set; } = 60;
        public int TotalQuestions { get; set; } = 40;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Category { get; set; } = string.Empty;
    }

    public class UserAnswer
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public DateTime AnsweredAt { get; set; }
    }

    public class TestResult
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public List<UserAnswer> Answers { get; set; } = new();
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double Score { get; set; }
        public double BandScore { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public DateTime CompletedAt { get; set; }
        public Dictionary<QuestionType, int> ScoresByType { get; set; } = new();
    }

    public class TestSession
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<UserAnswer> CurrentAnswers { get; set; } = new();
        public int CurrentSectionIndex { get; set; } = 0;
        public bool IsCompleted { get; set; }
        public TimeSpan RemainingTime { get; set; }
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