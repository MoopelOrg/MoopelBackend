using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;

namespace ApiTests.Integration;

public class ThemesControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public ThemesControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public async Task AllThemes_ReturnsList_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(Routes.Theme.AllThemes, _cancelToken);
        List<ThemeRead>? themes = await response.Content.ReadFromJsonAsync<List<ThemeRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(themes);
    }

    [Fact]
    public async Task ThemeById_ReturnsTheme_WhenNotAuthenticated_IfPublic()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage allResponse = await client.GetAsync(Routes.Theme.AllThemes, _cancelToken);
        List<ThemeRead>? themes = await allResponse.Content.ReadFromJsonAsync<List<ThemeRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        Assert.NotNull(themes);

        if (themes.Count == 0)
        {
            return;
        }

        ThemeRead firstTheme = themes.First();

        HttpResponseMessage response = await client.GetAsync($"theme/{firstTheme.ThemeId}", _cancelToken);
        ThemeRead? theme = await response.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(theme);
        Assert.Equal(firstTheme.ThemeId, theme.ThemeId);
    }

    [Fact]
    public async Task MyTheme_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(Routes.Theme.MyTheme, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTheme_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        CreateThemeRequest request = new()
        {
            Name = "Dark Blue"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SetMyTheme_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.PostAsync("theme/set/1", null, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task AllThemes_ReturnsList_WhenAuthenticated(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync(Routes.Theme.AllThemes, _cancelToken);
        List<ThemeRead>? themes = await response.Content.ReadFromJsonAsync<List<ThemeRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(themes);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateTheme_ReturnsTheme_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateThemeRequest request = new()
        {
            Name = "Ocean Blue"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, request, _cancelToken);

        ThemeRead? created = await response.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.ThemeId > 0);
        Assert.Equal("Ocean Blue", created.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task ThemeById_ReturnsTheme_WhenItExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateThemeRequest createRequest = new()
        {
            Name = "Forest"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, createRequest, _cancelToken);

        ThemeRead? created = await createResponse.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await client.GetAsync($"theme/{created.ThemeId}", _cancelToken);

        ThemeRead? result = await response.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.ThemeId, result.ThemeId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task ThemeById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync("theme/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTheme_UpdatesTheme_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateThemeRequest createRequest = new()
        {
            Name = "Old Theme"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, createRequest, _cancelToken);

        ThemeRead? created = await createResponse.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.NotNull(created);

        UpdateThemeRequest updateRequest = new()
        {
            Name = "New Theme"
        };

        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync($"theme/update/{created.ThemeId}", updateRequest, _cancelToken);

        ThemeRead? updated = await updateResponse.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.ThemeId, updated.ThemeId);
        Assert.Equal("New Theme", updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTheme_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UpdateThemeRequest updateRequest = new()
        {
            Name = "Missing Theme"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("theme/update/999999", updateRequest, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTheme_RemovesTheme_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateThemeRequest createRequest = new()
        {
            Name = "Delete Me"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, createRequest, _cancelToken);

        ThemeRead? created = await createResponse.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage deleteResponse =
            await client.DeleteAsync($"theme/delete/{created.ThemeId}", _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync($"theme/{created.ThemeId}", _cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTheme_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.DeleteAsync("theme/delete/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SetMyTheme_SetsAndReturnsTheme_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateThemeRequest createRequest = new()
        {
            Name = "My Selected Theme"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, createRequest, _cancelToken);

        ThemeRead? created = await createResponse.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage setResponse =
            await client.PostAsync($"theme/set/{created.ThemeId}", null, _cancelToken);

        Assert.True(
            setResponse.StatusCode == HttpStatusCode.OK ||
            setResponse.StatusCode == HttpStatusCode.NoContent);

        HttpResponseMessage myThemeResponse =
            await client.GetAsync(Routes.Theme.MyTheme, _cancelToken);

        ThemeRead? myTheme = await myThemeResponse.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, myThemeResponse.StatusCode);
        Assert.NotNull(myTheme);
        Assert.Equal(created.ThemeId, myTheme.ThemeId);
        Assert.Equal(created.Name, myTheme.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SetMyTheme_ReturnsNotFound_WhenThemeMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.PostAsync("theme/set/999999", null, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CanStillRead_AnotherUsersTheme_IfThemesAreShared()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "themeuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "themeuser2", "Password123!", _cancelToken);

        CreateThemeRequest request = new()
        {
            Name = "Shared Theme"
        };

        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Theme.CreateTheme, request, _cancelToken);

        ThemeRead? created = await createResponse.Content.ReadFromJsonAsync<ThemeRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await user2.GetAsync($"theme/{created.ThemeId}", _cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}