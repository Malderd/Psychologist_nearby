using System.ComponentModel.DataAnnotations;

namespace Psychologist_nearby.Data;

public class TestResult
{
    public int Id { get; set; }

    public int? UserId { get; set; } // Nullable для анонимных пользователей

    public int TestId { get; set; }

    public int TotalScore { get; set; }

    public string? Interpretation { get; set; }

    [MaxLength(255)]
    public string? UserContact { get; set; }

    public DateTime TimeCreated { get; set; } = DateTime.Now;

    public User? User { get; set; }
    public Test? Test { get; set; }
}