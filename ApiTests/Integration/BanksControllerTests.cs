using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Attributes;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.Integration;

public class BanksControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public BanksControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    #region Get My Accounts

    [Fact]
    [TestDocumentation("Get 'my' bank accounts should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task MyAccounts_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Bank.MyAccounts, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [TestDocumentation("Get 'my' bank accounts should return an empty list when user is authenticated", HttpStatusCode.OK)]
    public async Task MyAccounts_ReturnsList_WhenAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Bank.MyAccounts, _cancelToken);
        List<BankAccount>? accounts = await response.Content.ReadFromJsonAsync<List<BankAccount>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(accounts);
        Assert.Empty(accounts);
    }

    [Fact]
    [TestDocumentation("Get 'my' bank accounts should return created accounts", HttpStatusCode.OK)]
    public async Task MyAccounts_ReturnsListWithCreatedItem_AfterCreating_WhenAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        BankAccountCreateRequest request = BankAccountTestData.ValidCreateRequest;

        // Act
        await client.PostAsJsonAsync(Routes.Bank.CreateAccount, request, _cancelToken);
        HttpResponseMessage response = await client.GetAsync(Routes.Bank.MyAccounts, _cancelToken);
        List<BankAccount>? accounts = await response.Content.ReadFromJsonAsync<List<BankAccount>>(_cancelToken);

        BankAccount? account = accounts?.FirstOrDefault();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(accounts);
        Assert.NotEmpty(accounts);

        Assert.NotNull(account);
        Assert.Equal(request.Name, account.Name);
        Assert.Equal(request.Description, account.Description);
    }

    #endregion

    #region Create Account

    [Theory]
    [TestDocumentation("Create bank account should return created account when request is valid", HttpStatusCode.Created)]
    [MemberData(nameof(BankAccountTestData.ValidBankAccountCreateRequests), MemberType = typeof(BankAccountTestData))]
    public async Task CreateAccount_ReturnsAccount_WhenValid(BankAccountCreateRequest createRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        HttpStatusCode expectedResult = HttpStatusCode.Created;

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Bank.CreateAccount, createRequest, _cancelToken);

        BankAccount? result = await response.Content.ReadFromJsonAsync<BankAccount>(_cancelToken);

        // Assert
        Assert.Equal(response.StatusCode, expectedResult);

        Assert.NotNull(result);
        Assert.NotNull(createRequest);
        Assert.True(result.AccountId > 0);
        Assert.Equal(createRequest.Name, result.Name);
        Assert.Equal(createRequest.Description, result.Description);
    }

    #endregion

    #region Account By Id

    [Fact]
    [TestDocumentation("Get account by id should return the account when it exists and belongs to the user", HttpStatusCode.OK)]
    public async Task AccountById_ReturnsAccount_WhenExists()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        BankAccountCreateRequest createRequest = BankAccountTestData.ValidCreateRequest;

        // Act
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Bank.CreateAccount, createRequest, _cancelToken);

        BankAccount? created = await createResponse.Content.ReadFromJsonAsync<BankAccount>(_cancelToken);
        string endpoint = Routes.Bank.Generate.AccountById(created?.AccountId ?? 0);

        HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);

        BankAccount? result = await response.Content.ReadFromJsonAsync<BankAccount>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotNull(created);
        Assert.Equal(created.AccountId, result.AccountId);
    }

    [Fact]
    [TestDocumentation("Get account by id should return not found when the account does not exist", HttpStatusCode.NotFound)]
    public async Task AccountById_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        string endpoint = Routes.Bank.Generate.AccountById(99999999);
        HttpResponseMessage response = await client.GetAsync(endpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Update Account
    [Theory]
    [TestDocumentation("User can create account then update, with updating changing data", HttpStatusCode.OK)]
    [MemberData(nameof(BankAccountTestData.ValidBankAccountUpdateCreateRequests), MemberType = typeof(BankAccountTestData))]
    public async Task UpdateAccount_UpdatesAccount_WhenValid(BankAccountCreateRequest createRequest, BankAccountUpdateRequest updateRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Bank.CreateAccount, createRequest, _cancelToken);

        BankAccount? created = await createResponse.Content.ReadFromJsonAsync<BankAccount>(_cancelToken);

        string updateEndpoint = Routes.Bank.Generate.UpdateAccount(created?.AccountId ?? 0);
        HttpResponseMessage updateResponse = await client.PutAsJsonAsync(updateEndpoint, updateRequest, _cancelToken);

        BankAccount? updated = await updateResponse.Content.ReadFromJsonAsync<BankAccount>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.NotNull(updateRequest);
        Assert.Equal(updateRequest.Name, updated.Name);
        Assert.Equal(updateRequest.Description, updated.Description);
    }

    #endregion


    #region Delete Account

    [Theory]
    [TestDocumentation("User can create account then delete, with the delete removing the account", HttpStatusCode.NoContent)]
    [MemberData(nameof(BankAccountTestData.ValidBankAccountCreateRequests), MemberType = typeof(BankAccountTestData))]
    public async Task DeleteAccount_RemovesAccount_WhenValid(BankAccountCreateRequest createRequest)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, _cancelToken);

        // Act
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Bank.CreateAccount, createRequest, _cancelToken);
        BankAccount? created = await createResponse.Content.ReadFromJsonAsync<BankAccount>(_cancelToken);

        string deleteEndpoint = Routes.Bank.Generate.DeleteAccount(created?.AccountId ?? 0);
        HttpResponseMessage deleteResponse = await client.DeleteAsync(deleteEndpoint, _cancelToken);

        string getEndpoint = Routes.Bank.Generate.AccountById(created?.AccountId ?? 0);
        HttpResponseMessage getResponse = await client.GetAsync(getEndpoint, _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    #endregion

    #region Generic Access Control
    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersAccount()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient userClient1 = _apiFactory.CreateClient();
        HttpClient userClient2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(userClient1, _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(userClient2, _cancelToken);


        // Act
        BankAccountCreateRequest request = BankAccountTestData.ValidCreateRequest;

        HttpResponseMessage createResponse = await userClient1.PostAsJsonAsync(Routes.Bank.CreateAccount, request, _cancelToken);

        BankAccount? created = await createResponse.Content.ReadFromJsonAsync<BankAccount>(_cancelToken);

        string getEndpoint = Routes.Bank.Generate.AccountById(created?.AccountId ?? 0);
        HttpResponseMessage response = await userClient2.GetAsync(getEndpoint, _cancelToken);

        // Assert
        Assert.NotNull(created);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion
}