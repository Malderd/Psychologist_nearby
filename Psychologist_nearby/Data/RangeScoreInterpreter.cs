namespace Psychologist_nearby.Data;

/// <summary>
/// Основная реализация интерпретатора (поиск по диапазонам MinScore-MaxScore).
/// </summary>
public class RangeScoreInterpreter : IScoreInterpreter
{
    /// <inheritdoc />
    public TestInterpretation Interpret(Test test, int score)
    {
        var found = test.Interpretations
            .FirstOrDefault(i => score >= i.MinScore && score <= i.MaxScore);

        return found ?? new TestInterpretation
        {
            StatusText = "Не определено",
            RecommendationText = "-"
        };
    }
}