using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

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

    #region Get All Themes

    [Fact]
    public async Task AllThemes_ReturnsList_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Theme.AllThemes, _cancelToken);
        List<Theme>? themes = await response.Content.ReadFromJsonAsync<List<Theme>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(themes);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task AllThemes_ReturnsList_WhenAuthenticated(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Theme.AllThemes, _cancelToken);
        List<Theme>? themes = await response.Content.ReadFromJsonAsync<List<Theme>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(themes);
    }

    #endregion

    #region Get Theme By Id

    [Fact]
    public async Task ThemeById_ReturnsTheme_WhenNotAuthenticated_IfPublic()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage allResponse = await client.GetAsync(Routes.Theme.AllThemes, _cancelToken);
        List<Theme>? themes = await allResponse.Content.ReadFromJsonAsync<List<Theme>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        Assert.NotNull(themes);

        if (themes.Count == 0)
        {
            return;
        }

        Theme firstTheme = themes.First();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Theme.Generate.ThemeById(firstTheme.ThemeId), _cancelToken);
        Theme? theme = await response.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(theme);
        Assert.Equal(firstTheme.ThemeId, theme.ThemeId);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task ThemeById_ReturnsTheme_WhenItExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        ThemeCreateRequest createRequest = ThemeTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, createRequest, _cancelToken);

        Theme? created = await createResponse.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Theme.Generate.ThemeById(created.ThemeId), _cancelToken);

        Theme? result = await response.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.ThemeId, result.ThemeId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task ThemeById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Theme.Generate.ThemeById(9999), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CanStillRead_AnotherUsersTheme_IfThemesAreShared()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        ThemeCreateRequest request = ThemeTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Theme.CreateTheme, request, _cancelToken);

        Theme? created = await createResponse.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Theme.Generate.ThemeById(created.ThemeId), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Get My Theme

    [Fact]
    public async Task MyTheme_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Theme.MyTheme, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Create Theme

    [Fact]
    public async Task CreateTheme_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        ThemeCreateRequest request = ThemeTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, request, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateTheme_ReturnsTheme_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        ThemeCreateRequest request = ThemeTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, request, _cancelToken);

        Theme? created = await response.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.ThemeId > 0);
        Assert.Equal(ThemeTestData.ValidCreateRequest.Name, created.Name);
    }

    #endregion

    #region Update Theme

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTheme_UpdatesTheme_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        ThemeCreateRequest createRequest = ThemeTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, createRequest, _cancelToken);

        Theme? created = await createResponse.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        Assert.NotNull(created);

        ThemeUpdateRequest updateRequest = ThemeTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync(Routes.Theme.Generate.UpdateTheme(created.ThemeId), updateRequest, _cancelToken);

        Theme? updated = await updateResponse.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.ThemeId, updated.ThemeId);
        Assert.Equal(ThemeTestData.ValidUpdateRequest.Name, updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTheme_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        ThemeUpdateRequest updateRequest = ThemeTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.Theme.Generate.UpdateTheme(9999), updateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Delete Theme

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTheme_RemovesTheme_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        ThemeCreateRequest createRequest = ThemeTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, createRequest, _cancelToken);

        Theme? created = await createResponse.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Theme.Generate.DeleteTheme(created.ThemeId), _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Theme.Generate.ThemeById(created.ThemeId), _cancelToken);

        // Assert
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTheme_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Theme.Generate.DeleteTheme(9999), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Set My Theme

    [Fact]
    public async Task SetMyTheme_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsync(Routes.Theme.Generate.SetMyTheme(1), null, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SetMyTheme_SetsAndReturnsTheme_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        ThemeCreateRequest createRequest = ThemeTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, createRequest, _cancelToken);

        Theme? created = await createResponse.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage setResponse =
            await client.PostAsync(Routes.Theme.Generate.SetMyTheme(created.ThemeId), null, _cancelToken);

        Assert.True(
            setResponse.StatusCode == HttpStatusCode.OK ||
            setResponse.StatusCode == HttpStatusCode.NoContent);

        HttpResponseMessage myThemeResponse =
            await client.GetAsync(Routes.Theme.MyTheme, _cancelToken);

        Theme? myTheme = await myThemeResponse.Content.ReadFromJsonAsync<Theme>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, myThemeResponse.StatusCode);
        Assert.NotNull(myTheme);
        Assert.Equal(created.ThemeId, myTheme.ThemeId);
        Assert.Equal(created.Name, myTheme.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task SetMyTheme_ReturnsNotFound_WhenThemeMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsync(Routes.Theme.Generate.SetMyTheme(9999), null, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}