namespace Psychologist_nearby.Data;

/// <summary>
/// Тестовая подмена: всегда возвращает фиксированный результат, независимо от баллов.
/// </summary>
public class FixedScoreInterpreter : IScoreInterpreter
{
    private readonly TestInterpretation _fixedResult;

    public FixedScoreInterpreter(TestInterpretation fixedResult)
    {
        _fixedResult = fixedResult;
    }

    /// <inheritdoc />
    public TestInterpretation Interpret(Test test, int score)
    {
        return _fixedResult;
    }
}