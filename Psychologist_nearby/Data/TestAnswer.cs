using System.ComponentModel.DataAnnotations;

namespace Psychologist_nearby.Data;

public class TestAnswer
{
    public int Id { get; set; }

    public int TestId { get; set; }

    [Required]
    [MaxLength(255)]
    public string AnswerText { get; set; } = string.Empty;

    public int Score { get; set; } = 0;

    public Test? Test { get; set; }
}