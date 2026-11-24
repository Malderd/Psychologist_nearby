using Microsoft.EntityFrameworkCore;
using Psychologist_nearby.Data;

namespace Psychologist_nearby.Services;

public class AppointmentService(ApplicationDbContext context, ILogger<AppointmentService> logger)
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<AppointmentService> _logger = logger;

    public async Task<string?> CreateAppointmentAsync(int clientId, int psychologistId, DateTime selectedDateTime)
    {
        var client = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == clientId);

        if (client == null)
            return "Пользователь не найден.";

        if (client.Role?.Name != "Client")
            return "Только клиенты могут записываться на приём.";

        var psychologist = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == psychologistId);

        if (psychologist == null || psychologist.Role?.Name != "Psychologist")
            return "Выбранный психолог не найден.";

        // 1. Проверка времени: строго с 10:00 до 19:00
        TimeSpan startTime = new TimeSpan(10, 0, 0);
        TimeSpan endTime = new TimeSpan(19, 0, 0);
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

        // 2. Проверка: не больше 5 записей у психолога в этот день
        int dailyCount = await _context.Appointments
            .Where(a => a.PsychologistId == psychologistId
                        && a.AppointmentDate.Date == targetDate
                        && a.Status == "Confirmed")
            .CountAsync();

        if (dailyCount >= 5)
        {
            return "У этого психолога на этот день уже исчерпан лимит записей (максимум 5 в день).";
        }

        // 3. Проверка на дубликаты / занятость точного времени
        bool isSlotTaken = await _context.Appointments
            .AnyAsync(a => a.PsychologistId == psychologistId
                        && a.AppointmentDate == selectedDateTime
                        && a.Status == "Confirmed");

        if (isSlotTaken)
        {
            return "Это время уже занято, выберите другой слот.";
        }

        // 4. Проверка: не записан ли сам клиент на это же время к другому/этому же психологу
        bool clientBusy = await _context.Appointments
            .AnyAsync(a => a.UserId == clientId
                        && a.AppointmentDate == selectedDateTime
                        && a.Status == "Confirmed");

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

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _context.Entry(appointment).State = EntityState.Detached;

            _logger.LogError(ex,
                "Ошибка сохранения записи к психологу.");

            return "Не удалось сохранить запись. Попробуйте позже.";
        }
        catch (TimeoutException ex)
        {
            _context.Entry(appointment).State = EntityState.Detached;

            _logger.LogError(ex,
                "Превышено время ожидания при сохранении записи.");

            return "Превышено время ожидания. Попробуйте позже.";
        }

        _logger.LogInformation("Клиент ID {ClientId} успешно записан к психологу ID {PsychologistId} на {Date}", clientId, psychologistId, selectedDateTime);

        return null; // Успешно, ошибок нет
    }
}