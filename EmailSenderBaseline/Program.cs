
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EmailSenderBaseline;

internal class Program
{
    // Прогревочные отправки
    private const int WarmupCount = 2;

    // Количество измерений
    private const int RepeatCount = 5;

    // Результаты P5 — базовая линия
    private const double P5Median = 2274.24;
    private const double P5Min = 1815.33;
    private const double P5Max = 2578.90;
    private const double P5Spread = 33.58;

    private static async Task Main()
    {
        Console.WriteLine("P6: EmailSender — альтернативный вариант");
        Console.WriteLine("----------------------------------------");

        // Загружаем настройки из User Secrets
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string host = GetRequiredSetting(configuration, "Smtp:Server");
        int port = int.Parse(
            GetRequiredSetting(configuration, "Smtp:Port"));

        string user = GetRequiredSetting(configuration, "Smtp:User");
        string password = GetRequiredSetting(configuration, "Smtp:Password");

        // Адрес тестового получателя
        string to = GetRequiredEnvironmentVariable("SMTP_TEST_TO");

        const bool enableSsl = true;

        Console.WriteLine($"SMTP сервер: {host}");
        Console.WriteLine($"SMTP порт: {port}");
        Console.WriteLine($"SSL: {enableSsl}");
        Console.WriteLine("Альтернатива: повторное использование SmtpClient");
        Console.WriteLine($"Прогревочных отправок: {WarmupCount}");
        Console.WriteLine($"Измерений: {RepeatCount}");
        Console.WriteLine();

        var times = new List<double>();

        // Создаём один SMTP-клиент
        // и используем его для всех отправок
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(user, password),
            Timeout = 30000
        };

        // Прогрев
        Console.WriteLine("Прогрев...");

        for (int i = 0; i < WarmupCount; i++)
        {
            await SendTestEmailAsync(client, user, to);

            Console.WriteLine(
                $"Прогрев {i + 1}/{WarmupCount} завершён");
        }

        Console.WriteLine();
        Console.WriteLine("Измерения...");

        // Повторные отправки через один SmtpClient
        for (int i = 0; i < RepeatCount; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            await SendTestEmailAsync(client, user, to);

            stopwatch.Stop();

            double elapsed = stopwatch.Elapsed.TotalMilliseconds;

            times.Add(elapsed);

            Console.WriteLine(
                $"Измерение {i + 1}: {elapsed:F2} мс");
        }

        // Статистика P6
        double min = times.Min();
        double max = times.Max();
        double median = CalculateMedian(times);

        double spread = median == 0
            ? 0
            : (max - min) / median * 100;

        // Сравнение P5 и P6
        double difference = P5Median - median;

        double improvement = P5Median == 0
            ? 0
            : difference / P5Median * 100;

        Console.WriteLine();
        Console.WriteLine("Результаты P6");
        Console.WriteLine("----------------------------------------");

        Console.WriteLine($"Min: {min:F2} мс");
        Console.WriteLine($"Median: {median:F2} мс");
        Console.WriteLine($"Max: {max:F2} мс");
        Console.WriteLine($"Разброс: {spread:F2}%");

        Console.WriteLine();
        Console.WriteLine("Сравнение с P5");
        Console.WriteLine("----------------------------------------");

        Console.WriteLine($"P5 Median: {P5Median:F2} мс");
        Console.WriteLine($"P6 Median: {median:F2} мс");

        Console.WriteLine(
            $"Разница: {difference:F2} мс");

        Console.WriteLine(
            $"Изменение времени: {improvement:F2}%");

        Console.WriteLine();
        Console.WriteLine("Базовая линия P5:");
        Console.WriteLine($"Min: {P5Min:F2} мс");
        Console.WriteLine($"Max: {P5Max:F2} мс");
        Console.WriteLine($"Разброс: {P5Spread:F2}%");

        Console.WriteLine();
        Console.WriteLine("Эксперимент P6 завершён.");
    }

    // Отправка тестового письма через переданный SMTP-клиент
    private static async Task SendTestEmailAsync(
        SmtpClient client,
        string user,
        string to)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(user, "P6 — тест"),
            Subject = "P6 — повторное измерение EmailSender",
            Body = "Тестовое письмо для практической работы P6.",
            IsBodyHtml = false
        };

        message.To.Add(new MailAddress(to));

        await client.SendMailAsync(message);
    }

    // Получение настройки из User Secrets
    private static string GetRequiredSetting(
        IConfiguration configuration,
        string key)
    {
        string? value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"В User Secrets не задан параметр {key}.");
        }

        return value;
    }

    // Получение переменной окружения
    private static string GetRequiredEnvironmentVariable(
        string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Не задана переменная окружения {name}.");
        }

        return value;
    }

    // Расчёт медианы
    private static double CalculateMedian(List<double> values)
    {
        double[] sorted = values
            .OrderBy(value => value)
            .ToArray();

        int middle = sorted.Length / 2;

        if (sorted.Length % 2 == 0)
        {
            return (sorted[middle - 1] + sorted[middle]) / 2;
        }

        return sorted[middle];
    }
}
