namespace Psychologist_nearby.Data;

public static class TestScoreInterpreter
{
    public const string UnknownStatus = "Не определено";

    public static TestInterpretation Interpret(Test test, int score)
        => new RangeScoreInterpreter().Interpret(test, score);

    public static int CalculateScore(IEnumerable<TestAnswer> answers)
        => answers?.Sum(a => a.Score) ?? 0;

    public static bool IsTestFinished(int currentQuestionIndex, int questionsCount)
        => currentQuestionIndex >= questionsCount;

    public static double CalculateProgress(int answered, int total)
    {
        if (total <= 0) return 0;
        return (double)answered / total * 100;
    }
}