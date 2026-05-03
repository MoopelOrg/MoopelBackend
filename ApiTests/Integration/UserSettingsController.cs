using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;

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

    [Fact]
    public async Task MySettings_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(Routes.UserSettings.MySettings, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SettingsById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("usersettings/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSettings_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        CreateUserSettingsRequest request = new()
        {
            Name = "Default Settings"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.UserSettings.CreateSettings, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSettings_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        UpdateUserSettingsRequest request = new()
        {
            Name = "Updated Settings"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("usersettings/update/1", request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSettings_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.DeleteAsync("usersettings/delete/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MySettings_ReturnsSettingsOrEmpty_WhenAuthenticated(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync(Routes.UserSettings.MySettings, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateSettings_ReturnsSettings_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateUserSettingsRequest request = new()
        {
            Name = "My Settings"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.UserSettings.CreateSettings, request, _cancelToken);

        UserSettingsRead? created =
            await response.Content.ReadFromJsonAsync<UserSettingsRead>(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.SettingId > 0);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MySettings_ReturnsCreatedSettings_WhenTheyExist(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UserSettingsRead created = await CreateSettingsAsync(client, "Primary Settings");

        HttpResponseMessage response = await client.GetAsync(Routes.UserSettings.MySettings, _cancelToken);
        UserSettingsRead? result = await response.Content.ReadFromJsonAsync<UserSettingsRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.SettingId, result.SettingId);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SettingsById_ReturnsSettings_WhenOwned(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UserSettingsRead created = await CreateSettingsAsync(client, "Owned Settings");

        HttpResponseMessage response =
            await client.GetAsync($"usersettings/{created.SettingId}", _cancelToken);

        UserSettingsRead? result =
            await response.Content.ReadFromJsonAsync<UserSettingsRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.SettingId, result.SettingId);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SettingsById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.GetAsync("usersettings/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersSettings()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "settingsuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "settingsuser2", "Password123!", _cancelToken);

        UserSettingsRead created = await CreateSettingsAsync(user1, "Private Settings");

        HttpResponseMessage response =
            await user2.GetAsync($"usersettings/{created.SettingId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateSettings_UpdatesSettings_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UserSettingsRead created = await CreateSettingsAsync(client, "Old Settings");

        UpdateUserSettingsRequest request = new()
        {
            Name = "New Settings"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync($"usersettings/update/{created.SettingId}", request, _cancelToken);

        UserSettingsRead? updated =
            await response.Content.ReadFromJsonAsync<UserSettingsRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.SettingId, updated.SettingId);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateSettings_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UpdateUserSettingsRequest request = new()
        {
            Name = "Missing Settings"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("usersettings/update/999999", request, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotUpdate_AnotherUsersSettings()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "updatesettings1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "updatesettings2", "Password123!", _cancelToken);

        UserSettingsRead created = await CreateSettingsAsync(user1, "Private Update Settings");

        UpdateUserSettingsRequest request = new()
        {
            Name = "Hacked Settings"
        };

        HttpResponseMessage response =
            await user2.PutAsJsonAsync($"usersettings/update/{created.SettingId}", request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteSettings_RemovesSettings_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UserSettingsRead created = await CreateSettingsAsync(client, "Delete Settings");

        HttpResponseMessage deleteResponse =
            await client.DeleteAsync($"usersettings/delete/{created.SettingId}", _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync($"usersettings/{created.SettingId}", _cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteSettings_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.DeleteAsync("usersettings/delete/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotDelete_AnotherUsersSettings()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "deletesettings1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "deletesettings2", "Password123!", _cancelToken);

        UserSettingsRead created = await CreateSettingsAsync(user1, "Private Delete Settings");

        HttpResponseMessage response =
            await user2.DeleteAsync($"usersettings/delete/{created.SettingId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    private async Task<UserSettingsRead> CreateSettingsAsync(HttpClient client, string name)
    {
        CreateUserSettingsRequest request = new()
        {
            Name = name
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.UserSettings.CreateSettings, request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        UserSettingsRead? created =
            await response.Content.ReadFromJsonAsync<UserSettingsRead>(_cancelToken);

        Assert.NotNull(created);
        return created;
    }
}