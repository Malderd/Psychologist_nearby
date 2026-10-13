using System.ComponentModel.DataAnnotations;

namespace Psychologist_nearby.Data;

public class TestQuestion
{
    public int Id { get; set; }

    public int TestId { get; set; }

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public Test? Test { get; set; }
}