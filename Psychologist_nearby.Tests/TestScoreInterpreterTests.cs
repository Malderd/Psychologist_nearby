using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Psychologist_nearby.Data;

namespace Psychologist_nearby.Tests;

public class TestScoreInterpreterTests
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

    // ==== П1. Интерпретация по диапазону ====

    [Fact] // Тест 1
    public void Interpret_ScoreInsideSecondRange_ReturnsSecond()
    {
        var result = TestScoreInterpreter.Interpret(BuildTest(), 5);
        Assert.Equal("Лёгкая", result.StatusText);
    }

    [Fact] // Тест 2: граница MinScore
    public void Interpret_ScoreEqualsMinScore_ReturnsFirst()
    {
        var result = TestScoreInterpreter.Interpret(BuildTest(), 0);
        Assert.Equal("Минимум", result.StatusText);
    }

    [Fact] // Тест 3: граница MaxScore
    public void Interpret_ScoreEqualsMaxScore_ReturnsFirst()
    {
        var result = TestScoreInterpreter.Interpret(BuildTest(), 4);
        Assert.Equal("Минимум", result.StatusText);
    }

    [Fact] // Тест 4: вне диапазонов
    public void Interpret_ScoreOutsideAllRanges_ReturnsUnknown()
    {
        var result = TestScoreInterpreter.Interpret(BuildTest(), 25);
        Assert.Equal(TestScoreInterpreter.UnknownStatus, result.StatusText);
    }

    [Fact] // Тест 5: граница MinScore-1
    public void Interpret_ScoreBelowAllRanges_ReturnsUnknown()
    {
        var result = TestScoreInterpreter.Interpret(BuildTest(), -1);
        Assert.Equal(TestScoreInterpreter.UnknownStatus, result.StatusText);
    }

    [Fact] // Тест 6: граница MaxScore+1
    public void Interpret_ScoreJustAboveRange_ReturnsUnknown()
    {
        var test = new Test
        {
            Interpretations = new List<TestInterpretation>
            {
                new() { MinScore = 0, MaxScore = 4, StatusText = "Минимум" }
            }
        };
        var result = TestScoreInterpreter.Interpret(test, 5);
        Assert.Equal(TestScoreInterpreter.UnknownStatus, result.StatusText);
    }

    // ==== П2. Сумма баллов ====

    [Fact] // Тест 7
    public void CalculateScore_ThreeAnswers_ReturnsSum()
    {
        var answers = new List<TestAnswer>
        {
            new() { Score = 0 },
            new() { Score = 2 },
            new() { Score = 3 }
        };
        Assert.Equal(5, TestScoreInterpreter.CalculateScore(answers));
    }

    [Fact] // Тест 8: граница — пустой список
    public void CalculateScore_EmptyList_ReturnsZero()
    {
        Assert.Equal(0, TestScoreInterpreter.CalculateScore(new List<TestAnswer>()));
    }

    [Fact] // Тест 9: null
    public void CalculateScore_Null_ReturnsZero()
    {
        Assert.Equal(0, TestScoreInterpreter.CalculateScore(null!));
    }

    // ==== П3. Завершение теста ====

    [Fact] // Тест 10: граница — 0 вопросов
    public void IsTestFinished_ZeroQuestions_ReturnsTrue()
    {
        Assert.True(TestScoreInterpreter.IsTestFinished(0, 0));
    }

    [Fact] // Тест 11
    public void IsTestFinished_InProgress_ReturnsFalse()
    {
        Assert.False(TestScoreInterpreter.IsTestFinished(0, 5));
    }

    [Fact] // Тест 12: граница — index == count
    public void IsTestFinished_IndexEqualsCount_ReturnsTrue()
    {
        Assert.True(TestScoreInterpreter.IsTestFinished(5, 5));
    }

    [Fact] // Тест 13: граница — index > count
    public void IsTestFinished_IndexGreaterThanCount_ReturnsTrue()
    {
        Assert.True(TestScoreInterpreter.IsTestFinished(6, 5));
    }

    // ==== П4. Прогресс ====

    [Fact] // Тест 14
    public void CalculateProgress_Half_Returns50()
    {
        Assert.Equal(50.0, TestScoreInterpreter.CalculateProgress(3, 6));
    }

    [Fact] // Тест 15: граница — 0 %
    public void CalculateProgress_ZeroAnswered_ReturnsZero()
    {
        Assert.Equal(0.0, TestScoreInterpreter.CalculateProgress(0, 10));
    }

    [Fact] // Тест 16: граница — 100 %
    public void CalculateProgress_AllAnswered_Returns100()
    {
        Assert.Equal(100.0, TestScoreInterpreter.CalculateProgress(10, 10));
    }

    [Fact] // Тест 17: деление на ноль
    public void CalculateProgress_ZeroTotal_ReturnsZero()
    {
        Assert.Equal(0.0, TestScoreInterpreter.CalculateProgress(0, 0));
    }
}
