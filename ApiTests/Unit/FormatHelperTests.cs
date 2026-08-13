using ApiTests.TestData;

using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

using MoopelApi.Helpers;

using MoopelObjects.Dto;
using MoopelObjects.Enums;

namespace ApiTests.Unit;

public class FormatHelperTests
{
    [Fact]
    public void FormatNullable_ReturnsNullText_WhenValueIsMissing()
    {
        // Arrange

        // Act
        string result = FormatHelper.FormatNullable(null);

        // Assert
        Assert.Equal(FormatHelperTestData.NullText, result);
    }

    [Fact]
    public void FormatDate_ReturnsUtcString_WhenValueExists()
    {
        // Arrange
        string expected = $"{FormatHelperTestData.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC";

        // Act
        string result = FormatHelper.FormatDate(FormatHelperTestData.UtcDateTime);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatHttpContext_ReturnsMethodPathAndQuery_WhenContextHasValues()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Method = FormatHelperTestData.HttpMethod;
        context.Request.Path = FormatHelperTestData.HttpPath;
        context.Request.QueryString = new QueryString(FormatHelperTestData.HttpQuery);

        string expected = $"{FormatHelperTestData.HttpMethod} {FormatHelperTestData.HttpPath}{FormatHelperTestData.HttpQuery}";

        // Act
        string result = FormatHelper.FormatHttpContext(context);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatErrorLogMessage_IncludesMessageParametersAndCommandText()
    {
        // Arrange
        using SqlCommand command = new()
        {
            CommandText = FormatHelperTestData.CommandText
        };

        List<(string Name, object? Value)> parameters =
        [
            (FormatHelperTestData.ParameterName, FormatHelperTestData.ParameterValue)
        ];

        // Act
        string result = FormatHelper.FormatErrorLogMessage(
            FormatHelperTestData.DoingWhat,
            FormatHelperTestData.Message,
            parameters,
            command);

        // Assert
        Assert.Contains($"Error {FormatHelperTestData.DoingWhat}", result);
        Assert.Contains(FormatHelperTestData.Message, result);
        Assert.Contains(FormatHelperTestData.ParameterName, result);
        Assert.Contains(FormatHelperTestData.ParameterValue, result);
        Assert.Contains(FormatHelperTestData.CommandText, result);
    }

    [Fact]
    public void GenerateUserDescriptor_ReturnsExpectedDescriptor()
    {
        // Arrange
        User user = new()
        {
            UserId = FormatHelperTestData.UserId,
            Username = FormatHelperTestData.UserName,
            PasswordHash = "hash",
            Role = UserRole.Standard,
            CreatedAtUtc = FormatHelperTestData.UtcDateTime,
            Deactivated = false,
            ThemeId = null
        };

        string expected = $"User [{FormatHelperTestData.UserId}] - {FormatHelperTestData.UserName}";

        // Act
        string result = FormatHelper.GenerateUserDescriptor(user);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsDescriptorForUser_ReturnsTrue_WhenDescriptorMatchesUser()
    {
        // Arrange
        User user = new()
        {
            UserId = FormatHelperTestData.UserId,
            Username = FormatHelperTestData.UserName,
            PasswordHash = "hash",
            Role = UserRole.Standard,
            CreatedAtUtc = FormatHelperTestData.UtcDateTime,
            Deactivated = false,
            ThemeId = null
        };

        string descriptor = $"User [{FormatHelperTestData.UserId}] - something else";

        // Act
        bool result = FormatHelper.IsDescriptorForUser(descriptor, user);

        // Assert
        Assert.True(result);
    }
}
