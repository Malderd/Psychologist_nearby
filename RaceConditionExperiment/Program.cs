
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RaceConditionExperiment;

internal class Program
{
    // Количество увеличений счётчика одним рабочим потоком
    private const int IterationsPerWorker = 250_000;

    // Количество прогревочных запусков
    private const int WarmupCount = 1;

    // Количество измерений
    private const int RepeatCount = 5;

    // Объект для синхронизации через lock
    private static readonly object LockObject = new object();

    private static void Main()
    {
        Console.WriteLine("P4: Исследование гонки данных");
        Console.WriteLine("--------------------------------");

        int logicalProcessors = Environment.ProcessorCount;

        Console.WriteLine(
            $"Логических процессоров: {logicalProcessors}");

        Console.WriteLine(
            $"Итераций на поток: {IterationsPerWorker}");

        Console.WriteLine();

        int[] processorCounts =
        {
            2,
            logicalProcessors
        };

        processorCounts = processorCounts
            .Distinct()
            .ToArray();

        foreach (int p in processorCounts)
        {
            RunExperiment(p);
        }

        Console.WriteLine();
        Console.WriteLine("Эксперимент завершён.");
    }

    private static void RunExperiment(int p)
    {
        long expectedCount =
            (long)p * IterationsPerWorker;

        Console.WriteLine($"Количество потоков p = {p}");
        Console.WriteLine($"Ожидаемый результат: {expectedCount}");
        Console.WriteLine();

        // Прогрев
        for (int i = 0; i < WarmupCount; i++)
        {
            RunWithoutSynchronization(p);
            RunWithLock(p);
            RunWithInterlocked(p);
        }

        // Измерения
        var raceTimes = new List<double>();
        var lockTimes = new List<double>();
        var interlockedTimes = new List<double>();

        long raceResult = 0;

        // Повторные запуски
        for (int i = 0; i < RepeatCount; i++)
        {
            // P4.1 — без синхронизации
            Stopwatch stopwatch = Stopwatch.StartNew();

            raceResult = RunWithoutSynchronization(p);

            stopwatch.Stop();

            raceTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

            // P4.2 — с lock
            stopwatch.Restart();

            RunWithLock(p);

            stopwatch.Stop();

            lockTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

            // P4.3 — с Interlocked
            stopwatch.Restart();

            RunWithInterlocked(p);

            stopwatch.Stop();

            interlockedTimes.Add(
                stopwatch.Elapsed.TotalMilliseconds);
        }

        Console.WriteLine("P4.1 — Без синхронизации");

        Console.WriteLine($"Итоговый счётчик: {raceResult}");

        Console.WriteLine($"Ожидаемый счётчик: {expectedCount}");

        long lostUpdates = expectedCount - raceResult;

        Console.WriteLine($"Потерянные увеличения: {lostUpdates}");

        double lossPercent =
            (double)lostUpdates / expectedCount * 100;

        Console.WriteLine($"Потери: {lossPercent:F2}%");

        PrintTime("Без синхронизации", raceTimes);

        Console.WriteLine();

        // Проверка корректности lock
        long lockResult = RunWithLock(p);

        Console.WriteLine("P4.2 — С использованием lock");

        Console.WriteLine($"Итоговый счётчик: {lockResult}");

        Console.WriteLine(
            $"Корректность: {lockResult == expectedCount}");

        PrintTime("Lock", lockTimes);

        Console.WriteLine();

        // Проверка корректности Interlocked
        long interlockedResult = RunWithInterlocked(p);

        Console.WriteLine("P4.3 — С использованием Interlocked");

        Console.WriteLine(
            $"Итоговый счётчик: {interlockedResult}");

        Console.WriteLine(
            $"Корректность: {interlockedResult == expectedCount}");

        PrintTime("Interlocked", interlockedTimes);

        Console.WriteLine();

        // Расчёт стоимости синхронизации
        double raceMedian = CalculateMedian(raceTimes);

        double lockMedian = CalculateMedian(lockTimes);

        double interlockedMedian =
            CalculateMedian(interlockedTimes);

        Console.WriteLine("Стоимость синхронизации:");

        Console.WriteLine(
            $"Lock относительно без синхронизации: " +
            $"{lockMedian - raceMedian:F2} мс");

        Console.WriteLine(
            $"Interlocked относительно без синхронизации: " +
            $"{interlockedMedian - raceMedian:F2} мс");

        Console.WriteLine();
        Console.WriteLine("--------------------------------");
        Console.WriteLine();
    }

    // P4.1: общий счётчик без синхронизации
    private static long RunWithoutSynchronization(int p)
    {
        long counter = 0;

        Parallel.For(0, p, worker =>
        {
            for (int i = 0; i < IterationsPerWorker; i++)
            {
                counter++;
            }
        });

        return counter;
    }

    // P4.2: общий счётчик с lock
    private static long RunWithLock(int p)
    {
        long counter = 0;

        Parallel.For(0, p, worker =>
        {
            for (int i = 0; i < IterationsPerWorker; i++)
            {
                lock (LockObject)
                {
                    counter++;
                }
            }
        });

        return counter;
    }

    // P4.3: общий счётчик с Interlocked
    private static long RunWithInterlocked(int p)
    {
        long counter = 0;

        Parallel.For(0, p, worker =>
        {
            for (int i = 0; i < IterationsPerWorker; i++)
            {
                Interlocked.Increment(ref counter);
            }
        });

        return counter;
    }

    // Вывод статистики времени
    private static void PrintTime(
        string name,
        List<double> times)
    {
        double min = times.Min();

        double max = times.Max();

        double median = CalculateMedian(times);

        double spread =
            (max - min) / median * 100;

        Console.WriteLine($"Вариант: {name}");

        Console.WriteLine($"Min: {min:F2} мс");

        Console.WriteLine($"Median: {median:F2} мс");

        Console.WriteLine($"Max: {max:F2} мс");

        Console.WriteLine($"Разброс: {spread:F2}%");
    }

    // Расчёт медианы
    private static double CalculateMedian(
        List<double> values)
    {
        double[] sorted = values
            .OrderBy(value => value)
            .ToArray();

        int middle = sorted.Length / 2;

        if (sorted.Length % 2 == 0)
        {
            return (
                sorted[middle - 1] +
                sorted[middle]
            ) / 2;
        }

        return sorted[middle];
    }
}
