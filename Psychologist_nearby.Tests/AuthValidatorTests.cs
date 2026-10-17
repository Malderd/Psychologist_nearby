using Psychologist_nearby.Data;

namespace Psychologist_nearby.Tests;

public class AuthValidatorTests
{
    // П1: Email

    [Fact] // 1: обычный email нормализуется
    public void TryNormalizeEmail_ValidEmail_TrimsAndLowercases()
    {
        var ok = AuthValidator.TryNormalizeEmail("  TEST@Example.COM  ", out var normalized, out var error);
        Assert.True(ok);
        Assert.Equal("test@example.com", normalized);
        Assert.Null(error);
    }

    [Fact] // 2: null/пустой → ошибка
    public void TryNormalizeEmail_Empty_ReturnsError()
    {
        var ok = AuthValidator.TryNormalizeEmail("   ", out _, out var error);
        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact] // 3: некорректный формат
    public void TryNormalizeEmail_InvalidFormat_ReturnsError()
    {
        var ok = AuthValidator.TryNormalizeEmail("not-an-email", out _, out var error);
        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact] // 4: граница — длина > 100
    public void TryNormalizeEmail_TooLong_ReturnsError()
    {
        string local = new string('a', 95);
        string email = $"{local}@example.com";
        var ok = AuthValidator.TryNormalizeEmail(email, out _, out var error);
        Assert.False(ok);
        Assert.NotNull(error);
    }

    // П2: Пароль

    [Fact] // 5: корректный пароль
    public void IsPasswordValid_StrongPassword_ReturnsTrue()
    {
        Assert.True(AuthValidator.IsPasswordValid("Strong123!", out _));
    }

    [Fact] // 6: граница — короче 8
    public void IsPasswordValid_TooShort_ReturnsFalse()
    {
        var ok = AuthValidator.IsPasswordValid("Ab1!xyz", out var error);
        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact] // 7: граница — длиннее 40
    public void IsPasswordValid_TooLong_ReturnsFalse()
    {
        string pwd = "Aa1!" + new string('x', 36);
        Assert.False(AuthValidator.IsPasswordValid(pwd, out _));
    }

    [Fact] // 8: без заглавной
    public void IsPasswordValid_NoUppercase_ReturnsFalse()
    {
        Assert.False(AuthValidator.IsPasswordValid("lower123!", out _));
    }

    [Fact] // 9: без спецсимвола
    public void IsPasswordValid_NoSpecialChar_ReturnsFalse()
    {
        Assert.False(AuthValidator.IsPasswordValid("Strong1231", out _));
    }

    [Fact] // 10: со пробелом
    public void IsPasswordValid_WithSpace_ReturnsFalse()
    {
        Assert.False(AuthValidator.IsPasswordValid("Strong 123!", out _));
    }

    // П3: Совпадение паролей

    [Fact] // 11
    public void PasswordsMatch_Equal_ReturnsTrue()
    {
        Assert.True(AuthValidator.PasswordsMatch("Abc123!@", "Abc123!@"));
    }

    [Fact] // 12
    public void PasswordsMatch_Different_ReturnsFalse()
    {
        Assert.False(AuthValidator.PasswordsMatch("Abc123!@", "Xyz123!@"));
    }

    // П4: Телефон

    [Fact] // 13
    public void IsPhoneValid_TenDigits_ReturnsTrue()
    {
        Assert.True(AuthValidator.IsPhoneValid("9123456789"));
    }

    [Fact] // 14
    public void IsPhoneValid_Empty_ReturnsTrue()
    {
        Assert.True(AuthValidator.IsPhoneValid(""));
    }

    [Fact] // 15
    public void IsPhoneValid_WrongLength_ReturnsFalse()
    {
        Assert.False(AuthValidator.IsPhoneValid("912345"));
    }

    // П5: Имя

    [Fact] // 16
    public void IsNameValid_RussianLetters_ReturnsTrue()
    {
        Assert.True(AuthValidator.IsNameValid("Константин"));
    }

    [Fact] // 17: с цифрой
    public void IsNameValid_WithDigit_ReturnsFalse()
    {
        Assert.False(AuthValidator.IsNameValid("Константин1"));
    }

    // П6: Фамилия

    [Fact] // 18
    public void IsSurnameValid_WithDash_ReturnsTrue()
    {
        Assert.True(AuthValidator.IsSurnameValid("Куликов-Петров"));
    }

    [Fact] // 19: с цифрой
    public void IsSurnameValid_WithDigit_ReturnsFalse()
    {
        Assert.False(AuthValidator.IsSurnameValid("Куликов2"));
    }

    // П7: Код подтверждения

    [Fact] // 20
    public void GenerateConfirmationCode_ReturnsSixDigitsInRange()
    {
        for (int i = 0; i < 20; i++)
        {
            var code = AuthValidator.GenerateConfirmationCode();
            Assert.Equal(6, code.Length);
            int value = int.Parse(code);
            Assert.InRange(value, 100000, 999999);
        }
    }

    // П8: EmptyToNull

    [Fact] // 21
    public void EmptyToNull_WhitespaceOnly_ReturnsNull()
    {
        Assert.Null(AuthValidator.EmptyToNull("   "));
    }

    [Fact] // 22
    public void EmptyToNull_WithSpaces_Trims()
    {
        Assert.Equal("Привет", AuthValidator.EmptyToNull("  Привет  "));
    }

    // BCrypt

    [Fact] // 23
    public void BCrypt_HashAndVerify_Works()
    {
        string password = "Test123!@#";
        string hash = BCrypt.Net.BCrypt.HashPassword(password);

        Assert.True(BCrypt.Net.BCrypt.Verify(password, hash));
        Assert.False(BCrypt.Net.BCrypt.Verify("Wrong123!", hash));
    }

}