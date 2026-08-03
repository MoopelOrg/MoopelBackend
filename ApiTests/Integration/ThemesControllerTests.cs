using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Attributes;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using static ApiTests.TestHelper;

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
    [TestDocumentation("Get all themes should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task AllThemes_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Theme.AllThemes, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Fact]
    [TestDocumentation("Get all themes should return a list when user is authenticated", HttpStatusCode.OK)]
    public async Task AllThemes_ReturnsList_WhenAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Theme.AllThemes, _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        List<Theme>? themes = JsonSerializer.Deserialize<List<Theme>>(responseBody, _apiFactory.JsonSerializerOptions);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Assert.True(themes is not null, $"Response body could not be deserialized into a List<Theme>. Body: {responseBody}");
    }

    [Fact]
    [TestDocumentation("Get all themes should include a created theme", HttpStatusCode.OK)]
    public async Task AllThemes_ReturnsListWithCreatedItem_AfterCreating_WhenAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        ThemeCreateRequest request = ThemeTestData.ValidCreateRequest;

        // Act
        Theme created = await CreateThemeAsync(client, request);
        HttpResponseMessage response = await client.GetAsync(Routes.Theme.AllThemes, _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        List<Theme>? themes = JsonSerializer.Deserialize<List<Theme>>(responseBody, _apiFactory.JsonSerializerOptions);
        Theme? theme = themes?.FirstOrDefault(t => t.ThemeId == created.ThemeId);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Assert.True(themes is not null, $"Response body could not be deserialized into a List<Theme>. Body: {responseBody}");
        Assert.True(theme is not null, $"Response body could not be deserialized into a Theme. Body: {responseBody}");
        Assert.Equal(created.ThemeId, theme.ThemeId);
        Assert.Equal(request.Name, theme.Name);
        Assert.Equal(request.Description, theme.Description);
    }

    #endregion

    #region Get Theme By Id

    [Fact]
    [TestDocumentation("Get theme by id should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task ThemeById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        string endpoint = Routes.Theme.Generate.ThemeById(ThemeTestData.MissingThemeId);

        // Act
        HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Fact]
    [TestDocumentation("Get theme by id should return the theme when it exists", HttpStatusCode.OK)]
    public async Task ThemeById_ReturnsTheme_WhenItExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        ThemeCreateRequest createRequest = ThemeTestData.ValidCreateRequest;

        // Act
        Theme created = await CreateThemeAsync(client, createRequest);
        HttpResponseMessage response =
            await client.GetAsync(Routes.Theme.Generate.ThemeById(created.ThemeId), _cancelToken);

        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Theme? result = JsonSerializer.Deserialize<Theme>(responseBody, _apiFactory.JsonSerializerOptions);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Assert.True(result is not null, $"Response body could not be deserialized into a Theme. Body: {responseBody}");
        Assert.Equal(created.ThemeId, result.ThemeId);
        Assert.Equal(created.Name, result.Name);
        Assert.Equal(created.Description, result.Description);
    }

    [Fact]
    [TestDocumentation("Get theme by id should return not found when the theme does not exist", HttpStatusCode.NotFound)]
    public async Task ThemeById_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Theme.Generate.ThemeById(ThemeTestData.MissingThemeId), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    [Fact]
    [TestDocumentation("One user should still be able to read another user's theme when themes are shared", HttpStatusCode.OK)]
    public async Task OneUser_CanStillRead_AnotherUsersTheme_IfThemesAreShared()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        ThemeCreateRequest request = ThemeTestData.ValidCreateRequest;

        // Act
        Theme created = await CreateThemeAsync(user1, request);
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Theme.Generate.ThemeById(created.ThemeId), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
    }

    #endregion

    #region Create Theme

    [Fact]
    [TestDocumentation("Create theme should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task CreateTheme_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, ThemeTestData.ValidCreateRequest, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [TestDocumentation("Create theme should return created theme when request is valid", HttpStatusCode.Created)]
    [MemberData(nameof(ThemeTestData.ValidThemeCreateRequests), MemberType = typeof(ThemeTestData))]
    public async Task CreateTheme_ReturnsTheme_WhenValid(ThemeCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, request, _cancelToken);

        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Theme? created = JsonSerializer.Deserialize<Theme>(responseBody, _apiFactory.JsonSerializerOptions);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Created,
            userMessage: await TMsg(HttpStatusCode.Created, response));
        Assert.True(created is not null, $"Response body could not be deserialized into a Theme. Body: {responseBody}");
        Assert.NotNull(request);
        Assert.True(created.ThemeId > 0);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(request.Description, created.Description);
    }

    #endregion

    #region Update Theme

    [Fact]
    [TestDocumentation("Update theme should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task UpdateTheme_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(Routes.Theme.Generate.UpdateTheme(ThemeTestData.MissingThemeId), ThemeTestData.ValidUpdateRequest, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [TestDocumentation("Update theme should return updated theme when request is valid", HttpStatusCode.OK)]
    [MemberData(nameof(ThemeTestData.ValidThemeUpdateCreateRequests), MemberType = typeof(ThemeTestData))]
    public async Task UpdateTheme_UpdatesTheme_WhenValid(ThemeCreateRequest createRequest, ThemeUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        Theme created = await CreateThemeAsync(client, createRequest);

        HttpResponseMessage updateResponse =
            await client.PatchAsJsonAsync(Routes.Theme.Generate.UpdateTheme(created.ThemeId), updateRequest, _cancelToken);

        string updateResponseBody = await updateResponse.Content.ReadAsStringAsync(_cancelToken);
        Theme? updated = JsonSerializer.Deserialize<Theme>(updateResponseBody, _apiFactory.JsonSerializerOptions);

        // Assert
        Assert.True(
            condition: updateResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, updateResponse));
        Assert.True(updated is not null, $"Response body could not be deserialized into a Theme. Body: {updateResponseBody}");
        Assert.NotNull(createRequest);
        Assert.NotNull(updateRequest);
        Assert.Equal(created.ThemeId, updated.ThemeId);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Description, updated.Description);
        Assert.Equal(created.Data, updated.Data);
        Assert.Equal(created.IsDark, updated.IsDark);
    }

    [Theory]
    [TestDocumentation("Update theme should return not found when the theme does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(ThemeTestData.ValidThemeUpdateRequests), MemberType = typeof(ThemeTestData))]
    public async Task UpdateTheme_ReturnsNotFound_WhenMissing(ThemeUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(Routes.Theme.Generate.UpdateTheme(ThemeTestData.MissingThemeId), updateRequest, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    #endregion

    private async Task<Theme> CreateThemeAsync(HttpClient client, ThemeCreateRequest request)
    {
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Theme.CreateTheme, request, _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Created,
            userMessage: await TMsg(HttpStatusCode.Created, response));
        Theme? created = JsonSerializer.Deserialize<Theme>(responseBody, _apiFactory.JsonSerializerOptions);
        Assert.True(created is not null, $"Response body could not be deserialized into a Theme. Body: {responseBody}");

        return created!;
    }
}
