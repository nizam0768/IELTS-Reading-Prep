using Microsoft.EntityFrameworkCore;
using IELTSPracticeApp.Data;
using IELTSPracticeApp.Data.Models;
using System.Text.Json;

namespace IELTSPracticeApp.Services
{
    public class IELTSService
    {
        private readonly IELTSDbContext _context;

        public IELTSService(IELTSDbContext context)
        {
            _context = context;
        }

        // Reading Passage Management
        public async Task<List<ReadingPassage>> GetAllReadingPassagesAsync()
        {
            return await _context.ReadingPassages
                .Where(rp => rp.IsActive)
                .Include(rp => rp.Questions.Where(q => q.IsActive))
                .OrderBy(rp => rp.CreatedAt)
                .ToListAsync();
        }

        public async Task<ReadingPassage?> GetReadingPassageAsync(int id)
        {
            return await _context.ReadingPassages
                .Include(rp => rp.Questions.Where(q => q.IsActive))
                .FirstOrDefaultAsync(rp => rp.Id == id && rp.IsActive);
        }

        public async Task<ReadingPassage?> GetReadingPassageWithQuestionsAsync(int id)
        {
            return await _context.ReadingPassages
                .Include(rp => rp.Questions.Where(q => q.IsActive))
                .ThenInclude(q => q.UserAnswers)
                .FirstOrDefaultAsync(rp => rp.Id == id && rp.IsActive);
        }

        // Question Management
        public async Task<List<Question>> GetQuestionsForPassageAsync(int passageId)
        {
            return await _context.Questions
                .Where(q => q.ReadingPassageId == passageId && q.IsActive)
                .OrderBy(q => q.QuestionOrder)
                .ToListAsync();
        }

        public async Task<Question?> GetQuestionAsync(int questionId)
        {
            return await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == questionId && q.IsActive);
        }

        // Answer Validation
        public bool ValidateAnswer(Question question, string userAnswer)
        {
            if (string.IsNullOrWhiteSpace(userAnswer))
                return false;

            var correctAnswer = question.CorrectAnswer.Trim().ToLowerInvariant();
            var userAnswerNormalized = userAnswer.Trim().ToLowerInvariant();

            return question.QuestionType switch
            {
                QuestionType.MultipleChoice => ValidateMultipleChoice(correctAnswer, userAnswerNormalized),
                QuestionType.TrueFalseNotGiven => ValidateTrueFalseNotGiven(correctAnswer, userAnswerNormalized),
                QuestionType.YesNoNotGiven => ValidateYesNoNotGiven(correctAnswer, userAnswerNormalized),
                QuestionType.MatchingInformation => ValidateExactMatch(correctAnswer, userAnswerNormalized),
                QuestionType.MatchingHeadings => ValidateExactMatch(correctAnswer, userAnswerNormalized),
                QuestionType.MatchingFeatures => ValidateExactMatch(correctAnswer, userAnswerNormalized),
                QuestionType.MatchingSentenceEndings => ValidateExactMatch(correctAnswer, userAnswerNormalized),
                QuestionType.SentenceCompletion => ValidateSentenceCompletion(question, correctAnswer, userAnswerNormalized),
                QuestionType.SummaryCompletion => ValidateSummaryCompletion(question, correctAnswer, userAnswerNormalized),
                QuestionType.DiagramLabelCompletion => ValidateDiagramLabel(correctAnswer, userAnswerNormalized),
                QuestionType.ShortAnswerQuestions => ValidateShortAnswer(question, correctAnswer, userAnswerNormalized),
                _ => false
            };
        }

        private bool ValidateMultipleChoice(string correct, string user)
        {
            return correct == user;
        }

        private bool ValidateTrueFalseNotGiven(string correct, string user)
        {
            var validAnswers = new[] { "true", "false", "not given" };
            return validAnswers.Contains(user) && correct == user;
        }

        private bool ValidateYesNoNotGiven(string correct, string user)
        {
            var validAnswers = new[] { "yes", "no", "not given" };
            return validAnswers.Contains(user) && correct == user;
        }

