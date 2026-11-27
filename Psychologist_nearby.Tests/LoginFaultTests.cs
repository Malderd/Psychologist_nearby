using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Psychologist_nearby.Data;
using Psychologist_nearby.Components.Account.Pages;

namespace Psychologist_nearby.Tests;

public class LoginFaultTests
{
    [Theory]
    [InlineData(EmailFault.SmtpError)]
    [InlineData(EmailFault.Timeout)]
    public async Task LoginUser_WhenEmailFails_RejectsLogin(
        EmailFault fault)
    {
        using var testContext = new Bunit.TestContext();

        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        await using var db = new ApplicationDbContext(options);

        db.Roles.Add(new Role
        {
            Id = 1,
            Name = "Client"
        });

        db.Users.Add(new User
        {
            Id = 1,
            Email = "client@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("Strong123!"),
            RoleId = 1,
            IsEmailConfirmed = true
        });

        await db.SaveChangesAsync();

        var injector = new EmailFaultInjector
        {
            Mode = fault
        };

        testContext.Services.AddLogging();

        testContext.Services.AddSingleton(db);

        testContext.Services.AddSingleton<IAuthValidator,
            RegexAuthValidator>();

        testContext.Services.AddSingleton<EmailSender>(
            injector);

        var component =
            testContext.RenderComponent<Login>();

        component.Find("input[autocomplete='username']")
            .Change("client@example.com");

        component.Find("input[type='password']")
            .Change("Strong123!");

        await component.Find("form").SubmitAsync();

        // Проверка вызова механизма внесения отказов
        Assert.Equal(1, injector.Attempts);
        Assert.Equal(0, injector.SuccessfulSends);

        // Проверка реакции интерфейса
        component.WaitForAssertion(() =>
        {
            Assert.Contains(
                "Не удалось отправить код на почту",
                component.Find(".alert-danger").TextContent);
        });

        // Проверка сохранённых данных
        db.ChangeTracker.Clear();

        var user = await db.Users.SingleAsync();

        Assert.Null(user.EmailConfirmationCode);
        Assert.Null(user.CodeExpiryTime);

        // Проверка отсутствия перехода к подтверждению
        var navigation = testContext.Services
            .GetRequiredService<
                Microsoft.AspNetCore.Components.NavigationManager>();

        Assert.DoesNotContain(
            "/Account/VerifyCode", navigation.Uri);

        Assert.DoesNotContain(
            "/Account/PersonalAccount", navigation.Uri);
    }

    [Fact]
    public async Task LoginUser_WhenEmailAndCancellationFail_CodeMustNotRemain()
    {
        using var testContext = new Bunit.TestContext();

        var databaseFault = new LoginDatabaseFaultInjector();

        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(databaseFault)
                .Options;

        await using var db = new ApplicationDbContext(options);

        db.Roles.Add(new Role
        {
            Id = 1,
            Name = "Client"
        });

        db.Users.Add(new User
        {
            Id = 1,
            Email = "client@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("Strong123!"),
            RoleId = 1,
            IsEmailConfirmed = true
        });

        await db.SaveChangesAsync();

        var emailFault = new EmailFaultInjector
        {
            Mode = EmailFault.SmtpError
        };

        testContext.Services.AddLogging();

        testContext.Services.AddSingleton(db);

        testContext.Services.AddSingleton<IAuthValidator,
            RegexAuthValidator>();

        testContext.Services.AddSingleton<EmailSender>(
            emailFault);

        var component = testContext.RenderComponent<Login>();

        component.Find("input[autocomplete='username']")
            .Change("client@example.com");

        component.Find("input[type='password']")
            .Change("Strong123!");

        databaseFault.Enabled = true;

        await component.Find("form").SubmitAsync();

        databaseFault.Enabled = false;

        Assert.Equal(1, emailFault.Attempts);
        Assert.Equal(0, emailFault.SuccessfulSends);

        Assert.NotEmpty(
            component.Find(".alert-danger").TextContent);

        db.ChangeTracker.Clear();

        var user = await db.Users.SingleAsync();

        Assert.Null(user.EmailConfirmationCode);
        Assert.Null(user.CodeExpiryTime);
    }

    [Fact]
    public async Task RepeatedLoginFailures_Diagnostics()
    {
        using var process =
            System.Diagnostics.Process.GetCurrentProcess();

        // Предварительный прогон
        await LoginUser_WhenEmailFails_RejectsLogin(
            EmailFault.SmtpError);

        process.Refresh();

        double memoryBefore = process.WorkingSet64 / 1048576.0;
        int threadsBefore = process.Threads.Count;
        int handlesBefore = process.HandleCount;

        for (int i = 0; i < 10; i++)
        {
            await LoginUser_WhenEmailFails_RejectsLogin(
                EmailFault.SmtpError);

            await LoginUser_WhenEmailFails_RejectsLogin(
                EmailFault.Timeout);

            await LoginUser_WhenEmailAndCancellationFail_CodeMustNotRemain();
        }

        await Task.Delay(30000);

        process.Refresh();

        Console.WriteLine("Fault scenarios: 30");
        Console.WriteLine($"Memory before: {memoryBefore:F2} MB");
        Console.WriteLine($"Memory after: {process.WorkingSet64 / 1048576.0:F2} MB");
        Console.WriteLine($"Threads: {threadsBefore} -> {process.Threads.Count}");
        Console.WriteLine($"Handles: {handlesBefore} -> {process.HandleCount}");
    }
}