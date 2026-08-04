using ApiTests.TestData;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using MoopelApi.Exceptions;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services;

using static MoopelApi.ConstantValues.Database;

namespace ApiTests.Unit;

public class UserServiceTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public UserServiceTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region GetUsers

    [Fact]
    public async Task GetUsers_ReturnsUsers_WhenUsersExist()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        await InsertUserAsync(UserServiceTestData.PrimaryUsername, UserServiceTestData.PrimaryEmail);

        // Act
        List<User> users = await userService.GetUsers(_cancelToken);

        // Assert
        Assert.NotEmpty(users);
        Assert.Contains(users, user => user.Username == UserServiceTestData.PrimaryUsername);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyList_WhenCancelled()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);
        CancellationToken cancelToken = new(true);

        // Act
        List<User> users = await userService.GetUsers(cancelToken);

        // Assert
        Assert.Empty(users);
    }

    #endregion

    #region GetUserFromUserId

    [Fact]
    public async Task GetUserFromUserId_ReturnsUser_WhenFound()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(UserServiceTestData.PrimaryUsername, UserServiceTestData.PrimaryEmail);

        // Act
        User? user = await userService.GetUserFromUserId(userId);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(UserServiceTestData.PrimaryUsername, user!.Username);
        Assert.Equal(UserServiceTestData.PrimaryEmail, user.Email);
        Assert.Equal(userId, user.UserId);
    }

    [Fact]
    public async Task GetUserFromUserId_ReturnsNull_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        // Act
        User? user = await userService.GetUserFromUserId(UserTestData.MissingUserId);

        // Assert
        Assert.Null(user);
    }

    #endregion

    #region GetUserFromUserName

    [Fact]
    public async Task GetUserFromUserName_ReturnsUser_WhenFound()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(UserServiceTestData.PrimaryUsername, UserServiceTestData.PrimaryEmail);

        // Act
        User? user = await userService.GetUserFromUserName(UserServiceTestData.PrimaryUsername);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(userId, user!.UserId);
        Assert.Equal(UserServiceTestData.PrimaryUsername, user.Username);
        Assert.Equal(UserServiceTestData.PrimaryEmail, user.Email);
    }

    [Fact]
    public async Task GetUserFromUserName_ReturnsNull_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        // Act
        User? user = await userService.GetUserFromUserName(UserTestData.MissingUsername);

        // Assert
        Assert.Null(user);
    }

    #endregion

    #region UpdateUser

    [Fact]
    public async Task UpdateUser_ReturnsUpdatedUser_WhenUserExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(UserServiceTestData.PrimaryUsername, UserServiceTestData.PrimaryEmail);
        UserUpdateRequest updateRequest = new()
        {
            Email = UserServiceTestData.UpdatedEmail
        };

        // Act
        User? updatedUser = await userService.UpdateUser(updateRequest, userId);

        // Assert
        Assert.NotNull(updatedUser);
        Assert.Equal(userId, updatedUser!.UserId);
        Assert.Equal(UserServiceTestData.PrimaryUsername, updatedUser.Username);
        Assert.Equal(UserServiceTestData.UpdatedEmail, updatedUser.Email);
    }

    [Fact]
    public async Task UpdateUser_ThrowsNotFoundException_WhenUserIsMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        UserUpdateRequest updateRequest = new()
        {
            Email = UserServiceTestData.UpdatedEmail
        };

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => userService.UpdateUser(updateRequest, UserTestData.MissingUserId));
    }

    #endregion

    #region DisableUser

    [Fact]
    public async Task DisableUser_DisablesUser_WhenUserExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(UserServiceTestData.PrimaryUsername, UserServiceTestData.PrimaryEmail);

        // Act
        await userService.DisableUser(userId);
        User? disabledUser = await userService.GetUserFromUserId(userId);

        // Assert
        Assert.NotNull(disabledUser);
        Assert.True(disabledUser!.Deactivated);
    }

    [Fact]
    public async Task DisableUser_Throws_WhenUserIsMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => userService.DisableUser(UserTestData.MissingUserId));
    }

    #endregion

    #region User settings

    [Fact]
    public async Task GetSettingsFromUserId_ReturnsSettings_WhenCreated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(UserServiceTestData.PrimaryUsername, UserServiceTestData.PrimaryEmail);
        UserSettings created = await userService.CreateUserSettings(CreateSettingsRequest(), userId);

        // Act
        UserSettings? settings = await userService.GetSettingsFromUserId(userId);

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(created.SettingId, settings!.SettingId);
        Assert.Equal(userId, settings.UserId);
        Assert.Equal(UserSettingsTestData.InitialTimezone, settings.Timezone);
        Assert.Equal(UserSettingsTestData.InitialDateFormat, settings.DateFormat);
    }

    [Fact]
    public async Task GetSettingsFromUserId_ReturnsNull_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        // Act
        UserSettings? settings = await userService.GetSettingsFromUserId(UserTestData.MissingUserId);

        // Assert
        Assert.Null(settings);
    }

    [Fact]
    public async Task GetSettingsFromSettingId_ReturnsSettings_WhenCreated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(UserServiceTestData.PrimaryUsername, UserServiceTestData.PrimaryEmail);
        UserSettings created = await userService.CreateUserSettings(CreateSettingsRequest(), userId);

        // Act
        UserSettings? settings = await userService.GetSettingsFromSettingId(created.SettingId);

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(created.SettingId, settings!.SettingId);
        Assert.Equal(userId, settings.UserId);
    }

    [Fact]
    public async Task GetSettingsFromSettingId_ReturnsNull_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        // Act
        UserSettings? settings = await userService.GetSettingsFromSettingId(UserSettingsTestData.MissingSettingId);

        // Assert
        Assert.Null(settings);
    }

    [Fact]
    public async Task CreateUserSettings_ReturnsSettings_WhenValid()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(UserServiceTestData.PrimaryUsername, UserServiceTestData.PrimaryEmail);

        // Act
        UserSettings settings = await userService.CreateUserSettings(CreateSettingsRequest(), userId);

        // Assert
        Assert.Equal(userId, settings.UserId);
        Assert.Equal(UserSettingsTestData.InitialTimezone, settings.Timezone);
        Assert.Equal(UserSettingsTestData.InitialDateFormat, settings.DateFormat);
        Assert.True(settings.SettingId > 0);
    }

    [Fact]
    public async Task UpdateUserSettings_ReturnsUpdatedSettings_WhenExisting()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        int userId = await InsertUserAsync(UserServiceTestData.PrimaryUsername, UserServiceTestData.PrimaryEmail);
        UserSettings created = await userService.CreateUserSettings(CreateSettingsRequest(), userId);

        // Act
        UserSettings? updated = await userService.UpdateUserSettings(UserSettingsTestData.ValidUpdateRequest, created.SettingId);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(created.SettingId, updated!.SettingId);
        Assert.Equal(userId, updated.UserId);
        Assert.Equal(UserSettingsTestData.UpdatedTimezone, updated.Timezone);
        Assert.Equal(UserSettingsTestData.UpdatedDateFormat, updated.DateFormat);
    }

    [Fact]
    public async Task UpdateUserSettings_ThrowsNotFoundException_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        UserService userService = ActivatorUtilities.CreateInstance<UserService>(scope.ServiceProvider);

        // Act / Assert
        await Assert.ThrowsAsync<NotFoundException>(() => userService.UpdateUserSettings(UserSettingsTestData.ValidUpdateRequest, UserSettingsTestData.MissingSettingId));
    }

    #endregion

    private static UserSettingsCreateRequest CreateSettingsRequest()
    {
        return new()
        {
            Timezone = UserSettingsTestData.InitialTimezone,
            DateFormat = UserSettingsTestData.InitialDateFormat
        };
    }

    private async Task<int> InsertUserAsync(string username, string? email)
    {
        await using SqlConnection connection = new(_apiFactory.ConnectionString);
        await connection.OpenAsync(_cancelToken);

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
             INSERT INTO {UsersTable.SelectName}
             (
                 {UserNameField.SelectName},
                 {UserPasswordHashField.SelectName},
                 {UserRoleField.SelectName},
                 {UserEmailField.SelectName},
                 {UserDeactivatedField.SelectName},
                 {UserThemeIdField.SelectName}
             )
             OUTPUT INSERTED.{UserIdField.SelectName}
             VALUES
             (
                 @Username,
                 @PasswordHash,
                 @Role,
                 @Email,
                 @Deactivated,
                 @ThemeId
             );
             """;
        command.Parameters.Add(UserNameField.GenerateParameter("@Username", username));
        command.Parameters.Add(UserPasswordHashField.GenerateParameter("@PasswordHash", UserServiceTestData.PasswordHash));
        command.Parameters.Add(UserRoleField.GenerateParameter("@Role", "Standard"));
        command.Parameters.Add(UserEmailField.GenerateParameter("@Email", email));
        command.Parameters.Add(UserDeactivatedField.GenerateParameter("@Deactivated", false));
        command.Parameters.Add(UserThemeIdField.GenerateParameter("@ThemeId", null));

        object? insertedUserId = await command.ExecuteScalarAsync(_cancelToken);
        return Convert.ToInt32(insertedUserId);
    }
}
