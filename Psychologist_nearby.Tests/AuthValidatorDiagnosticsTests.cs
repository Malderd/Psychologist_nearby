using Psychologist_nearby.Data;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Psychologist_nearby.Tests;

public class AuthValidatorDiagnosticsTests
{
    private readonly ITestOutputHelper _output;

    public AuthValidatorDiagnosticsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private void RunOriginalTests()
    {
        var tests = new AuthValidatorTests();

        tests.TryNormalizeEmail_ValidEmail_TrimsAndLowercases();
        tests.TryNormalizeEmail_Empty_ReturnsError();
        tests.TryNormalizeEmail_InvalidFormat_ReturnsError();
        tests.TryNormalizeEmail_TooLong_ReturnsError();

        tests.IsPasswordValid_StrongPassword_ReturnsTrue();
        tests.IsPasswordValid_TooShort_ReturnsFalse();
        tests.IsPasswordValid_TooLong_ReturnsFalse();
        tests.IsPasswordValid_NoUppercase_ReturnsFalse();
        tests.IsPasswordValid_NoSpecialChar_ReturnsFalse();
        tests.IsPasswordValid_WithSpace_ReturnsFalse();

        tests.PasswordsMatch_Equal_ReturnsTrue();
        tests.PasswordsMatch_Different_ReturnsFalse();

        tests.IsPhoneValid_CorrectFormat_ReturnsTrue();
        tests.IsPhoneValid_Empty_ReturnsTrue();
        tests.IsPhoneValid_TenDigits_ReturnsFalse();

        tests.IsNameValid_RussianLetters_ReturnsTrue();
        tests.IsNameValid_WithDigit_ReturnsFalse();

        tests.IsSurnameValid_WithDash_ReturnsTrue();
        tests.IsSurnameValid_WithDigit_ReturnsFalse();

        tests.GenerateConfirmationCode_ReturnsSixDigitsInRange();

        tests.EmptyToNull_WhitespaceOnly_ReturnsNull();
        tests.EmptyToNull_WithSpaces_Trims();

        tests.BCrypt_HashAndVerify_Works();

        tests.IsPasswordValid_MaxLength40_ReturnsTrue();
        tests.RegexAuthValidator_MaxLength40_ReturnsTrue();
    }

    [Fact]
    public async Task RepeatedValidation_ResourceDiagnostics()
    {
        using var process = Process.GetCurrentProcess();

        // Предварительный прогрев
        RunOriginalTests();

        process.Refresh();

        long memoryBefore = process.WorkingSet64;
        int threadsBefore = process.Threads.Count;
        int handlesBefore = process.HandleCount;

        var stopwatch = Stopwatch.StartNew();

        // Повторное выполнение существующих тестов
        for (int i = 0; i < 10; i++)
        {
            RunOriginalTests();
        }

        // Дополнительный сценарий ошибочных данных
        for (int i = 0; i < 10000; i++)
        {
            Assert.False(
                AuthValidator.TryNormalizeEmail(
                    null, out _, out _));

            Assert.False(
                AuthValidator.TryNormalizeEmail(
                    "invalid-email", out _, out _));

            Assert.False(
                AuthValidator.IsPasswordValid(
                    null, out _));

            Assert.False(
                AuthValidator.IsPasswordValid(
                    "123", out _));

            Assert.False(
                AuthValidator.IsPhoneValid("123"));

            Assert.False(
                AuthValidator.IsNameValid("Name123"));

            Assert.False(
                AuthValidator.IsSurnameValid("Surname123"));
        }

        stopwatch.Stop();

        // Ожидание освобождения ресурсов
        await Task.Delay(30000);

        process.Refresh();

        _output.WriteLine($"Process ID: {process.Id}");
        _output.WriteLine("Original test calls: 250");
        _output.WriteLine("Invalid input checks: 70000");

        _output.WriteLine(
            $"Memory before: {memoryBefore / 1048576.0:F2} MiB");

        _output.WriteLine(
            $"Memory after: {process.WorkingSet64 / 1048576.0:F2} MiB");

        _output.WriteLine(
            $"Threads: {threadsBefore} -> {process.Threads.Count}");

        _output.WriteLine(
            $"Handles: {handlesBefore} -> {process.HandleCount}");

        _output.WriteLine(
            $"Execution time: {stopwatch.Elapsed.TotalSeconds:F2} s");
    }
}