using System.Text.RegularExpressions;

namespace Psychologist_nearby.Data;

public class RegexAuthValidator : IAuthValidator
{
    public bool TryNormalizeEmail(string? email, out string normalizedEmail, out string? error)
    {
        normalizedEmail = string.Empty;
        error = null;

        if (string.IsNullOrWhiteSpace(email))
        {
            error = "Введите e-mail.";
            return false;
        }

        var trimmed = email.Trim().ToLowerInvariant();

        if (trimmed.Length > 100)
        {
            error = "E-mail не должен превышать 100 символов.";
            return false;
        }

        // Простая проверка формата
        if (!Regex.IsMatch(trimmed, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            error = "Введите корректный адрес электронной почты.";
            return false;
        }

        normalizedEmail = trimmed;
        return true;
    }

    public bool IsPasswordValid(string? password, out string? error)
    {
        error = null;
        if (string.IsNullOrEmpty(password)) { error = "Введите пароль."; return false; }
        if (password.Length < 8 || password.Length > 40) { error = "Пароль должен быть от 8 до 40 символов."; return false; }
        if (password.Contains(' ')) { error = "Пароль не должен содержать пробелов."; return false; }
        if (!password.Any(char.IsUpper)) { error = "Пароль должен содержать заглавную букву."; return false; }
        if (!password.Any(char.IsLower)) { error = "Пароль должен содержать строчную букву."; return false; }
        if (!password.Any(char.IsDigit)) { error = "Пароль должен содержать цифру."; return false; }
        if (password.All(char.IsLetterOrDigit)) { error = "Пароль должен содержать спецсимвол."; return false; }

        return true;
    }

    public bool PasswordsMatch(string? password, string? confirmPassword)
        => password == confirmPassword;

    public bool IsPhoneValid(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return true; // Пустой телефон разрешен
        return Regex.IsMatch(phone.Trim(), @"^\d{10}$");
    }

    public bool IsNameValid(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;
        return Regex.IsMatch(name.Trim(), @"^[a-zA-Zа-яА-ЯёЁ\s-]{1,150}$");
    }

    public bool IsSurnameValid(string? surname)
    {
        if (string.IsNullOrWhiteSpace(surname)) return true;
        return Regex.IsMatch(surname.Trim(), @"^[a-zA-Zа-яА-ЯёЁ-]{1,150}$");
    }

    public string? EmptyToNull(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        return input.Trim();
    }

    public string GenerateConfirmationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}