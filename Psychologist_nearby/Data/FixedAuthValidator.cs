namespace Psychologist_nearby.Data;

/// <summary>
/// Тестовая подмена (Stub). Всегда возвращает положительный результат.
/// Используется для проверки работы потребителей без реальной логики валидации.
/// </summary>
public class FixedAuthValidator : IAuthValidator
{
    public bool TryNormalizeEmail(string? email, out string normalizedEmail, out string? error)
    {
        normalizedEmail = "test@example.com";
        error = null;
        return true;
    }

    public bool IsPasswordValid(string? password, out string? error)
    {
        error = null;
        return true;
    }

    public bool PasswordsMatch(string? password, string? confirmPassword) => true;
    public bool IsPhoneValid(string? phone) => true;
    public bool IsNameValid(string? name) => true;
    public bool IsSurnameValid(string? surname) => true;
    public string? EmptyToNull(string? input) => input;
    public string GenerateConfirmationCode() => "123456";
}