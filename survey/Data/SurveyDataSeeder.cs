using AlphaSurvey.Models;
using Microsoft.EntityFrameworkCore;

namespace AlphaSurvey.Data;

public static class SurveyDataSeeder
{
    public static async Task SeedDefaultUseCasesAsync(SurveyDbContext context)
    {
        // Check if we already have data
        if (await context.UseCaseCategories.AnyAsync())
        {
            return; // Already seeded
        }

        var categories = new List<UseCaseCategory>
        {
            new UseCaseCategory
            {
                Name = "Chat & Conversation",
                Description = "How do you use AI for conversation and communication?",
                DisplayOrder = 1,
                Options = new List<UseCaseOption>
                {
                    new UseCaseOption { Name = "General conversation", DisplayOrder = 1 },
                    new UseCaseOption { Name = "Technical support/troubleshooting", DisplayOrder = 2 },
                    new UseCaseOption { Name = "Philosophical discussions", DisplayOrder = 3 },
                    new UseCaseOption { Name = "Mental health/emotional support", DisplayOrder = 4 },
                    new UseCaseOption { Name = "Language learning/practice", DisplayOrder = 5 },
                    new UseCaseOption { Name = "Entertainment/roleplay", DisplayOrder = 6 },
                    new UseCaseOption { Name = "Advice (career, relationships, life)", DisplayOrder = 7 },
                }
            },
            new UseCaseCategory
            {
                Name = "Writing & Content",
                Description = "What writing tasks do you use AI for?",
                DisplayOrder = 2,
                Options = new List<UseCaseOption>
                {
                    new UseCaseOption { Name = "Email drafting", DisplayOrder = 1 },
                    new UseCaseOption { Name = "Blog posts/articles", DisplayOrder = 2 },
                    new UseCaseOption { Name = "Social media content", DisplayOrder = 3 },
                    new UseCaseOption { Name = "Marketing copy", DisplayOrder = 4 },
                    new UseCaseOption { Name = "Creative writing (stories, poetry)", DisplayOrder = 5 },
                    new UseCaseOption { Name = "Academic writing", DisplayOrder = 6 },
                    new UseCaseOption { Name = "Resume/cover letters", DisplayOrder = 7 },
                    new UseCaseOption { Name = "Documentation/technical writing", DisplayOrder = 8 },
                    new UseCaseOption { Name = "Editing/proofreading", DisplayOrder = 9 },
                }
            },
            new UseCaseCategory
            {
                Name = "Coding & Development",
                Description = "How do you use AI for programming?",
                DisplayOrder = 3,
                Options = new List<UseCaseOption>
                {
                    new UseCaseOption { Name = "Writing new code", DisplayOrder = 1 },
                    new UseCaseOption { Name = "Debugging/troubleshooting", DisplayOrder = 2 },
                    new UseCaseOption { Name = "Code review/optimization", DisplayOrder = 3 },
                    new UseCaseOption { Name = "Learning to code", DisplayOrder = 4 },
                    new UseCaseOption { Name = "Converting between languages", DisplayOrder = 5 },
                    new UseCaseOption { Name = "Writing tests", DisplayOrder = 6 },
                    new UseCaseOption { Name = "API integration", DisplayOrder = 7 },
                    new UseCaseOption { Name = "Database queries (SQL)", DisplayOrder = 8 },
                    new UseCaseOption { Name = "DevOps/deployment scripts", DisplayOrder = 9 },
                }
            },
            new UseCaseCategory
            {
                Name = "Research & Learning",
                Description = "How do you use AI for research and education?",
                DisplayOrder = 4,
                Options = new List<UseCaseOption>
                {
                    new UseCaseOption { Name = "General research/fact-finding", DisplayOrder = 1 },
                    new UseCaseOption { Name = "Summarizing articles/documents", DisplayOrder = 2 },
                    new UseCaseOption { Name = "Learning new topics", DisplayOrder = 3 },
                    new UseCaseOption { Name = "Explaining complex concepts", DisplayOrder = 4 },
                    new UseCaseOption { Name = "Homework/study help", DisplayOrder = 5 },
                    new UseCaseOption { Name = "Literature review", DisplayOrder = 6 },
                    new UseCaseOption { Name = "Market research", DisplayOrder = 7 },
                    new UseCaseOption { Name = "Competitive analysis", DisplayOrder = 8 },
                }
            },
            new UseCaseCategory
            {
                Name = "Brainstorming & Ideas",
                Description = "Using AI for creative thinking and problem-solving",
                DisplayOrder = 5,
                Options = new List<UseCaseOption>
                {
                    new UseCaseOption { Name = "Business ideas", DisplayOrder = 1 },
                    new UseCaseOption { Name = "Product naming/branding", DisplayOrder = 2 },
                    new UseCaseOption { Name = "Creative project ideas", DisplayOrder = 3 },
                    new UseCaseOption { Name = "Problem-solving strategies", DisplayOrder = 4 },
                    new UseCaseOption { Name = "Overcoming creative blocks", DisplayOrder = 5 },
                    new UseCaseOption { Name = "Scenario planning", DisplayOrder = 6 },
                    new UseCaseOption { Name = "Innovation/ideation sessions", DisplayOrder = 7 },
                }
            },
            new UseCaseCategory
            {
                Name = "Data & Analysis",
                Description = "Working with data and analytics",
                DisplayOrder = 6,
                Options = new List<UseCaseOption>
                {
                    new UseCaseOption { Name = "Data analysis", DisplayOrder = 1 },
                    new UseCaseOption { Name = "Excel/spreadsheet formulas", DisplayOrder = 2 },
                    new UseCaseOption { Name = "Creating visualizations", DisplayOrder = 3 },
                    new UseCaseOption { Name = "Statistical analysis", DisplayOrder = 4 },
                    new UseCaseOption { Name = "Interpreting data/reports", DisplayOrder = 5 },
                    new UseCaseOption { Name = "Data cleaning/transformation", DisplayOrder = 6 },
                }
            },
            new UseCaseCategory
            {
                Name = "Business & Productivity",
                Description = "Professional and work-related tasks",
                DisplayOrder = 7,
                Options = new List<UseCaseOption>
                {
                    new UseCaseOption { Name = "Meeting summaries", DisplayOrder = 1 },
                    new UseCaseOption { Name = "Project planning", DisplayOrder = 2 },
                    new UseCaseOption { Name = "Task management/prioritization", DisplayOrder = 3 },
                    new UseCaseOption { Name = "Business strategy", DisplayOrder = 4 },
                    new UseCaseOption { Name = "Financial planning/analysis", DisplayOrder = 5 },
                    new UseCaseOption { Name = "Legal document review", DisplayOrder = 6 },
                    new UseCaseOption { Name = "Presentations/slides", DisplayOrder = 7 },
                }
            },
            new UseCaseCategory
            {
                Name = "Creative Projects",
                Description = "Art, music, design, and creative work",
                DisplayOrder = 8,
                Options = new List<UseCaseOption>
                {
                    new UseCaseOption { Name = "Image generation prompts", DisplayOrder = 1 },
                    new UseCaseOption { Name = "Design feedback/critique", DisplayOrder = 2 },
                    new UseCaseOption { Name = "Music composition ideas", DisplayOrder = 3 },
                    new UseCaseOption { Name = "Video scriptwriting", DisplayOrder = 4 },
                    new UseCaseOption { Name = "Game design/worldbuilding", DisplayOrder = 5 },
                    new UseCaseOption { Name = "UI/UX design ideas", DisplayOrder = 6 },
                }
            },
            new UseCaseCategory
            {
                Name = "Personal Assistant",
                Description = "Daily life and personal tasks",
                DisplayOrder = 9,
                Options = new List<UseCaseOption>
                {
                    new UseCaseOption { Name = "Meal planning/recipes", DisplayOrder = 1 },
                    new UseCaseOption { Name = "Travel planning", DisplayOrder = 2 },
                    new UseCaseOption { Name = "Shopping recommendations", DisplayOrder = 3 },
                    new UseCaseOption { Name = "Health/fitness advice", DisplayOrder = 4 },
                    new UseCaseOption { Name = "Home improvement/DIY", DisplayOrder = 5 },
                    new UseCaseOption { Name = "Gift ideas", DisplayOrder = 6 },
                    new UseCaseOption { Name = "Event planning", DisplayOrder = 7 },
                }
            }
        };

        await context.UseCaseCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
