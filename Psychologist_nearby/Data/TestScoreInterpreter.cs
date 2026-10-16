namespace Psychologist_nearby.Data
{
    public static class TestScoreInterpreter
    {
        public const string UnknownStatus = "Не определено";

        // П1: выбор интерпретации по диапазону MinScore ≤ score ≤ MaxScore
        public static TestInterpretation Interpret(Test test, int score)
        {
            var found = test.Interpretations
                .FirstOrDefault(i => score >= i.MinScore && score <= i.MaxScore);

            return found ?? new TestInterpretation
            {
                StatusText = UnknownStatus,
                RecommendationText = "-"
            };
        }

        // П2: сумма баллов по ответам
        public static int CalculateScore(IEnumerable<TestAnswer> answers)
            => answers?.Sum(a => a.Score) ?? 0;

        // П3: тест завершён, когда индекс вопроса дошёл до количества
        public static bool IsTestFinished(int currentQuestionIndex, int questionsCount)
            => currentQuestionIndex >= questionsCount;

        // П4: прогресс в процентах; при total = 0 вернуть 0
        public static double CalculateProgress(int answered, int total)
        {
            if (total <= 0) return 0;
            return (double)answered / total * 100;
        }
    }
}