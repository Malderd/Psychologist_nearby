using System.ComponentModel.DataAnnotations;

namespace Psychologist_nearby.Data;

public class Test
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsPublished { get; set; } = true;

    public DateTime TimeCreated { get; set; } = DateTime.Now;

    public ICollection<TestQuestion> Questions { get; set; } = new List<TestQuestion>();
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}