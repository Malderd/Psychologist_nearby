using Psychologist_nearby.Data;

namespace Psychologist_nearby.Tests;

/// <summary>
/// Тесты подмены реализации (P4) и проверки с двумя реализациями (P6).
/// </summary>
public class FixedScoreInterpreterTests
{
    private static Test BuildTest()
    {
        return new Test
        {
            Id = 1,
            Title = "PHQ-9",
            Interpretations = new List<TestInterpretation>
            {
                new() { MinScore = 0, MaxScore = 4, StatusText = "Минимум" },
                new() { MinScore = 5, MaxScore = 9, StatusText = "Лёгкая" }
            }
        };
    }

    // ==== Штатный случай: RangeScoreInterpreter ====
    [Fact]
    public void RangeInterpreter_ScoreInsideRange_ReturnsExpected()
    {
        IScoreInterpreter interpreter = new RangeScoreInterpreter();
        var result = interpreter.Interpret(BuildTest(), 6);
        Assert.Equal("Лёгкая", result.StatusText);
    }

    // ==== Граничный случай: RangeScoreInterpreter ====
    [Fact]
    public void RangeInterpreter_ScoreEqualsMaxBoundary_ReturnsRange()
    {
        IScoreInterpreter interpreter = new RangeScoreInterpreter();
        var result = interpreter.Interpret(BuildTest(), 4);
        Assert.Equal("Минимум", result.StatusText);
    }

    // ==== Ошибочный случай: RangeScoreInterpreter ====
    [Fact]
    public void RangeInterpreter_ScoreOutsideRanges_ReturnsUnknown()
    {
        IScoreInterpreter interpreter = new RangeScoreInterpreter();
        var result = interpreter.Interpret(BuildTest(), 99);
        Assert.Equal("Не определено", result.StatusText);
    }

    // ==== Подмена: FixedScoreInterpreter всегда возвращает одно и то же ====
    [Fact]
    public void FixedInterpreter_AlwaysReturnsSameResult_RegardlessOfScore()
    {
        var fixedResult = new TestInterpretation
        {
            StatusText = "Фиксированный результат",
            RecommendationText = "Тестовая заглушка"
        };

        IScoreInterpreter interpreter = new FixedScoreInterpreter(fixedResult);
        var test = BuildTest();

        // Для любого балла — один и тот же результат
        Assert.Equal("Фиксированный результат", interpreter.Interpret(test, 0).StatusText);
        Assert.Equal("Фиксированный результат", interpreter.Interpret(test, 5).StatusText);
        Assert.Equal("Фиксированный результат", interpreter.Interpret(test, 99).StatusText);
    }

    // ==== Обе реализации подключаются через один и тот же интерфейс ====
    [Fact]
    public void BothImplementations_WorkThroughSameInterface()
    {
        IScoreInterpreter range = new RangeScoreInterpreter();
        IScoreInterpreter fixedInterp = new FixedScoreInterpreter(
            new TestInterpretation { StatusText = "Фикс" });

        var test = BuildTest();
        var rangeResult = range.Interpret(test, 5);
        var fixedResult = fixedInterp.Interpret(test, 5);

        Assert.Equal("Лёгкая", rangeResult.StatusText);
        Assert.Equal("Фикс", fixedResult.StatusText);
    }
}