using System.ComponentModel.DataAnnotations.Schema;

namespace Psychologist_nearby.Data;

public class TestResult
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public int TestId { get; set; }

    [ForeignKey(nameof(TestId))]
    public Test Test { get; set; } = null!;

    public int TotalScore { get; set; }

    public string? Interpretation { get; set; }

    public DateTime TimeCreated { get; set; } = DateTime.Now;
}