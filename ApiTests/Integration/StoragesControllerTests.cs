using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using static ApiTests.TestHelper;

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

    #region Storages

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
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        List<Storage>? storages = await response.Content.ReadFromJsonAsync<List<Storage>>(_cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Assert.True(storages is not null, $"Response body could not be deserialized into a list. Body: {responseBody}");
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
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Storage? created = await createResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        Assert.True(created is not null, $"Failed to create storage. Status: {createResponse.StatusCode}, Body: {createBody}");

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Storage.Generate.StorageById(created.StorageId), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Storage? result = await response.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Assert.True(result is not null, $"Response body could not be deserialized into a Storage. Body: {responseBody}");
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
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
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

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Storage? created = await createResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        Assert.True(created is not null, $"Failed to create storage for user1. Status: {createResponse.StatusCode}, Body: {createBody}");

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Storage.Generate.StorageById(created.StorageId), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound} but got {response.StatusCode}. Body: {responseBody}");
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
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
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
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created,
            $"Expected {HttpStatusCode.OK} or {HttpStatusCode.Created} but got {response.StatusCode}. Body: {responseBody}");

        Assert.True(created is not null, $"Response body could not be deserialized into a Storage. Body: {responseBody}");
        Assert.True(created.StorageId > 0, $"Expected a positive StorageId but got {created.StorageId}");
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

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Storage? created = await createResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        Assert.True(created is not null, $"Failed to create storage. Status: {createResponse.StatusCode}, Body: {createBody}");

        StorageUpdateRequest updateRequest = StorageTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync(Routes.Storage.Generate.UpdateStorage(created.StorageId), updateRequest, _cancelToken);
        string updateBody = await updateResponse.Content.ReadAsStringAsync(_cancelToken);
        Storage? updated = await updateResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        // Assert
        Assert.True(
            condition: updateResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, updateResponse));
        Assert.True(updated is not null, $"Response body could not be deserialized into a Storage. Body: {updateBody}");
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
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
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

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Storage? created = await createResponse.Content.ReadFromJsonAsync<Storage>(_cancelToken);

        Assert.True(created is not null, $"Failed to create storage. Status: {createResponse.StatusCode}, Body: {createBody}");

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Storage.Generate.DeleteStorage(created.StorageId), _cancelToken);
        string deleteBody = await deleteResponse.Content.ReadAsStringAsync(_cancelToken);
        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Storage.Generate.StorageById(created.StorageId), _cancelToken);

        // Assert
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK || deleteResponse.StatusCode == HttpStatusCode.NoContent,
            $"Expected {HttpStatusCode.OK} or {HttpStatusCode.NoContent} but got {deleteResponse.StatusCode}. Body: {deleteBody}");

        Assert.True(
            condition: getResponse.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, getResponse));
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
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    #endregion

    #endregion

    #region Storage Items

    #region Create Storage Item

    [Fact]
    public async Task CreateStorageItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            Routes.Storage.Generate.CreateStorageItem(StorageTestData.NonExistentStorageId),
            StorageTestData.ValidCreateRequest,
            _cancelToken);

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    #endregion


    #region Update Storage Item

    [Fact]
    public async Task UpdateStorageItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.PatchAsJsonAsync(
            Routes.Storage.Generate.UpdateStorageItem(StorageTestData.NonExistentStorageItemId),
            StorageTestData.ValidUpdateRequest,
            _cancelToken);

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    #endregion

    #region Delete Storage Item

    [Fact]
    public async Task DeleteStorageItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.DeleteAsync(
            Routes.Storage.Generate.DeleteStorageItem(StorageTestData.NonExistentStorageItemId),
            _cancelToken);

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    #endregion

    #endregion

}