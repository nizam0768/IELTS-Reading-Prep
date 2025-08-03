using Microsoft.EntityFrameworkCore;
using IELTSPracticeApp.Data;
using IELTSPracticeApp.Data.Models;

namespace IELTSPracticeApp.Services
{
    public class TestSessionService
    {
        private readonly IELTSDbContext _context;
        private readonly IELTSService _ieltsService;

        public TestSessionService(IELTSDbContext context, IELTSService ieltsService)
        {
            _context = context;
            _ieltsService = ieltsService;
        }

        // Start a new test session
        public async Task<TestSession> StartTestSessionAsync(string userId, int readingPassageId)
        {
            var passage = await _context.ReadingPassages
                .Include(rp => rp.Questions.Where(q => q.IsActive))
                .FirstOrDefaultAsync(rp => rp.Id == readingPassageId && rp.IsActive);

            if (passage == null)
                throw new ArgumentException("Reading passage not found or inactive");

            var testSession = new TestSession
            {
                UserId = userId,
                ReadingPassageId = readingPassageId,
                StartTime = DateTime.UtcNow,
                Status = TestSessionStatus.InProgress,
                TotalQuestions = passage.Questions.Count,
                TimeAllowedSeconds = 3600 // 60 minutes
            };

            _context.TestSessions.Add(testSession);
            await _context.SaveChangesAsync();

            return testSession;
        }

        // Get active test session for user
        public async Task<TestSession?> GetActiveTestSessionAsync(string userId)
        {
            return await _context.TestSessions
                .Include(ts => ts.ReadingPassage)
                .ThenInclude(rp => rp.Questions.Where(q => q.IsActive))
                .Include(ts => ts.UserAnswers)
                .ThenInclude(ua => ua.Question)
                .FirstOrDefaultAsync(ts => ts.UserId == userId && 
                                          ts.Status == TestSessionStatus.InProgress);
        }

        // Get test session by ID
        public async Task<TestSession?> GetTestSessionAsync(int sessionId)
        {
            return await _context.TestSessions
                .Include(ts => ts.ReadingPassage)
                .ThenInclude(rp => rp.Questions.Where(q => q.IsActive))
                .Include(ts => ts.UserAnswers)
                .ThenInclude(ua => ua.Question)
                .FirstOrDefaultAsync(ts => ts.Id == sessionId);
        }

        // Update test session time
        public async Task UpdateSessionTimeAsync(int sessionId, int timeSpentSeconds)
        {
            var session = await _context.TestSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.TimeSpentSeconds = timeSpentSeconds;
                await _context.SaveChangesAsync();
            }
        }

        // Submit an answer
        public async Task<UserAnswer> SubmitAnswerAsync(int sessionId, int questionId, string userResponse)
        {
            var session = await GetTestSessionAsync(sessionId);
            if (session == null || session.Status != TestSessionStatus.InProgress)
                throw new InvalidOperationException("Invalid or inactive test session");

            var question = await _ieltsService.GetQuestionAsync(questionId);
            if (question == null)
                throw new ArgumentException("Question not found");

            // Check if answer already exists
            var existingAnswer = await _context.UserAnswers
                .FirstOrDefaultAsync(ua => ua.TestSessionId == sessionId && ua.QuestionId == questionId);

            var isCorrect = _ieltsService.ValidateAnswer(question, userResponse);
            var pointsAwarded = isCorrect ? question.Points : 0;

            if (existingAnswer != null)
            {
                // Update existing answer
                existingAnswer.UserResponse = userResponse;
                existingAnswer.IsCorrect = isCorrect;
                existingAnswer.PointsAwarded = pointsAwarded;
                existingAnswer.AnsweredAt = DateTime.UtcNow;
            }
            else
            {
                // Create new answer
                existingAnswer = new UserAnswer
                {
                    TestSessionId = sessionId,
                    QuestionId = questionId,
                    UserResponse = userResponse,
                    IsCorrect = isCorrect,
                    PointsAwarded = pointsAwarded,
                    AnsweredAt = DateTime.UtcNow
                };
                _context.UserAnswers.Add(existingAnswer);
            }

            await _context.SaveChangesAsync();
            return existingAnswer;
        }

