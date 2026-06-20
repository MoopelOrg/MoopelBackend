using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Attributes;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.Integration;

public class UserSettingsControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public UserSettingsControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region Get My Settings

    [Fact]
    [TestDocumentation("Get my settings should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task MySettings_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.UserSettings.MySettings, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MySettings_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.UserSettings.MySettings, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MySettings_ReturnsSettings_WhenCreated(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UserSettings created = await CreateOrUpdateSettingsAsync(client, UserSettingsTestData.InitialTimezone, UserSettingsTestData.InitialDateFormat);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.UserSettings.MySettings, _cancelToken);
        UserSettings? result = await response.Content.ReadFromJsonAsync<UserSettings>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.SettingId, result!.SettingId);
        Assert.Equal(created.UserId, result.UserId);
        Assert.Equal(UserSettingsTestData.InitialTimezone, result.Timezone);
        Assert.Equal(UserSettingsTestData.InitialDateFormat, result.DateFormat);
    }

    #endregion

    #region Get Settings By Id

    [Fact]
    [TestDocumentation("Get settings by id should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task SettingsById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.UserSettings.Generate.SettingsById(UserSettingsTestData.MissingSettingId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SettingsById_ReturnsSettings_WhenOwned(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UserSettings created = await CreateOrUpdateSettingsAsync(client, UserSettingsTestData.InitialTimezone, UserSettingsTestData.InitialDateFormat);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.UserSettings.Generate.SettingsById(created.SettingId), _cancelToken);
        UserSettings? result = await response.Content.ReadFromJsonAsync<UserSettings>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.SettingId, result!.SettingId);
        Assert.Equal(created.UserId, result.UserId);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SettingsById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.UserSettings.Generate.SettingsById(UserSettingsTestData.MissingSettingId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersSettings()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        UserSettings created = await CreateOrUpdateSettingsAsync(user1, UserSettingsTestData.InitialTimezone, UserSettingsTestData.InitialDateFormat);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.UserSettings.Generate.SettingsById(created.SettingId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion

    #region Update Settings

    [Fact]
    [TestDocumentation("Update settings should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task UpdateSettings_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.UserSettings.UpdateSettings, UserSettingsTestData.ValidUpdateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateSettings_UpdatesSettings_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UserSettings created = await CreateOrUpdateSettingsAsync(client, UserSettingsTestData.InitialTimezone, UserSettingsTestData.InitialDateFormat);

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.UserSettings.UpdateSettings, UserSettingsTestData.ValidUpdateRequest, _cancelToken);
        UserSettings? updated = await response.Content.ReadFromJsonAsync<UserSettings>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.SettingId, updated!.SettingId);
        Assert.Equal(created.UserId, updated.UserId);
        Assert.Equal(UserSettingsTestData.UpdatedTimezone, updated.Timezone);
        Assert.Equal(UserSettingsTestData.UpdatedDateFormat, updated.DateFormat);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateSettings_CreatesSettings_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.UserSettings.UpdateSettings, UserSettingsTestData.ValidUpdateRequest, _cancelToken);
        UserSettings? updated = await response.Content.ReadFromJsonAsync<UserSettings>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.True(updated!.SettingId > 0);
        Assert.Equal(UserSettingsTestData.UpdatedTimezone, updated.Timezone);
        Assert.Equal(UserSettingsTestData.UpdatedDateFormat, updated.DateFormat);
    }

    #endregion

    private async Task<UserSettings> CreateOrUpdateSettingsAsync(HttpClient client, string? timezone, string? dateFormat)
    {
        UserSettingsUpdateRequest request = new()
        {
            Timezone = timezone,
            DateFormat = dateFormat
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.UserSettings.UpdateSettings, request, _cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserSettings? created = await response.Content.ReadFromJsonAsync<UserSettings>(_cancelToken);

        Assert.NotNull(created);
        return created!;
    }
}
