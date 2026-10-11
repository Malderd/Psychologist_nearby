using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Psychologist_nearby.Data;

public class User
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Name { get; set; }

    [MaxLength(150)]
    public string? Surname { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    // Внешний ключ на таблицу Roles
    public int RoleId { get; set; } = 1; // 1 = Client по умолчанию

    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;

    public bool IsEmailConfirmed { get; set; } = false;

    [MaxLength(6)]
    public string? EmailConfirmationCode { get; set; }

    public DateTime? CodeExpiryTime { get; set; }

    public DateTime TimeCreated { get; set; } = DateTime.Now;

    // Навигационные свойства
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}