using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychologist_nearby.Data;

public class Review
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public string TextReview { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    public DateTime TimeCreated { get; set; } = DateTime.Now;
}