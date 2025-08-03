using IELTSPracticeApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IELTSPracticeApp.Data
{
    public static class SeedData
    {
        public static void SeedQuestions(ModelBuilder modelBuilder)
        {
            var questions = new List<Question>
            {
                // Multiple Choice Questions (1-3)
                new Question
                {
                    Id = 1,
                    ReadingPassageId = 1,
                    QuestionText = "According to the passage, ocean acidification is caused by:",
                    QuestionType = QuestionType.MultipleChoice,
                    QuestionOrder = 1,
                    Instructions = "Choose the correct answer A, B, C or D.",
                    Options = @"[
                        {""key"": ""A"", ""text"": ""Rising sea levels""},
                        {""key"": ""B"", ""text"": ""The ocean absorbing carbon dioxide from the atmosphere""},
                        {""key"": ""C"", ""text"": ""Increased ocean temperatures""},
                        {""key"": ""D"", ""text"": ""Marine pollution""}
                    ]",
                    CorrectAnswer = "b",
                    Points = 1
                },
                new Question
                {
                    Id = 2,
                    ReadingPassageId = 1,
                    QuestionText = "The Great Barrier Reef has experienced mass bleaching events due to:",
                    QuestionType = QuestionType.MultipleChoice,
                    QuestionOrder = 2,
                    Instructions = "Choose the correct answer A, B, C or D.",
                    Options = @"[
                        {""key"": ""A"", ""text"": ""Ocean acidification""},
                        {""key"": ""B"", ""text"": ""Overfishing""},
                        {""key"": ""C"", ""text"": ""Rising ocean temperatures""},
                        {""key"": ""D"", ""text"": ""Pollution""}
                    ]",
                    CorrectAnswer = "c",
                    Points = 1
                },

                // True/False/Not Given Questions (3-6)
                new Question
                {
                    Id = 3,
                    ReadingPassageId = 1,
                    QuestionText = "Ocean pH has dropped by exactly 0.1 units since the Industrial Revolution.",
                    QuestionType = QuestionType.TrueFalseNotGiven,
                    QuestionOrder = 3,
                    Instructions = "Write TRUE if the statement agrees with the information, FALSE if it contradicts, or NOT GIVEN if there is no information.",
                    CorrectAnswer = "true",
                    Points = 1
                },
                new Question
                {
                    Id = 4,
                    ReadingPassageId = 1,
                    QuestionText = "All marine organisms are equally affected by ocean acidification.",
                    QuestionType = QuestionType.TrueFalseNotGiven,
                    QuestionOrder = 4,
                    Instructions = "Write TRUE if the statement agrees with the information, FALSE if it contradicts, or NOT GIVEN if there is no information.",
                    CorrectAnswer = "false",
                    Points = 1
                },
                new Question
                {
                    Id = 5,
                    ReadingPassageId = 1,
                    QuestionText = "Phytoplankton populations have increased due to climate change.",
                    QuestionType = QuestionType.TrueFalseNotGiven,
                    QuestionOrder = 5,
                    Instructions = "Write TRUE if the statement agrees with the information, FALSE if it contradicts, or NOT GIVEN if there is no information.",
                    CorrectAnswer = "not given",
                    Points = 1
                },

                // Matching Information Questions (6-8)
                new Question
                {
                    Id = 6,
                    ReadingPassageId = 1,
                    QuestionText = "Which paragraph contains information about the impact on coral reefs?",
                    QuestionType = QuestionType.MatchingInformation,
                    QuestionOrder = 6,
                    Instructions = "Match the information to the correct paragraph (A-I).",
                    CorrectAnswer = "d",
                    Points = 1
                },
                new Question
                {
                    Id = 7,
                    ReadingPassageId = 1,
                    QuestionText = "Which paragraph discusses the effects on marine food webs?",
                    QuestionType = QuestionType.MatchingInformation,
                    QuestionOrder = 7,
                    Instructions = "Match the information to the correct paragraph (A-I).",
                    CorrectAnswer = "e",
                    Points = 1
                },

                // Sentence Completion Questions (8-10)
                new Question
                {
                    Id = 8,
                    ReadingPassageId = 1,
                    QuestionText = "Ocean acidification makes it difficult for marine organisms to extract _______ ions.",
                    QuestionType = QuestionType.SentenceCompletion,
                    QuestionOrder = 8,
                    Instructions = "Complete the sentence using NO MORE THAN TWO WORDS from the passage.",
                    CorrectAnswer = "carbonate",
                    WordLimit = 2,
                    Points = 1
                },
                new Question
                {
                    Id = 9,
                    ReadingPassageId = 1,
                    QuestionText = "Coral reefs are often called the _______ of the sea.",
                    QuestionType = QuestionType.SentenceCompletion,
                    QuestionOrder = 9,
                    Instructions = "Complete the sentence using NO MORE THAN TWO WORDS from the passage.",
                    CorrectAnswer = "rainforests",
                    WordLimit = 2,
                    Points = 1
                },

                // Short Answer Questions (10-12)
                new Question
                {
                    Id = 10,
                    ReadingPassageId = 1,
                    QuestionText = "What percentage increase in acidity does a 0.1 unit drop in pH represent?",
                    QuestionType = QuestionType.ShortAnswerQuestions,
                    QuestionOrder = 10,
                    Instructions = "Answer using NO MORE THAN TWO WORDS AND/OR A NUMBER.",
                    CorrectAnswer = "30%|thirty percent",
                    WordLimit = 3,
                    Points = 1
                },
                new Question
                {
                    Id = 11,
                    ReadingPassageId = 1,
                    QuestionText = "What are the microscopic plants that form the base of marine food chains called?",
                    QuestionType = QuestionType.ShortAnswerQuestions,
                    QuestionOrder = 11,
                    Instructions = "Answer using NO MORE THAN TWO WORDS.",
                    CorrectAnswer = "phytoplankton",
                    WordLimit = 2,
                    Points = 1
                },

                // Summary Completion Questions (12-14)
                new Question
                {
                    Id = 12,
                    ReadingPassageId = 1,
                    QuestionText = "Climate change has led to rising ocean temperatures and increased _______, which affects marine ecosystems.",
                    QuestionType = QuestionType.SummaryCompletion,
                    QuestionOrder = 12,
                    Instructions = "Complete the summary using words from the passage. Use NO MORE THAN TWO WORDS.",
                    CorrectAnswer = "ocean acidification",
                    WordLimit = 2,
                    Points = 1
                },
                new Question
                {
                    Id = 13,
                    ReadingPassageId = 1,
                    QuestionText = "Fish populations are moving toward the _______ as they follow their preferred temperature ranges.",
                    QuestionType = QuestionType.SummaryCompletion,
                    QuestionOrder = 13,
                    Instructions = "Complete the summary using words from the passage. Use NO MORE THAN TWO WORDS.",
                    CorrectAnswer = "poles",
                    WordLimit = 2,
                    Points = 1
                },

                // Yes/No/Not Given Questions (14-16)
                new Question
                {
                    Id = 14,
                    ReadingPassageId = 1,
                    QuestionText = "The writer believes that marine protected areas can help build resilience in marine ecosystems.",
                    QuestionType = QuestionType.YesNoNotGiven,
                    QuestionOrder = 14,
                    Instructions = "Write YES if the statement agrees with the writer's views, NO if it contradicts, or NOT GIVEN if there is no information about the writer's views.",
                    CorrectAnswer = "yes",
                    Points = 1
                },
                new Question
                {
                    Id = 15,
                    ReadingPassageId = 1,
                    QuestionText = "The writer suggests that individual actions are more important than global cooperation in addressing climate change.",
                    QuestionType = QuestionType.YesNoNotGiven,
                    QuestionOrder = 15,
                    Instructions = "Write YES if the statement agrees with the writer's views, NO if it contradicts, or NOT GIVEN if there is no information about the writer's views.",
                    CorrectAnswer = "no",
                    Points = 1
                }
            };

            modelBuilder.Entity<Question>().HasData(questions);

            // Seed Question Options for Multiple Choice Questions
            var questionOptions = new List<QuestionOption>
            {
                // Question 1 options
                new QuestionOption { Id = 1, QuestionId = 1, OptionKey = "A", OptionText = "Rising sea levels", OptionOrder = 1 },
                new QuestionOption { Id = 2, QuestionId = 1, OptionKey = "B", OptionText = "The ocean absorbing carbon dioxide from the atmosphere", OptionOrder = 2, IsCorrect = true },
                new QuestionOption { Id = 3, QuestionId = 1, OptionKey = "C", OptionText = "Increased ocean temperatures", OptionOrder = 3 },
                new QuestionOption { Id = 4, QuestionId = 1, OptionKey = "D", OptionText = "Marine pollution", OptionOrder = 4 },

                // Question 2 options
                new QuestionOption { Id = 5, QuestionId = 2, OptionKey = "A", OptionText = "Ocean acidification", OptionOrder = 1 },
                new QuestionOption { Id = 6, QuestionId = 2, OptionKey = "B", OptionText = "Overfishing", OptionOrder = 2 },
                new QuestionOption { Id = 7, QuestionId = 2, OptionKey = "C", OptionText = "Rising ocean temperatures", OptionOrder = 3, IsCorrect = true },
                new QuestionOption { Id = 8, QuestionId = 2, OptionKey = "D", OptionText = "Pollution", OptionOrder = 4 }
            };

            modelBuilder.Entity<QuestionOption>().HasData(questionOptions);
        }
    }
}