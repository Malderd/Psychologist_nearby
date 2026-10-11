using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychologist_nearby.Data;

public class TestQuestion
{
    public int Id { get; set; }

    public int TestId { get; set; }

    [ForeignKey(nameof(TestId))]
    public Test Test { get; set; } = null!;

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    public ICollection<TestAnswer> Answers { get; set; } = new List<TestAnswer>();
}