        private bool ValidateExactMatch(string correct, string user)
        {
            return correct == user;
        }

        private bool ValidateSentenceCompletion(Question question, string correct, string user)
        {
            // Check word limit if specified
            if (question.WordLimit.HasValue)
            {
                var wordCount = user.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                if (wordCount > question.WordLimit.Value)
                    return false;
            }

            // Allow multiple correct answers separated by |
            var correctAnswers = correct.Split('|');
            return correctAnswers.Any(ca => ca.Trim() == user);
        }

        private bool ValidateSummaryCompletion(Question question, string correct, string user)
        {
            return ValidateSentenceCompletion(question, correct, user);
        }

        private bool ValidateDiagramLabel(string correct, string user)
        {
            // Allow multiple correct answers separated by |
            var correctAnswers = correct.Split('|');
            return correctAnswers.Any(ca => ca.Trim() == user);
        }

        private bool ValidateShortAnswer(Question question, string correct, string user)
        {
            // Check word limit if specified
            if (question.WordLimit.HasValue)
            {
                var wordCount = user.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                if (wordCount > question.WordLimit.Value)
                    return false;
            }

            // Allow multiple correct answers separated by |
            var correctAnswers = correct.Split('|');
            return correctAnswers.Any(ca => ca.Trim() == user);
        }

        // Scoring System
        public decimal CalculateBandScore(int correctAnswers, int totalQuestions)
        {
            if (totalQuestions == 0) return 0;

            var percentage = (decimal)correctAnswers / totalQuestions * 100;

            return percentage switch
            {
                >= 90 => 9.0m,
                >= 87 => 8.5m,
                >= 83 => 8.0m,
                >= 79 => 7.5m,
                >= 75 => 7.0m,
                >= 70 => 6.5m,
                >= 65 => 6.0m,
                >= 60 => 5.5m,
                >= 55 => 5.0m,
                >= 50 => 4.5m,
                >= 45 => 4.0m,
                >= 40 => 3.5m,
                >= 35 => 3.0m,
                >= 30 => 2.5m,
                >= 25 => 2.0m,
                >= 20 => 1.5m,
                _ => 1.0m
            };
        }

        // User Progress Management
        public async Task<UserProgress?> GetUserProgressAsync(string userId)
        {
            return await _context.UserProgress
                .FirstOrDefaultAsync(up => up.UserId == userId);
        }

        public async Task UpdateUserProgressAsync(string userId, decimal score, int timeSpentMinutes)
        {
            var progress = await GetUserProgressAsync(userId);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    TotalTestsCompleted = 1,
                    AverageScore = score,
                    BestScore = score,
                    TotalTimeSpentMinutes = timeSpentMinutes,
                    LastTestDate = DateTime.UtcNow
                };
                _context.UserProgress.Add(progress);
            }
            else
            {
                progress.TotalTestsCompleted++;
                progress.AverageScore = ((progress.AverageScore * (progress.TotalTestsCompleted - 1)) + score) / progress.TotalTestsCompleted;
                progress.BestScore = Math.Max(progress.BestScore, score);
                progress.TotalTimeSpentMinutes += timeSpentMinutes;
                progress.LastTestDate = DateTime.UtcNow;
                progress.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // Generate User ID for anonymous users
        public string GenerateAnonymousUserId(string? ipAddress = null)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var random = new Random().Next(1000, 9999);
            var hash = ipAddress?.GetHashCode().ToString("X") ?? "ANON";
            return $"USER_{hash}_{timestamp}_{random}";
        }

        // Get test statistics
        public async Task<object> GetTestStatisticsAsync()
        {
            var totalTests = await _context.TestSessions.CountAsync(ts => ts.Status == TestSessionStatus.Completed);
            var averageScore = await _context.TestSessions
                .Where(ts => ts.Status == TestSessionStatus.Completed)
                .AverageAsync(ts => (double?)ts.Score) ?? 0;
            var totalUsers = await _context.UserProgress.CountAsync();

            return new
            {
                TotalTests = totalTests,
                AverageScore = Math.Round(averageScore, 1),
                TotalUsers = totalUsers
            };
        }
    }
}