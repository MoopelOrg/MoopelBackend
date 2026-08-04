using System.Net;
using System.Net.Http.Json;

using ApiTests.TestData;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using MoopelApi;
using MoopelApi.Helpers;
using MoopelApi.Models;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests;
using MoopelApi.Models.Results;
using MoopelApi.Services;

using static MoopelApi.ConstantValues.Database;

namespace ApiTests.Unit;

public class AuthServiceTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public AuthServiceTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region CanUserUpdateTheme

    [Fact]
    public async Task CanUserUpdateTheme_ReturnsTrue_WhenUserIsAdmin()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        User user = CreateUser(AuthServiceTestData.AdminUserId, AuthServiceTestData.AdminUserName, UserRole.Admin);
        Theme theme = CreateTheme(AuthServiceTestData.PrimaryThemeId, CreateUser(AuthServiceTestData.ThemeOwnerUserId, AuthServiceTestData.ThemeOwnerUserName, UserRole.Standard));

        // Act
        bool result = await authService.CanUserUpdateTheme(user, theme);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserUpdateTheme_ReturnsTrue_WhenThemeBelongsToUser()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        User user = CreateUser(AuthServiceTestData.ThemeOwnerUserId, AuthServiceTestData.ThemeOwnerUserName, UserRole.Standard);
        Theme theme = CreateTheme(AuthServiceTestData.PrimaryThemeId, user);

        // Act
        bool result = await authService.CanUserUpdateTheme(user, theme);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserUpdateTheme_ReturnsFalse_WhenUserIsNotAdminAndThemeDoesNotMatch()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        User user = CreateUser(AuthServiceTestData.ThemeEditorUserId, AuthServiceTestData.ThemeEditorUserName, UserRole.Standard);
        Theme theme = CreateTheme(AuthServiceTestData.PrimaryThemeId, CreateUser(AuthServiceTestData.OtherUserId, AuthServiceTestData.OtherUserName, UserRole.Standard));

        // Act
        bool result = await authService.CanUserUpdateTheme(user, theme);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Register

    [Fact]
    public async Task Register_ReturnsUser_WhenCredentialsAreValid()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        RegistrationRequest request = new()
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = AuthServiceTestData.RegisterPassword
        };

        // Act
        User? result = await authService.Register(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AuthServiceTestData.RegisterUsername, result.Username);
        Assert.Equal(UserRole.Standard, result.Role);
        Assert.False(result.Deactivated);
        Assert.Null(result.ThemeId);
        Assert.False(string.IsNullOrWhiteSpace(result.PasswordHash));
        Assert.True(result.UserId > 0);
    }

    [Fact]
    public async Task Register_ReturnsNull_WhenUsernameStartsWithGuestPrefix()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        RegistrationRequest request = new()
        {
            Username = AuthServiceTestData.GuestUsername,
            Password = AuthServiceTestData.RegisterPassword
        };

        // Act
        User? result = await authService.Register(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Register_ReturnsNull_WhenUsernameAlreadyExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        RegistrationRequest request = new()
        {
            Username = AuthServiceTestData.DuplicateUsername,
            Password = AuthServiceTestData.RegisterPassword
        };

        // Act
        User? firstResult = await authService.Register(request);
        User? secondResult = await authService.Register(request);

        // Assert
        Assert.NotNull(firstResult);
        Assert.Null(secondResult);
    }

    #endregion

    #region Login

    [Fact]
    public async Task Login_ReturnsTokenAndSession_WhenCredentialsAreValid()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        RegistrationRequest registerRequest = new()
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = AuthServiceTestData.RegisterPassword
        };

        RequestContext requestContext = CreateRequestContext();
        LoginRequest loginRequest = new()
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = AuthServiceTestData.RegisterPassword
        };

        await authService.Register(registerRequest);

        // Act
        LoginResult? result = await authService.Login(loginRequest, requestContext, _cancelToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.NotNull(result.Login);
        Assert.NotNull(result.Session);
        Assert.Null(result.Error);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal(AuthServiceTestData.RegisterUsername, result.User!.Username);
    }

    [Fact]
    public async Task Login_ReturnsErrorAndNoToken_WhenUserDoesNotExist()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        LoginRequest request = new()
        {
            Username = UserTestData.MissingUsername,
            Password = AuthServiceTestData.RegisterPassword
        };

        // Act
        LoginResult? result = await authService.Login(request, CreateRequestContext(), _cancelToken);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.User);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
        Assert.True(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task Login_ReturnsErrorAndNoToken_WhenPasswordIsIncorrect()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        await authService.Register(new RegistrationRequest
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = AuthServiceTestData.RegisterPassword
        });

        LoginRequest request = new()
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = $"{AuthServiceTestData.RegisterPassword}{AuthServiceTestData.InvalidPasswordSuffix}"
        };

        // Act
        LoginResult? result = await authService.Login(request, CreateRequestContext(), _cancelToken);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.User);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
        Assert.True(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task Login_ReturnsErrorAndNoToken_WhenUserIsDeactivated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        User? user = await authService.Register(new RegistrationRequest
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = AuthServiceTestData.RegisterPassword
        });

        Assert.NotNull(user);
        bool deactivated = await authService.DeactivateUser(user!.UserId);

        LoginRequest request = new()
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = AuthServiceTestData.RegisterPassword
        };

        // Act
        LoginResult? result = await authService.Login(request, CreateRequestContext(), _cancelToken);

        // Assert
        Assert.True(deactivated);
        Assert.NotNull(result);
        Assert.Null(result.User);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
        Assert.True(string.IsNullOrWhiteSpace(result.Token));
    }

    #endregion

    #region GuestLogin

    [Fact]
    public async Task GuestLogin_ReturnsGuestUserAndSession_WhenRequested()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        // Act
        LoginResult? result = await authService.GuestLogin(CreateRequestContext(), _cancelToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.NotNull(result.Session);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.StartsWith(AuthServiceTestData.GuestPrefix, result.User!.Username);
        Assert.Equal(Roles.Guest, result.User.Role);
    }

    #endregion

    #region DeactivateUser

    [Fact]
    public async Task DeactivateUser_ReturnsTrue_WhenUserExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        User? user = await authService.Register(new RegistrationRequest
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = AuthServiceTestData.RegisterPassword
        });

        Assert.NotNull(user);

        // Act
        bool result = await authService.DeactivateUser(user!.UserId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeactivateUser_ReturnsFalse_WhenUserDoesNotExist()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        // Act
        bool result = await authService.DeactivateUser(UserTestData.MissingUserId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetUserGroupAccess

    [Fact]
    public async Task GetUserGroupAccess_ReturnsNoAccess_WhenUserIsNotInGroup()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        User? user = await authService.Register(new RegistrationRequest
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = AuthServiceTestData.RegisterPassword
        });

        Assert.NotNull(user);

        Group group = new()
        {
            GroupId = AuthServiceTestData.MissingGroupId,
            Name = AuthServiceTestData.GroupName,
            Description = AuthServiceTestData.GroupDescription,
            CreatedAtUtc = AuthServiceTestData.RequestedAtUtc
        };

        // Act
        GroupAccess result = await authService.GetUserGroupAccess(user!, group);

        // Assert
        Assert.False(result.IsGroupMember);
        Assert.False(result.CanCreateWorkItems);
        Assert.False(result.CanViewWorkItems);
        Assert.False(result.CanUpdateWorkItems);
        Assert.False(result.CanDeleteWorkItems);
        Assert.False(result.CanCreateTags);
        Assert.False(result.CanUpdateTags);
        Assert.False(result.CanDeleteTags);
    }

    [Fact]
    public async Task GetUserGroupAccess_ReturnsAccess_WhenMembershipExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        User? user = await authService.Register(new RegistrationRequest
        {
            Username = AuthServiceTestData.RegisterUsername,
            Password = AuthServiceTestData.RegisterPassword
        });

        Assert.NotNull(user);

        int groupId = await InsertGroupAsync(
            AuthServiceTestData.GroupName,
            AuthServiceTestData.GroupDescription);

        await InsertGroupUserAsync(groupId, user!.UserId);

        Group group = new()
        {
            GroupId = groupId,
            Name = AuthServiceTestData.GroupName,
            Description = AuthServiceTestData.GroupDescription,
            CreatedAtUtc = AuthServiceTestData.RequestedAtUtc
        };

        // Act
        GroupAccess result = await authService.GetUserGroupAccess(user, group);

        // Assert
        Assert.True(result.IsGroupMember);
        Assert.True(result.CanViewWorkItems);
        Assert.True(result.CanCreateWorkItems);
        Assert.True(result.CanUpdateWorkItems);
        Assert.True(result.CanDeleteWorkItems);
        Assert.True(result.CanCreateTags);
        Assert.True(result.CanUpdateTags);
        Assert.True(result.CanDeleteTags);
    }

    #endregion

    #region Not implemented

    [Fact]
    public async Task LogoutUser_ThrowsNotImplementedException()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        // Act / Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => authService.LogoutUser(AuthServiceTestData.MissingUserId));
    }

    [Fact]
    public async Task GetGroupAccess_ThrowsNotImplementedException()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        AuthService authService = ActivatorUtilities.CreateInstance<AuthService>(scope.ServiceProvider);

        Group group = new()
        {
            GroupId = AuthServiceTestData.MissingGroupId,
            Name = AuthServiceTestData.GroupName,
            Description = AuthServiceTestData.GroupDescription,
            CreatedAtUtc = AuthServiceTestData.RequestedAtUtc
        };

        // Act / Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => authService.GetGroupAccess(group));
    }

    #endregion

    private static User CreateUser(int userId, string username, UserRole role)
    {
        return new()
        {
            UserId = userId,
            Username = username,
            PasswordHash = "hash",
            Role = role,
            CreatedAtUtc = AuthServiceTestData.RequestedAtUtc,
            Deactivated = false,
            ThemeId = null
        };
    }

    private static Theme CreateTheme(int themeId, User user)
    {
        return new()
        {
            ThemeId = themeId,
            CreatorDescriptor = FormatHelper.GenerateUserDescriptor(user),
            Name = AuthServiceTestData.ThemeName,
            Data = AuthServiceTestData.ThemeData,
            Description = AuthServiceTestData.ThemeDescription,
            IsDark = false,
            CreatedAtUtc = AuthServiceTestData.RequestedAtUtc,
            IsDeleted = false
        };
    }

    private static RequestContext CreateRequestContext()
    {
        return new()
        {
            UserId = AuthServiceTestData.RequestUserId,
            Username = AuthServiceTestData.RegisterUsername,
            Email = null,
            Role = UserRole.Standard,
            IpAddress = AuthServiceTestData.RequestIpAddress,
            UserAgent = AuthServiceTestData.RequestUserAgent,
            TraceId = AuthServiceTestData.RequestTraceId,
            RequestPath = AuthServiceTestData.RequestPath,
            RequestMethod = AuthServiceTestData.RequestMethod,
            RequestedAtUtc = AuthServiceTestData.RequestedAtUtc
        };
    }

    private async Task<int> InsertGroupAsync(string name, string? description)
    {
        await using SqlConnection connection = new(_apiFactory.ConnectionString);
        await connection.OpenAsync(_cancelToken);

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
             INSERT INTO {GroupsTable.SelectName}
             ({GroupNameField.SelectName}, {GroupDescriptionField.SelectName})
             OUTPUT INSERTED.{GroupIdField.SelectName}
             VALUES (@Name, @Description);
             """;
        command.Parameters.Add(GroupNameField.GenerateParameter("@Name", name));
        command.Parameters.Add(GroupDescriptionField.GenerateParameter("@Description", description));

        object? insertedGroupId = await command.ExecuteScalarAsync(_cancelToken);
        return Convert.ToInt32(insertedGroupId);
    }

    private async Task InsertGroupUserAsync(int groupId, int userId)
    {
        await using SqlConnection connection = new(_apiFactory.ConnectionString);
        await connection.OpenAsync(_cancelToken);

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
             INSERT INTO {GroupUsersTable.SelectName}
             (
                 {GroupUsersGroupIdField.SelectName},
                 {GroupUsersUserIdField.SelectName},
                 {GroupUsersCanReadWorkItemsField.SelectName},
                 {GroupUsersCanCreateWorkItemsField.SelectName},
                 {GroupUsersCanUpdateWorkItemsField.SelectName},
                 {GroupUsersCanDeleteWorkItemsField.SelectName},
                 {GroupUsersCanCreateTagsField.SelectName},
                 {GroupUsersCanUpdateTagsField.SelectName},
                 {GroupUsersCanDeleteTagsField.SelectName},
                 {GroupUsersIsAdminField.SelectName},
                 {GroupUsersIsOwnerField.SelectName}
             )
             VALUES
             (
                 @GroupId,
                 @UserId,
                 @CanReadWorkItems,
                 @CanCreateWorkItems,
                 @CanUpdateWorkItems,
                 @CanDeleteWorkItems,
                 @CanCreateTags,
                 @CanUpdateTags,
                 @CanDeleteTags,
                 @IsAdmin,
                 @IsOwner
             );
             """;
        command.Parameters.Add(GroupUsersGroupIdField.GenerateParameter("@GroupId", groupId));
        command.Parameters.Add(GroupUsersUserIdField.GenerateParameter("@UserId", userId));
        command.Parameters.Add(GroupUsersCanReadWorkItemsField.GenerateParameter("@CanReadWorkItems", true));
        command.Parameters.Add(GroupUsersCanCreateWorkItemsField.GenerateParameter("@CanCreateWorkItems", true));
        command.Parameters.Add(GroupUsersCanUpdateWorkItemsField.GenerateParameter("@CanUpdateWorkItems", true));
        command.Parameters.Add(GroupUsersCanDeleteWorkItemsField.GenerateParameter("@CanDeleteWorkItems", true));
        command.Parameters.Add(GroupUsersCanCreateTagsField.GenerateParameter("@CanCreateTags", true));
        command.Parameters.Add(GroupUsersCanUpdateTagsField.GenerateParameter("@CanUpdateTags", true));
        command.Parameters.Add(GroupUsersCanDeleteTagsField.GenerateParameter("@CanDeleteTags", true));
        command.Parameters.Add(GroupUsersIsAdminField.GenerateParameter("@IsAdmin", true));
        command.Parameters.Add(GroupUsersIsOwnerField.GenerateParameter("@IsOwner", true));

        await command.ExecuteNonQueryAsync(_cancelToken);
    }
}
