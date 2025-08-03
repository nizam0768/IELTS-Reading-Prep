using Microsoft.EntityFrameworkCore;
using IELTSReadingApp.Data;
using IELTSReadingApp.Models;
using System.Text.Json;

namespace IELTSReadingApp.Services
{
    public interface IIELTSDataService
    {
        Task<List<ReadingTest>> GetAllTestsAsync();
        Task<ReadingTest?> GetTestByIdAsync(int id);
        Task<List<ReadingTest>> GetTestsByDifficultyAsync(DifficultyLevel difficulty);
        Task<TestSession> CreateTestSessionAsync(int testId, string userId = "anonymous");
        Task<TestSession?> GetTestSessionAsync(int sessionId);
        Task SaveUserAnswerAsync(int sessionId, int questionId, string answer);
        Task<TestResult> CompleteTestAsync(int sessionId);
        Task<List<TestResult>> GetUserTestResultsAsync(string userId = "anonymous");
        Task<TestResult?> GetTestResultAsync(int resultId);
    }

    public class IELTSDataService : IIELTSDataService
    {
        private readonly IELTSDbContext _context;
        private readonly ILogger<IELTSDataService> _logger;

        public IELTSDataService(IELTSDbContext context, ILogger<IELTSDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ReadingTest>> GetAllTestsAsync()
        {
            try
            {
                var tests = await _context.ReadingTests
                    .Include(t => t.TestSections)
                        .ThenInclude(ts => ts.Section)
                            .ThenInclude(s => s.Passage)
                    .ToListAsync();

                // Build the sections list for each test
                foreach (var test in tests)
                {
                    test.Sections = test.TestSections
                        .OrderBy(ts => ts.SectionOrder)
                        .Select(ts => ts.Section)
                        .ToList();

                    // Load questions for each section
                    foreach (var section in test.Sections)
                    {
                        section.Questions = await _context.Questions
                            .Where(q => q.PassageId == section.PassageId)
                            .OrderBy(q => q.QuestionNumber)
                            .ToListAsync();

                        // Deserialize JSON fields
                        DeserializePassageFields(section.Passage);
                        foreach (var question in section.Questions)
                        {
                            DeserializeQuestionFields(question);
                        }
                    }
                }

                return tests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tests");
                throw;
            }
        }

        public async Task<ReadingTest?> GetTestByIdAsync(int id)
        {
            try
            {
                var test = await _context.ReadingTests
                    .Include(t => t.TestSections)
                        .ThenInclude(ts => ts.Section)
                            .ThenInclude(s => s.Passage)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (test == null) return null;

                // Build the sections list
                test.Sections = test.TestSections
                    .OrderBy(ts => ts.SectionOrder)
                    .Select(ts => ts.Section)
                    .ToList();

                // Load questions for each section
                foreach (var section in test.Sections)
                {
                    section.Questions = await _context.Questions
                        .Where(q => q.PassageId == section.PassageId)
                        .OrderBy(q => q.QuestionNumber)
                        .ToListAsync();

                    // Deserialize JSON fields
                    DeserializePassageFields(section.Passage);
                    foreach (var question in section.Questions)
                    {
                        DeserializeQuestionFields(question);
                    }
                }

                return test;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test with ID {TestId}", id);
                throw;
            }
        }

        public async Task<List<ReadingTest>> GetTestsByDifficultyAsync(DifficultyLevel difficulty)
        {
            try
            {
                var tests = await _context.ReadingTests
                    .Where(t => t.Difficulty == difficulty)
                    .Include(t => t.TestSections)
                        .ThenInclude(ts => ts.Section)
                            .ThenInclude(s => s.Passage)
                    .ToListAsync();

                // Build the sections list for each test
                foreach (var test in tests)
                {
                    test.Sections = test.TestSections
                        .OrderBy(ts => ts.SectionOrder)
                        .Select(ts => ts.Section)
                        .ToList();

                    // Load questions for each section
                    foreach (var section in test.Sections)
                    {
                        section.Questions = await _context.Questions
                            .Where(q => q.PassageId == section.PassageId)
                            .OrderBy(q => q.QuestionNumber)
                            .ToListAsync();

                        // Deserialize JSON fields
                        DeserializePassageFields(section.Passage);
                        foreach (var question in section.Questions)
                        {
                            DeserializeQuestionFields(question);
                        }
                    }
                }

                return tests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tests by difficulty {Difficulty}", difficulty);
                throw;
            }
        }

        public async Task<TestSession> CreateTestSessionAsync(int testId, string userId = "anonymous")
        {
            try
            {
                var test = await _context.ReadingTests.FindAsync(testId);
                if (test == null)
                    throw new ArgumentException($"Test with ID {testId} not found");

                var session = new TestSession
                {
                    TestId = testId,
                    UserId = userId,
                    StartTime = DateTime.Now,
                    CurrentSectionIndex = 0,
                    IsCompleted = false,
                    RemainingTime = TimeSpan.FromMinutes(test.TotalTimeMinutes)
                };

                _context.TestSessions.Add(session);
                await _context.SaveChangesAsync();

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test session for test {TestId} and user {UserId}", testId, userId);
                throw;
            }
        }

        public async Task<TestSession?> GetTestSessionAsync(int sessionId)
        {
            try
            {
                var session = await _context.TestSessions
                    .Include(s => s.Test)
                    .Include(s => s.UserAnswers)
                        .ThenInclude(ua => ua.Question)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (session != null)
                {
                    // Build current answers dictionary
                    session.CurrentAnswers = session.UserAnswers.ToList();
                }

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task SaveUserAnswerAsync(int sessionId, int questionId, string answer)
        {
            try
            {
                var session = await _context.TestSessions.FindAsync(sessionId);
                if (session == null)
                    throw new ArgumentException($"Test session with ID {sessionId} not found");

                var question = await _context.Questions.FindAsync(questionId);
                if (question == null)
                    throw new ArgumentException($"Question with ID {questionId} not found");

                // Check if answer already exists
                var existingAnswer = await _context.UserAnswers
                    .FirstOrDefaultAsync(ua => ua.TestSessionId == sessionId && ua.QuestionId == questionId);

                if (existingAnswer != null)
                {
                    // Update existing answer
                    existingAnswer.Answer = answer;
                    existingAnswer.AnsweredAt = DateTime.Now;
                    existingAnswer.IsCorrect = IsAnswerCorrect(question, answer);
                }
                else
                {
                    // Create new answer
                    var userAnswer = new UserAnswer
                    {
                        TestSessionId = sessionId,
                        QuestionId = questionId,
                        Answer = answer,
                        AnsweredAt = DateTime.Now,
                        IsCorrect = IsAnswerCorrect(question, answer)
                    };

                    _context.UserAnswers.Add(userAnswer);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user answer for session {SessionId}, question {QuestionId}", sessionId, questionId);
                throw;
            }
        }

        public async Task<TestResult> CompleteTestAsync(int sessionId)
        {
            try
            {
                var session = await _context.TestSessions
                    .Include(s => s.Test)
                    .Include(s => s.UserAnswers)
                        .ThenInclude(ua => ua.Question)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (session == null)
                    throw new ArgumentException($"Test session with ID {sessionId} not found");

                // Mark session as completed
                session.IsCompleted = true;
                session.EndTime = DateTime.Now;

                // Calculate results
                var allQuestions = await _context.Questions
                    .Where(q => _context.TestSections
                        .Where(ts => ts.TestId == session.TestId)
                        .Select(ts => ts.Section.PassageId)
                        .Contains(q.PassageId))
                    .ToListAsync();

                var correctAnswers = session.UserAnswers.Count(ua => ua.IsCorrect);
                var timeTaken = session.EndTime.Value - session.StartTime;

                // Calculate scores by question type
                var scoresByType = new Dictionary<QuestionType, int>();
                foreach (var userAnswer in session.UserAnswers.Where(ua => ua.IsCorrect))
                {
                    var questionType = userAnswer.Question.Type;
                    scoresByType[questionType] = scoresByType.GetValueOrDefault(questionType, 0) + 1;
                }

                var testResult = new TestResult
                {
                    TestId = session.TestId,
                    TestSessionId = sessionId,
                    CorrectAnswers = correctAnswers,
                    TotalQuestions = allQuestions.Count,
                    Score = allQuestions.Count > 0 ? (double)correctAnswers / allQuestions.Count * 100 : 0,
                    BandScore = BandScoreCalculator.CalculateBandScore(correctAnswers),
                    TimeTaken = timeTaken,
                    CompletedAt = DateTime.Now,
                    ScoresByType = scoresByType
                };

                _context.TestResults.Add(testResult);
                await _context.SaveChangesAsync();

                return testResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing test for session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<List<TestResult>> GetUserTestResultsAsync(string userId = "anonymous")
        {
            try
            {
                var results = await _context.TestResults
                    .Include(r => r.Test)
                    .Include(r => r.TestSession)
                    .Where(r => r.TestSession!.UserId == userId)
                    .OrderByDescending(r => r.CompletedAt)
                    .ToListAsync();

                // Deserialize JSON fields
                foreach (var result in results)
                {
                    DeserializeTestResultFields(result);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test results for user {UserId}", userId);
                throw;
            }
        }

        public async Task<TestResult?> GetTestResultAsync(int resultId)
        {
            try
            {
                var result = await _context.TestResults
                    .Include(r => r.Test)
                    .Include(r => r.TestSession)
                        .ThenInclude(s => s!.UserAnswers)
                            .ThenInclude(ua => ua.Question)
                    .FirstOrDefaultAsync(r => r.Id == resultId);

                if (result != null)
                {
                    // Build answers list from session
                    result.Answers = result.TestSession?.UserAnswers.ToList() ?? new List<UserAnswer>();
                    DeserializeTestResultFields(result);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test result {ResultId}", resultId);
                throw;
            }
        }

        private bool IsAnswerCorrect(Question question, string userAnswer)
        {
            if (string.IsNullOrWhiteSpace(userAnswer) || string.IsNullOrWhiteSpace(question.CorrectAnswer))
                return false;

            return string.Equals(userAnswer.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private void DeserializePassageFields(ReadingPassage passage)
        {
            try
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
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing passage JSON fields for passage {PassageId}", passage.Id);
            }
        }

        private void DeserializeQuestionFields(Question question)
        {
            try
            {
                if (!string.IsNullOrEmpty(question.OptionsJson) && question.OptionsJson != "[]")
                {
                    question.Options = JsonSerializer.Deserialize<List<string>>(question.OptionsJson) ?? new List<string>();
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing question JSON fields for question {QuestionId}", question.Id);
            }
        }

        private void DeserializeTestResultFields(TestResult result)
        {
            try
            {
                if (!string.IsNullOrEmpty(result.ScoresByTypeJson) && result.ScoresByTypeJson != "{}")
                {
                    result.ScoresByType = JsonSerializer.Deserialize<Dictionary<QuestionType, int>>(result.ScoresByTypeJson) ?? new Dictionary<QuestionType, int>();
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializing test result JSON fields for result {ResultId}", result.Id);
            }
        }
    }
}