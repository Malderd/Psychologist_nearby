
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CpuBoundExperiment;

internal class Program
{
    // Размер тестового файла: 64 МБ
    private const int FileSizeMb = 64;

    // Размер буфера чтения: 1 МБ
    private const int BufferSize = 1024 * 1024;

    // Количество прогревочных запусков
    private const int WarmupCount = 2;

    // Количество измерений
    private const int RepeatCount = 5;

    private static readonly string TestFilePath =
        Path.Combine(AppContext.BaseDirectory, "io_test_data.bin");

    private static async Task Main()
    {
        Console.WriteLine("I/O-bound эксперимент");
        Console.WriteLine("---------------------");

        int logicalProcessors = Environment.ProcessorCount;

        Console.WriteLine($"Логических процессоров: {logicalProcessors}");
        Console.WriteLine($"Размер файла: {FileSizeMb} МБ");
        Console.WriteLine($"Количество измерений: {RepeatCount}");
        Console.WriteLine();

        // Создаём файл до начала измерений
        await CreateTestFileAsync();

        long totalBytes = new FileInfo(TestFilePath).Length;

        // Варианты асинхронного выполнения
        int[] processorCounts =
        {
            1,
            2,
            logicalProcessors
        };

        processorCounts = processorCounts
            .Distinct()
            .ToArray();

        // Проверяем корректность чтения
        Console.WriteLine("Проверка корректности...");

        long syncBytes = ReadFileSynchronously();

        foreach (int p in processorCounts)
        {
            long asyncBytes = await ReadFileAsynchronously(p);

            if (syncBytes != asyncBytes)
            {
                throw new InvalidOperationException(
                    "Результаты чтения не совпадают.");
            }
        }

        Console.WriteLine("Проверка пройдена.");
        Console.WriteLine();

        // Прогревочные запуски
        Console.WriteLine("Прогрев...");

        for (int i = 0; i < WarmupCount; i++)
        {
            ReadFileSynchronously();

            foreach (int p in processorCounts)
            {
                await ReadFileAsynchronously(p);
            }
        }

        Console.WriteLine("Прогрев завершён.");
        Console.WriteLine();

        // Сохраняем результаты измерений
        var syncTimes = new List<double>();

        var asyncTimes = processorCounts.ToDictionary(
            p => p,
            p => new List<double>());

        // Выполняем измерения
        Console.WriteLine("Начало измерений...");

        for (int i = 0; i < RepeatCount; i++)
        {
            // Синхронное чтение
            Stopwatch stopwatch = Stopwatch.StartNew();

            ReadFileSynchronously();

            stopwatch.Stop();

            syncTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

            // Асинхронное чтение
            foreach (int p in processorCounts)
            {
                stopwatch.Restart();

                await ReadFileAsynchronously(p);

                stopwatch.Stop();

                asyncTimes[p].Add(
                    stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        Console.WriteLine();
        Console.WriteLine("Результаты измерений");
        Console.WriteLine("--------------------");

        PrintResults(
            "P3 Sync",
            syncTimes,
            totalBytes);

        foreach (int p in processorCounts)
        {
            PrintResults(
                $"P3 Async, p={p}",
                asyncTimes[p],
                totalBytes);
        }

        Console.WriteLine();
        Console.WriteLine("Эксперимент завершён.");
    }

    // Создание тестового файла
    private static async Task CreateTestFileAsync()
    {
        Console.WriteLine("Создание тестового файла...");

        byte[] buffer = new byte[BufferSize];

        Random random = new Random(12345);

        using FileStream stream = new FileStream(
            TestFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            BufferSize,
            FileOptions.Asynchronous);

        long totalBytes = (long)FileSizeMb * 1024 * 1024;

        long writtenBytes = 0;

        while (writtenBytes < totalBytes)
        {
            random.NextBytes(buffer);

            int bytesToWrite = (int)Math.Min(
                buffer.Length,
                totalBytes - writtenBytes);

            await stream.WriteAsync(
                buffer.AsMemory(0, bytesToWrite));

            writtenBytes += bytesToWrite;
        }

        Console.WriteLine("Файл создан.");
        Console.WriteLine();
    }

    // P3 Sync: последовательное синхронное чтение
    private static long ReadFileSynchronously()
    {
        byte[] buffer = new byte[BufferSize];

        long totalBytes = 0;

        using FileStream stream = new FileStream(
            TestFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.SequentialScan);

        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            totalBytes += bytesRead;
        }

        return totalBytes;
    }

    // P3 Async: асинхронное чтение с p рабочими задачами
    private static async Task<long> ReadFileAsynchronously(int p)
    {
        long fileLength = new FileInfo(TestFilePath).Length;

        long chunkSize = fileLength / p;

        Task<long>[] tasks = new Task<long>[p];

        for (int i = 0; i < p; i++)
        {
            long startPosition = i * chunkSize;

            long length = (i == p - 1)
                ? fileLength - startPosition
                : chunkSize;

            tasks[i] = ReadChunkAsync(
                startPosition,
                length);
        }

        long[] results = await Task.WhenAll(tasks);

        return results.Sum();
    }

    // Асинхронное чтение отдельного диапазона файла
    private static async Task<long> ReadChunkAsync(
        long startPosition,
        long length)
    {
        byte[] buffer = new byte[BufferSize];

        long totalBytes = 0;

        using FileStream stream = new FileStream(
            TestFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous);

        stream.Seek(startPosition, SeekOrigin.Begin);

        while (totalBytes < length)
        {
            int bytesToRead = (int)Math.Min(
                buffer.Length,
                length - totalBytes);

            int bytesRead = await stream.ReadAsync(
                buffer.AsMemory(0, bytesToRead));

            if (bytesRead == 0)
            {
                break;
            }

            totalBytes += bytesRead;
        }

        return totalBytes;
    }

    // Вывод статистики
    private static void PrintResults(
        string name,
        List<double> times,
        long totalBytes)
    {
        double min = times.Min();

        double max = times.Max();

        double median = CalculateMedian(times);

        double spread = (max - min) / median * 100;

        double throughput =
            totalBytes / (median / 1000.0) / (1024 * 1024);

        Console.WriteLine(name);

        Console.WriteLine($"  Min: {min:F2} мс");

        Console.WriteLine($"  Median: {median:F2} мс");

        Console.WriteLine($"  Max: {max:F2} мс");

        Console.WriteLine($"  Разброс: {spread:F2}%");

        Console.WriteLine(
            $"  Пропускная способность: {throughput:F2} МБ/с");

        Console.WriteLine();
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
