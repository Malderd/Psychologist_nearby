using System.ComponentModel.DataAnnotations;

namespace Psychologist_nearby.Data;

public class TestInterpretation
{
    public int Id { get; set; }

    public int TestId { get; set; }

    public int MinScore { get; set; }

    public int MaxScore { get; set; }

    [Required]
    [MaxLength(255)]
    public string StatusText { get; set; } = string.Empty;

    public string? RecommendationText { get; set; }

    public Test? Test { get; set; }
}