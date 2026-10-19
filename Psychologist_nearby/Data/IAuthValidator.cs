namespace Psychologist_nearby.Data;

/// <summary>
/// Контракт модуля валидации и нормализации пользовательских данных.
/// </summary>
public interface IAuthValidator
{
    /// <summary>
    /// Нормализует email (trim + lowercase) и проверяет его формат.
    /// </summary>
    /// <param name="email">Входная строка email.</param>
    /// <param name="normalizedEmail">Нормализованный email при успехе, иначе пустая строка.</param>
    /// <param name="error">Текст ошибки при неудаче, иначе null.</param>
    /// <returns>True, если email валиден.</returns>
    bool TryNormalizeEmail(string? email, out string normalizedEmail, out string? error);

    /// <summary>
    /// Проверяет сложность пароля (длина, заглавные, строчные, цифры, спецсимволы, без пробелов).
    /// </summary>
    bool IsPasswordValid(string? password, out string? error);

    /// <summary>
    /// Проверяет совпадение паролей.
    /// </summary>
    bool PasswordsMatch(string? password, string? confirmPassword);

    /// <summary>
    /// Проверяет формат номера телефона.
    /// </summary>
    bool IsPhoneValid(string? phone);

    /// <summary>
    /// Проверяет корректность имени.
    /// </summary>
    bool IsNameValid(string? name);

    /// <summary>
    /// Проверяет корректность фамилии.
    /// </summary>
    bool IsSurnameValid(string? surname);

    /// <summary>
    /// Преобразует пустую строку или строку из пробелов в null, иначе делает trim.
    /// </summary>
    string? EmptyToNull(string? input);

    /// <summary>
    /// Генерирует 6-значный код подтверждения.
    /// </summary>
    string GenerateConfirmationCode();
}