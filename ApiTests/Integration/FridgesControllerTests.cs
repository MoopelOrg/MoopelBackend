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

public class FridgesControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public FridgesControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region Fridges

    #region Get My Fridges

    [Fact]
    public async Task MyFridges_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Fridge.MyFridges, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MyFridges_ReturnsList_WhenAuthenticated(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Fridge.MyFridges, _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        List<Fridge>? fridges = await response.Content.ReadFromJsonAsync<List<Fridge>>(_cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Assert.True(fridges is not null, $"Response body could not be deserialized into a list. Body: {responseBody}");
    }

    #endregion

    #region Get Fridge By Id
    [Fact]
    public async Task FridgeById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Fridge.Generate.FridgeById(FridgeTestData.ArbitraryFridgeId), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task FridgeById_ReturnsFridge_WhenItExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        FridgeCreateRequest createRequest = FridgeTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, createRequest, _cancelToken);

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Fridge? created = await createResponse.Content.ReadFromJsonAsync<Fridge>(_cancelToken);

        string endpoint = Routes.Fridge.Generate.FridgeById(created?.FridgeId ?? 0);
        HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Fridge? result = await response.Content.ReadFromJsonAsync<Fridge>(_cancelToken);

        // Assert
        Assert.True(created is not null, $"Failed to create fridge. Status: {createResponse.StatusCode}, Body: {createBody}");
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, response));
        Assert.True(result is not null, $"Response body could not be deserialized into a Fridge. Body: {responseBody}");
        Assert.Equal(created.FridgeId, result.FridgeId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task FridgeById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Fridge.Generate.FridgeById(FridgeTestData.NonExistentFridgeId), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    #endregion

    #region Create Fridge

    [Fact]
    public async Task CreateFridge_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        FridgeCreateRequest request = FridgeTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, request, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    [Theory]
    [MemberData(nameof(FridgeTestData.ValidFridgeCreateRequests), MemberType = typeof(FridgeTestData))]
    public async Task CreateFridge_ReturnsFridge_WhenValid(FridgeCreateRequest request)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, request, _cancelToken);

        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Fridge? fridge = await response.Content.ReadFromJsonAsync<Fridge>(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created,
            $"Expected {HttpStatusCode.OK} or {HttpStatusCode.Created} but got {response.StatusCode}. Body: {responseBody}");

        Assert.True(fridge is not null, $"Response body could not be deserialized into a Fridge. Body: {responseBody}");
        Assert.NotNull(request);
        Assert.True(fridge.FridgeId > 0, $"Expected a positive FridgeId but got {fridge.FridgeId}");
        Assert.Equal(request.Name, fridge.Name);
    }

    #endregion

    #region Update Fridge

    [Theory]
    [MemberData(nameof(FridgeTestData.ValidFridgeUpdateCreateRequests), MemberType = typeof(FridgeTestData))]
    public async Task UpdateFridge_UpdatesFridge_WhenValid(FridgeCreateRequest createRequest, FridgeUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, _cancelToken);

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, createRequest, _cancelToken);

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Fridge? created = await createResponse.Content.ReadFromJsonAsync<Fridge>(_cancelToken);

        Assert.True(created is not null, $"Failed to create fridge. Status: {createResponse.StatusCode}, Body: {createBody}");

        // Act
        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync(Routes.Fridge.Generate.UpdateFridge(created.FridgeId), updateRequest, _cancelToken);
        string updateBody = await updateResponse.Content.ReadAsStringAsync(_cancelToken);
        Fridge? updated = await updateResponse.Content.ReadFromJsonAsync<Fridge>(_cancelToken);

        // Assert
        Assert.True(
            condition: updateResponse.StatusCode == HttpStatusCode.OK,
            userMessage: await TMsg(HttpStatusCode.OK, updateResponse));
        Assert.True(updated is not null, $"Response body could not be deserialized into a Fridge. Body: {updateBody}");
        Assert.NotNull(updateRequest);
        Assert.NotNull(createRequest);
        Assert.Equal(created.FridgeId, updated.FridgeId);
        Assert.Equal(updateRequest.Name, updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateFridge_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        FridgeUpdateRequest updateRequest = FridgeTestData.ValidUpdateRequest;

        // Act
        HttpResponseMessage response =
            await client.PutAsJsonAsync(Routes.Fridge.Generate.UpdateFridge(FridgeTestData.NonExistentFridgeId), updateRequest, _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    #endregion

    #region Delete Fridge

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteFridge_RemovesFridge_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        FridgeCreateRequest createRequest = FridgeTestData.ValidCreateRequest;

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, createRequest, _cancelToken);

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Fridge? created = await createResponse.Content.ReadFromJsonAsync<Fridge>(_cancelToken);

        Assert.True(created is not null, $"Failed to create fridge. Status: {createResponse.StatusCode}, Body: {createBody}");

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Fridge.Generate.DeleteFridge(created.FridgeId), _cancelToken);
        string deleteBody = await deleteResponse.Content.ReadAsStringAsync(_cancelToken);
        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Fridge.Generate.FridgeById(created.FridgeId), _cancelToken);

        // Assert
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK || deleteResponse.StatusCode == HttpStatusCode.NoContent,
            $"Expected {HttpStatusCode.OK} or {HttpStatusCode.NoContent} but got {deleteResponse.StatusCode}. Body: {deleteBody}");

        Assert.True(
            condition: getResponse.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, getResponse));
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteFridge_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Fridge.Generate.DeleteFridge(FridgeTestData.NonExistentFridgeId), _cancelToken);

        // Assert
        Assert.True(
            condition: response.StatusCode == HttpStatusCode.NotFound,
            userMessage: await TMsg(HttpStatusCode.NotFound, response));
    }

    #endregion

    #endregion

    #region Fridge Items

    #region Create Fridge Item

    [Fact]
    public async Task CreateFridgeItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            Routes.Fridge.Generate.CreateFridgeItem(FridgeTestData.NonExistentFridgeId),
            FridgeTestData.ValidItemCreateRequest,
            _cancelToken);

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    #endregion

    #region Update Fridge Item

    [Fact]
    public async Task UpdateFridgeItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.PatchAsJsonAsync(
            Routes.Fridge.Generate.UpdateFridgeItem(FridgeTestData.NonExistentFridgeItemId),
            FridgeTestData.ValidItemUpdateRequest,
            _cancelToken);

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    #endregion

    #region Delete Fridge Item

    [Fact]
    public async Task DeleteFridgeItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.DeleteAsync(
            Routes.Fridge.Generate.DeleteFridgeItem(FridgeTestData.NonExistentFridgeItemId),
            _cancelToken);

        Assert.True(
            condition: response.StatusCode == HttpStatusCode.Unauthorized,
            userMessage: await TMsg(HttpStatusCode.Unauthorized, response));
    }

    #endregion

    #endregion

    #region Flow

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersFridge()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        FridgeCreateRequest request = FridgeTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Fridge.CreateFridge, request, _cancelToken);

        string createBody = await createResponse.Content.ReadAsStringAsync(_cancelToken);
        Fridge? created = await createResponse.Content.ReadFromJsonAsync<Fridge>(_cancelToken);

        HttpResponseMessage response =
            await user2.GetAsync(Routes.Fridge.Generate.FridgeById(created?.FridgeId ?? 0), _cancelToken);
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        // Assert
        Assert.True(created is not null, $"Failed to create fridge for user1. Status: {createResponse.StatusCode}, Body: {createBody}");
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound} but got {response.StatusCode}. Body: {responseBody}");
    }

    #endregion
}