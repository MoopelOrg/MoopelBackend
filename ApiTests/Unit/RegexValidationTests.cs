using ApiTests.TestData;
using MoopelApi.Helpers;

using static MoopelApi.ConstantValues;

namespace ApiTests.Unit;

public class RegexValidationTests
{
    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UsernamePasswordRegex_ShouldMatchValidInput(string username, string password)
    {
        // Arrange
        bool expectedUsernameValidationResult = true;
        bool expectedPasswordValidationResult = true;

        // Act 
        bool usernameValidationResult = RegexHelper.CheckWithRegex(username, ValidationRegex.UsernameRegex);
        bool passwordValidationResult = RegexHelper.CheckWithRegex(password, ValidationRegex.PasswordRegex);

        // Assert
        Assert.True(expectedUsernameValidationResult == usernameValidationResult,
            $"Expected username validation result {expectedUsernameValidationResult}, actual {usernameValidationResult} for username '{username}'.");
        Assert.True(expectedPasswordValidationResult == passwordValidationResult,
            $"Expected password validation result {expectedPasswordValidationResult}, actual {passwordValidationResult} for username '{username}'.");
    }

    [Theory]
    [MemberData(nameof(AuthTestData.InValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UsernamePasswordRegex_ShouldRejectInvalidInput(string username, string password, string reason)
    {
        // Arrange

        // Act 
        bool usernameValidationResult = RegexHelper.CheckWithRegex(username, ValidationRegex.UsernameRegex);
        bool passwordValidationResult = RegexHelper.CheckWithRegex(password, ValidationRegex.PasswordRegex);
        bool oneFailed = !usernameValidationResult || !passwordValidationResult;


        // Assert
        Assert.True(oneFailed, reason);
    }
}
