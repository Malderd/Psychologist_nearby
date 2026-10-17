using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Psychologist_nearby.Data;

public static class AuthValidator
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 40;
    public const int MaxEmailLength = 100;
    public const int PhoneLength = 10;
    public const int MaxNameLength = 150;
    public const int ConfirmationCodeMin = 100000;
    public const int ConfirmationCodeMax = 999999;

    // П1: Email — trim + lowercase + валидный формат + ≤ 100 символов
    public static bool TryNormalizeEmail(string? raw, out string normalized, out string? error)
    {
        error = null;
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "Введите e-mail";
            return false;
        }

        normalized = raw.Trim().ToLowerInvariant();

        if (!new EmailAddressAttribute().IsValid(normalized))
        {
            error = "Введите корректный адрес электронной почты";
            return false;
        }

        if (normalized.Length > MaxEmailLength)
        {
            error = "Почта не должна содержать больше 100 символов";
            return false;
        }

        return true;
    }

    // П2: Пароль — 8–40 символов, заглавная, строчная, цифра, спецсимвол, без пробелов
    public static bool IsPasswordValid(string? password, out string? error)
    {
        error = null;

        if (string.IsNullOrEmpty(password))
        {
            error = "Введите пароль";
            return false;
        }

        if (password.Length < MinPasswordLength || password.Length >= MaxPasswordLength)
        {
            error = "Пароль должен содержать от 8 до 40 символов";
            return false;
        }

        if (!password.Any(char.IsUpper))
        {
            error = "Пароль должен содержать хотя бы одну заглавную букву";
            return false;
        }

        if (!password.Any(char.IsLower))
        {
            error = "Пароль должен содержать хотя бы одну строчную букву";
            return false;
        }

        if (!password.Any(char.IsDigit))
        {
            error = "Пароль должен содержать хотя бы одну цифру";
            return false;
        }

        if (!password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
        {
            error = "Пароль должен содержать хотя бы один специальный символ";
            return false;
        }

        if (password.Any(char.IsWhiteSpace))
        {
            error = "Пароль не должен содержать пробелы";
            return false;
        }

        return true;
    }

    // П3: Пароли совпадают
    public static bool PasswordsMatch(string? a, string? b) => a == b;

    // П4: Телефон — ровно 10 цифр или пусто
    public static bool IsPhoneValid(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return true;
        if (phone.Length != PhoneLength) return false;
        return phone.All(char.IsDigit);
    }

    // П5: Имя — только буквы (рус/англ) или пусто, ≤ 150
    public static bool IsNameValid(string? name)
    {
        if (string.IsNullOrEmpty(name)) return true;
        if (name.Length > MaxNameLength) return false;
        return Regex.IsMatch(name, @"^(?:[ \t]*[А-Яа-яЁёA-Za-z]+[ \t]*|[ \t]*)$");
    }

    // П6: Фамилия — буквы и дефис или пусто, ≤ 150
    public static bool IsSurnameValid(string? surname)
    {
        if (string.IsNullOrEmpty(surname)) return true;
        if (surname.Length > MaxNameLength) return false;
        return Regex.IsMatch(surname, @"^(?:[ \t]*[А-Яа-яЁёA-Za-z]+(?:-[А-Яа-яЁёA-Za-z]+)*[ \t]*|[ \t]*)$");
    }

    // П7: Генерация 6-значного кода подтверждения
    public static string GenerateConfirmationCode(int? seed = null)
    {
        var rnd = seed.HasValue ? new Random(seed.Value) : new Random();
        return rnd.Next(ConfirmationCodeMin, ConfirmationCodeMax + 1).ToString();
    }

    // П8: Пустое → null, иначе trim
    public static string? EmptyToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}