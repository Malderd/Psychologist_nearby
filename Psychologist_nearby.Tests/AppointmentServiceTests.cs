using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Psychologist_nearby.Data;
using Psychologist_nearby.Services;
using Xunit;

namespace Psychologist_nearby.Tests;

public class AppointmentServiceTests
{
    [Theory]
    [InlineData(AppointmentFault.DatabaseError)]
    [InlineData(AppointmentFault.Timeout)]
    public async Task CreateAppointmentAsync_WhenDatabaseFails(
        AppointmentFault fault)
    {
        // Создание механизма внедрения отказов
        var injector = new AppointmentFaultInjector();

        // Изолированная база данных
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(injector)
            .Options;

        await using var context = new ApplicationDbContext(options);

        // Подготовка ролей
        context.Roles.AddRange(
            new Role { Id = 1, Name = "Client" },
            new Role { Id = 2, Name = "Psychologist" }
        );

        // Подготовка пользователей
        context.Users.AddRange(
            new User
            {
                Id = 1,
                Email = "client@test.local",
                PasswordHash = "test_hash",
                RoleId = 1,
                IsEmailConfirmed = true
            },
            new User
            {
                Id = 2,
                Email = "psychologist@test.local",
                PasswordHash = "test_hash",
                RoleId = 2,
                IsEmailConfirmed = true
            }
        );

        await context.SaveChangesAsync();

        var service = new AppointmentService(
            context,
            NullLogger<AppointmentService>.Instance
        );

        var appointmentDate = DateTime.Today
            .AddDays(1)
            .AddHours(12);

        // Включение искусственного отказа
        injector.Mode = fault;

        string? result = null;

        var exception = await Record.ExceptionAsync(async () =>
        {
            result = await service.CreateAppointmentAsync(
                1, 2, appointmentDate);
        });

        // Отключение искусственного отказа
        injector.Mode = AppointmentFault.None;

        // Проверка отсутствия записи в базе
        Assert.Equal(0, await context.Appointments.CountAsync());

        // Проверка отсутствия необработанного исключения
        Assert.Null(exception);

        // Проверка сообщения об ошибке
        Assert.False(string.IsNullOrWhiteSpace(result));

        // Проверка отсутствия незавершённых записей
        Assert.DoesNotContain(
            context.ChangeTracker.Entries<Appointment>(),
            entry => entry.State == EntityState.Added
        );

        // Проверка восстановления после отказа
        var retryResult = await service.CreateAppointmentAsync(
            1, 2, appointmentDate);

        Assert.Null(retryResult);
        Assert.Equal(1, await context.Appointments.CountAsync());
    }
}