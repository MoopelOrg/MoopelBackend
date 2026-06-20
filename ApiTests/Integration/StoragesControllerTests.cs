using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.Integration;

public class StoragesControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public StoragesControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region Get My Storages

    [Fact]
    public async Task MyStorages_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Storage.MyStorages, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MyStorages_ReturnsList_WhenAuthenticated(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Storage.MyStorages, _cancelToken);
        List<Storage>? storages = await response.Content.ReadFromJsonAsync<List<Storage>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(storages);
    }

    #endregion

    #region Get Storage By Id

    [Fact]
    public async Task StorageById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Storage.Generate.StorageById(1), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task StorageById_ReturnsStorage_WhenItExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        StorageCreateRequest createRequest = StorageTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Storage.CreateStorage, createRequest, _cancelToken);

        Storage? created = await createResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Storage.Generate.StorageById(created.StorageId), _cancelToken);

        Storage? result = await response.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.StorageId, result.StorageId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task StorageById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Storage.Generate.StorageById(9999), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersStorage()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        StorageCreateRequest request = StorageTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Storage.CreateStorage, request, _cancelToken);

        Storage? created = await createResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Storage.Generate.StorageById(created.StorageId), _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion

    #region Create Storage

    [Fact]
    public async Task CreateStorage_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        StorageCreateRequest request = StorageTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Storage.CreateStorage, request, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateStorage_ReturnsStorage_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        StorageCreateRequest request = StorageTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Storage.CreateStorage, request, _cancelToken);

        Storage? created = await response.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.StorageId > 0);
        Assert.Equal(StorageTestData.ValidCreateRequest.Name, created.Name);
    }

    #endregion

    #region Update Storage

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateStorage_UpdatesStorage_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        StorageCreateRequest createRequest = StorageTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Storage.CreateStorage, createRequest, _cancelToken);

        Storage? created = await createResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        Assert.NotNull(created);

        StorageUpdateRequest updateRequest = StorageTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync(Routes.Storage.Generate.UpdateStorage(created.StorageId), updateRequest, _cancelToken);

        Storage? updated = await updateResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.StorageId, updated.StorageId);
        Assert.Equal(StorageTestData.ValidUpdateRequest.Name, updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateStorage_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        StorageUpdateRequest updateRequest = StorageTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.Storage.Generate.UpdateStorage(9999), updateRequest, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Delete Storage

    [Fact]
    public async Task DeleteStorage_RemovesStorage_WhenValid()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        StorageCreateRequest createRequest = StorageTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Storage.CreateStorage, createRequest, _cancelToken);

        Storage? created = await createResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Storage.Generate.DeleteStorage(created.StorageId), _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Storage.Generate.StorageById(created.StorageId), _cancelToken);

        // Assert
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteStorage_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Storage.Generate.DeleteStorage(9999), _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}