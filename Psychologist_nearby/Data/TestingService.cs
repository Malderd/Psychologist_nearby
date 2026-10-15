using Microsoft.EntityFrameworkCore;
using Psychologist_nearby.Data;
namespace Psychologist_nearby.Data;

public class TestingService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public TestingService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // Получить все опубликованные тесты со всеми связанными данными
    public async Task<List<Test>> GetActiveTestsAsync()
    {
        using var context = await _dbFactory.CreateDbContextAsync();

        return await context.Tests
            .Where(t => t.IsPublished)
            .Include(t => t.Questions.OrderBy(q => q.SortOrder))
            .Include(t => t.Answers)
            .Include(t => t.Interpretations)
            .ToListAsync();
    }

    // Сохранить результат прохождения теста в таблицу TestResults
    public async Task SaveResultAsync(int testId, int totalScore, string interpretation, int? userId = null, string? userContact = null)
    {
        using var context = await _dbFactory.CreateDbContextAsync();

        var result = new TestResult
        {
            TestId = testId,
            UserId = userId,
            TotalScore = totalScore,
            Interpretation = interpretation,
            UserContact = userContact,
            TimeCreated = DateTime.Now
        };

        context.TestResults.Add(result);
        await context.SaveChangesAsync();
    }
}