using Microsoft.EntityFrameworkCore;
using Psychologist_nearby.Data;

namespace Psychologist_nearby.Services;

public class AppointmentService(ApplicationDbContext context, ILogger<AppointmentService> logger)
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<AppointmentService> _logger = logger;

    public async Task<string?> CreateAppointmentAsync(int clientId, int psychologistId, DateTime selectedDateTime)
    {
        // 1. Проверка времени: строго с 10:00 до 20:00
        TimeSpan startTime = new TimeSpan(10, 0, 0);
        TimeSpan endTime = new TimeSpan(20, 0, 0);
        TimeSpan selectedTime = selectedDateTime.TimeOfDay;

        if (selectedTime < startTime || selectedTime > endTime)
        {
            return "Запись возможна только с 10:00 до 20:00.";
        }

        // Запись строго на ровный час (например, 10:00, 11:00 и т.д.)
        if (selectedDateTime.Minute != 0 || selectedDateTime.Second != 0)
        {
            return "Запись осуществляется строго на ровный час (например, 10:00, 11:00 и т.д.).";
        }
        if (selectedDateTime <= DateTime.Now)
            return "Нельзя записаться на прошедшее время.";

        var targetDate = selectedDateTime.Date;

        // 2. Проверка: не больше 4 записей у психолога в этот день
        int dailyCount = await _context.Appointments
            .Where(a => a.PsychologistId == psychologistId
                        && a.AppointmentDate.Date == targetDate
                        && a.Status != "Canceled")
            .CountAsync();

        if (dailyCount >= 4)
        {
            return "У этого психолога на этот день уже исчерпан лимит записей (максимум 4 в день).";
        }

        // 3. Проверка на дубликаты / занятость точного времени
        bool isSlotTaken = await _context.Appointments
            .AnyAsync(a => a.PsychologistId == psychologistId
                        && a.AppointmentDate == selectedDateTime
                        && a.Status != "Canceled");

        if (isSlotTaken)
        {
            return "Это время уже занято, выберите другой слот.";
        }

        // 4. Проверка: не записан ли сам клиент на это же время к другому/этому же психологу
        bool clientBusy = await _context.Appointments
            .AnyAsync(a => a.UserId == clientId
                        && a.AppointmentDate == selectedDateTime
                        && a.Status != "Canceled");

        if (clientBusy)
        {
            return "У вас уже есть активная запись на это же время.";
        }

        // 5. Создаем запись
        var appointment = new Appointment
        {
            UserId = clientId,
            PsychologistId = psychologistId,
            AppointmentDate = selectedDateTime,
            Status = "Confirmed",
            TimeCreated = DateTime.Now
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Клиент ID {ClientId} успешно записан к психологу ID {PsychologistId} на {Date}", clientId, psychologistId, selectedDateTime);

        return null; // Успешно, ошибок нет
    }
}