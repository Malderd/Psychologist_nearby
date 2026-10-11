using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychologist_nearby.Data;

public class TestAnswer
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public TestQuestion Question { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string AnswerText { get; set; } = string.Empty;

    public int Score { get; set; } = 0;
}