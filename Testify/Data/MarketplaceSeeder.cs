using Microsoft.EntityFrameworkCore;
using Testify.Entities;

namespace Testify.Data
{
    public class MarketplaceSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            // --- 1. Ensure required categories exist ---
            var requiredCategories = new Dictionary<string, string>
            {
                ["AI & ML"] = "Artificial Intelligence and Machine Learning test suites",
                ["Security"] = "Security audit and compliance test suites",
                ["Web"] = "Web application and UI test suites",
                ["Finance"] = "FinTech and financial transaction test suites",
                ["Mobile"] = "Mobile application test suites",
                ["API"] = "API integration and performance test suites"
            };
            foreach (var (name, desc) in requiredCategories)
            {
                if (!await db.TemplateCategories.AnyAsync(c => c.Name == name))
                {
                    db.TemplateCategories.Add(new TemplateCategory { Name = name, Description = desc });
                }
            }
            await db.SaveChangesAsync();

            // Skip if the specific seeded templates already exist
            if (await db.TestSuiteTemplates.AnyAsync(t => t.Name == "Gemini API Stress Matrix")) return;

            // --- 2. Tags (add missing ones) ---
            var tagNames = new[] { "Gemini", "LLM", "Stress-Test", "Security", "SOC2", "Enterprise",
                                   "Ecom", "Stripe", "UI", "FinTech", "Ledger", "Math",
                                   "React", "API", "Performance", "Mobile", "iOS", "Android" };
            var existingTags = await db.TemplateTags.Select(t => t.TagName).ToListAsync();
            var newTags = tagNames.Where(t => !existingTags.Contains(t)).Select(t => new TemplateTag { TagName = t }).ToList();
            if (newTags.Any())
            {
                db.TemplateTags.AddRange(newTags);
                await db.SaveChangesAsync();
            }

            // Get first admin user for ownership
            var adminUser = await db.Users.FirstOrDefaultAsync();
            if (adminUser == null) return;
            var userId = adminUser.Id;

            // Lookup helpers — filter nulls to avoid ArgumentNullException
            var catLookup = await db.TemplateCategories
                .Where(c => c.Name != null)
                .ToDictionaryAsync(c => c.Name!, c => c.Id);

            var tagLookup = await db.TemplateTags
                .Where(t => t.TagName != null)
                .ToDictionaryAsync(t => t.TagName!, t => t.Id);

            // --- 3. Templates ---
            var templates = new List<TestSuiteTemplate>
            {
                new()
                {
                    Name = "Gemini API Stress Matrix",
                    Description = "Comprehensive edge-case validation for LLM integration, specifically tuned for token overflow and semantic drift.",
                    CategoryId = catLookup["AI & ML"],
                    UserId = userId,
                    IsPublic = true,
                    ViewCount = 890,
                    CloneCount = 1200,
                    TotalStarred = 49
                },
                new()
                {
                    Name = "SOC2 Security Baseline",
                    Description = "Standardized audit suite for compliance-heavy applications. Covers MFA bypass, session hijacking, and log integrity.",
                    CategoryId = catLookup["Security"],
                    UserId = userId,
                    IsPublic = true,
                    ViewCount = 42,
                    CloneCount = 3400,
                    TotalStarred = 48
                },
                new()
                {
                    Name = "E-Commerce Checkout Flow",
                    Description = "Universal checkout logic including multi-currency handling, cart persistence, and stripe integration hooks.",
                    CategoryId = catLookup["Web"],
                    UserId = userId,
                    IsPublic = true,
                    ViewCount = 18,
                    CloneCount = 850,
                    TotalStarred = 45
                },
                new()
                {
                    Name = "FinTech Ledger Integrity",
                    Description = "Strict validation for financial transaction double-entry records and floating point precision audits.",
                    CategoryId = catLookup["Finance"],
                    UserId = userId,
                    IsPublic = true,
                    ViewCount = 56,
                    CloneCount = 420,
                    TotalStarred = 50
                },
                new()
                {
                    Name = "React Component Smoke Tests",
                    Description = "Automated snapshot and interaction tests for common React UI patterns including forms, modals, and navigation flows.",
                    CategoryId = catLookup["Web"],
                    UserId = userId,
                    IsPublic = true,
                    ViewCount = 34,
                    CloneCount = 620,
                    TotalStarred = 42
                },
                new()
                {
                    Name = "REST API Contract Validator",
                    Description = "Schema validation, response time benchmarks, and error handling coverage for RESTful microservice architectures.",
                    CategoryId = catLookup["API"],
                    UserId = userId,
                    IsPublic = true,
                    ViewCount = 28,
                    CloneCount = 980,
                    TotalStarred = 47
                },
                new()
                {
                    Name = "iOS Native Regression Pack",
                    Description = "Full regression suite for iOS apps covering push notifications, deep linking, biometric auth, and offline mode.",
                    CategoryId = catLookup["Mobile"],
                    UserId = userId,
                    IsPublic = true,
                    ViewCount = 22,
                    CloneCount = 340,
                    TotalStarred = 38
                },
                new()
                {
                    Name = "GPT Prompt Injection Defense",
                    Description = "Security-focused test suite for detecting and preventing prompt injection attacks in AI-powered applications.",
                    CategoryId = catLookup["AI & ML"],
                    UserId = userId,
                    IsPublic = true,
                    ViewCount = 15,
                    CloneCount = 560,
                    TotalStarred = 46
                }
            };
            db.TestSuiteTemplates.AddRange(templates);
            await db.SaveChangesAsync();

            // --- 4. Template-Tag associations ---
            // Reuse the already-tracked local list — no need to re-query the DB
            var tagAssociations = new List<TestSuiteTemplateTag>();

            foreach (var t in templates)
            {
                var associatedTags = t.Name switch
                {
                    "Gemini API Stress Matrix" => new[] { "Gemini", "LLM", "Stress-Test" },
                    "SOC2 Security Baseline" => new[] { "Security", "SOC2", "Enterprise" },
                    "E-Commerce Checkout Flow" => new[] { "Ecom", "Stripe", "UI" },
                    "FinTech Ledger Integrity" => new[] { "FinTech", "Ledger", "Math" },
                    "React Component Smoke Tests" => new[] { "React", "UI", "Performance" },
                    "REST API Contract Validator" => new[] { "API", "Performance", "Enterprise" },
                    "iOS Native Regression Pack" => new[] { "Mobile", "iOS", "Performance" },
                    "GPT Prompt Injection Defense" => new[] { "Security", "LLM", "Stress-Test" },
                    _ => Array.Empty<string>()
                };

                foreach (var tagName in associatedTags)
                {
                    if (tagLookup.TryGetValue(tagName, out var tagId))
                    {
                        tagAssociations.Add(new TestSuiteTemplateTag { TemplateId = t.Id, TagId = tagId });
                    }
                }
            }

            db.TestSuiteTemplateTags.AddRange(tagAssociations);
            await db.SaveChangesAsync();
        }
    }
}