using System.ComponentModel.DataAnnotations;

namespace Psychologist_nearby.Data;

public class Role
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty; // "Client", "Psychologist"

    public ICollection<User> Users { get; set; } = new List<User>();
}