        // Complete test session
        public async Task<TestSession> CompleteTestSessionAsync(int sessionId)
        {
            var session = await GetTestSessionAsync(sessionId);
            if (session == null)
                throw new ArgumentException("Test session not found");

            // Calculate final score
            var correctAnswers = session.UserAnswers.Count(ua => ua.IsCorrect);
            var totalQuestions = session.TotalQuestions;
            var bandScore = _ieltsService.CalculateBandScore(correctAnswers, totalQuestions);

            session.Status = TestSessionStatus.Completed;
            session.EndTime = DateTime.UtcNow;
            session.CorrectAnswers = correctAnswers;
            session.Score = bandScore;

            // Update final time spent
            if (session.EndTime.HasValue)
            {
                session.TimeSpentSeconds = (int)(session.EndTime.Value - session.StartTime).TotalSeconds;
            }

            await _context.SaveChangesAsync();

            // Update user progress
            var timeSpentMinutes = session.TimeSpentSeconds / 60;
            await _ieltsService.UpdateUserProgressAsync(session.UserId, bandScore, timeSpentMinutes);

            return session;
        }

        // Pause test session
        public async Task PauseTestSessionAsync(int sessionId)
        {
            var session = await _context.TestSessions.FindAsync(sessionId);
            if (session != null && session.Status == TestSessionStatus.InProgress)
            {
                session.Status = TestSessionStatus.Paused;
                await _context.SaveChangesAsync();
            }
        }

        // Resume test session
        public async Task ResumeTestSessionAsync(int sessionId)
        {
            var session = await _context.TestSessions.FindAsync(sessionId);
            if (session != null && session.Status == TestSessionStatus.Paused)
            {
                session.Status = TestSessionStatus.InProgress;
                await _context.SaveChangesAsync();
            }
        }

        // Get remaining time for session
        public int GetRemainingTimeSeconds(TestSession session)
        {
            if (session.Status != TestSessionStatus.InProgress)
                return 0;

            var elapsed = (int)(DateTime.UtcNow - session.StartTime).TotalSeconds;
            return Math.Max(0, session.TimeAllowedSeconds - elapsed);
        }

        // Check if session has expired
        public bool IsSessionExpired(TestSession session)
        {
            return GetRemainingTimeSeconds(session) <= 0;
        }

        // Auto-complete expired sessions
        public async Task AutoCompleteExpiredSessionsAsync()
        {
            var expiredSessions = await _context.TestSessions
                .Where(ts => ts.Status == TestSessionStatus.InProgress)
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                if (IsSessionExpired(session))
                {
                    await CompleteTestSessionAsync(session.Id);
                }
            }
        }

        // Get user's test history
        public async Task<List<TestSession>> GetUserTestHistoryAsync(string userId, int pageSize = 10, int page = 1)
        {
            return await _context.TestSessions
                .Include(ts => ts.ReadingPassage)
                .Where(ts => ts.UserId == userId && ts.Status == TestSessionStatus.Completed)
                .OrderByDescending(ts => ts.EndTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get detailed test results
        public async Task<object> GetTestResultsAsync(int sessionId)
        {
            var session = await GetTestSessionAsync(sessionId);
            if (session == null)
                return new { Error = "Test session not found" };

            var questionResults = session.UserAnswers.Select(ua => new
            {
                QuestionId = ua.QuestionId,
                QuestionText = ua.Question.QuestionText,
                QuestionType = ua.Question.QuestionType.ToString(),
                UserAnswer = ua.UserResponse,
                CorrectAnswer = ua.Question.CorrectAnswer,
                IsCorrect = ua.IsCorrect,
                PointsAwarded = ua.PointsAwarded,
                TimeSpent = ua.TimeSpentSeconds
            }).ToList();

            var questionTypePerformance = session.UserAnswers
                .GroupBy(ua => ua.Question.QuestionType)
                .Select(g => new
                {
                    QuestionType = g.Key.ToString(),
                    TotalQuestions = g.Count(),
                    CorrectAnswers = g.Count(ua => ua.IsCorrect),
                    Accuracy = g.Count() > 0 ? (double)g.Count(ua => ua.IsCorrect) / g.Count() * 100 : 0
                }).ToList();

            return new
            {
                SessionId = session.Id,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                TimeSpentMinutes = session.TimeSpentSeconds / 60,
                TotalQuestions = session.TotalQuestions,
                CorrectAnswers = session.CorrectAnswers,
                Score = session.Score,
                PassageTitle = session.ReadingPassage.Title,
                QuestionResults = questionResults,
                QuestionTypePerformance = questionTypePerformance
            };
        }
    }
}