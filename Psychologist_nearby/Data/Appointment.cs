using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Psychologist_nearby.Data;

public class Appointment
{
    public int Id { get; set; }

    // ID клиента, который записался
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    // ID психолога, к которому записываются
    public int PsychologistId { get; set; }
    [ForeignKey(nameof(PsychologistId))]
    public User Psychologist { get; set; } = null!;

    // Дата и точное время сеанса
    [Required]
    public DateTime AppointmentDate { get; set; }

    // Статус (например: "Confirmed", "Canceled", "Completed")
    [MaxLength(50)]
    public string Status { get; set; } = "Confirmed";

    public DateTime TimeCreated { get; set; } = DateTime.Now;
}