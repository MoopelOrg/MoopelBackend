using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;

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

    [Fact]
    public async Task MyFridges_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(Routes.Fridge.MyFridges, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FridgeById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("fridge/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateFridge_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        CreateFridgeRequest request = new()
        {
            Name = "Kitchen Fridge"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MyFridges_ReturnsList_WhenAuthenticated(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync(Routes.Fridge.MyFridges, _cancelToken);
        List<FridgeRead>? fridges = await response.Content.ReadFromJsonAsync<List<FridgeRead>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(fridges);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateFridge_ReturnsFridge_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateFridgeRequest request = new()
        {
            Name = "Kitchen Fridge"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, request, _cancelToken);

        FridgeRead? fridge = await response.Content.ReadFromJsonAsync<FridgeRead>(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(fridge);
        Assert.True(fridge.FridgeId > 0);
        Assert.Equal("Kitchen Fridge", fridge.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task FridgeById_ReturnsFridge_WhenItExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateFridgeRequest createRequest = new()
        {
            Name = "Garage Fridge"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, createRequest, _cancelToken);

        FridgeRead? created = await createResponse.Content.ReadFromJsonAsync<FridgeRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await client.GetAsync($"fridge/{created.FridgeId}", _cancelToken);

        FridgeRead? result = await response.Content.ReadFromJsonAsync<FridgeRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.FridgeId, result.FridgeId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task FridgeById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response = await client.GetAsync("fridge/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateFridge_UpdatesFridge_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateFridgeRequest createRequest = new()
        {
            Name = "Old Fridge Name"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, createRequest, _cancelToken);

        FridgeRead? created = await createResponse.Content.ReadFromJsonAsync<FridgeRead>(_cancelToken);

        Assert.NotNull(created);

        UpdateFridgeRequest updateRequest = new()
        {
            Name = "New Fridge Name"
        };

        HttpResponseMessage updateResponse =
            await client.PutAsJsonAsync($"fridge/update/{created.FridgeId}", updateRequest, _cancelToken);

        FridgeRead? updated = await updateResponse.Content.ReadFromJsonAsync<FridgeRead>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.FridgeId, updated.FridgeId);
        Assert.Equal("New Fridge Name", updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateFridge_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        UpdateFridgeRequest updateRequest = new()
        {
            Name = "Does Not Exist"
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("fridge/update/999999", updateRequest, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteFridge_RemovesFridge_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        CreateFridgeRequest createRequest = new()
        {
            Name = "Delete Me"
        };

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(Routes.Fridge.CreateFridge, createRequest, _cancelToken);

        FridgeRead? created = await createResponse.Content.ReadFromJsonAsync<FridgeRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage deleteResponse =
            await client.DeleteAsync($"fridge/delete/{created.FridgeId}", _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync($"fridge/{created.FridgeId}", _cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteFridge_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.DeleteAsync("fridge/delete/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersFridge()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "fridgeuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "fridgeuser2", "Password123!", _cancelToken);

        CreateFridgeRequest request = new()
        {
            Name = "Private Fridge"
        };

        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Fridge.CreateFridge, request, _cancelToken);

        FridgeRead? created = await createResponse.Content.ReadFromJsonAsync<FridgeRead>(_cancelToken);

        Assert.NotNull(created);

        HttpResponseMessage response =
            await user2.GetAsync($"fridge/{created.FridgeId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }
}