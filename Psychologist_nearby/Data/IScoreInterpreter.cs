namespace Psychologist_nearby.Data;

/// <summary>
/// Контракт модуля интерпретации результатов тестирования.
/// </summary>
public interface IScoreInterpreter
{
    /// <summary>
    /// Выбирает интерпретацию по набранному баллу.
    /// </summary>
    /// <param name="test">Тест с диапазонами интерпретаций.</param>
    /// <param name="score">Набранный балл.</param>
    /// <returns>Найденная интерпретация или заглушка.</returns>
    TestInterpretation Interpret(Test test, int score);
}