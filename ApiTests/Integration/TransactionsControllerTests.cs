using System.Net;
using System.Net.Http.Json;

using ApiTests.Integration.Generic;
using ApiTests.TestData;

using MoopelApi;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace ApiTests.Integration;

public class TransactionsControllerTests : IClassFixture<MoopelApiFactory>
{
    private readonly MoopelApiFactory _apiFactory;
    private readonly CancellationToken _cancelToken;

    public TransactionsControllerTests(MoopelApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _cancelToken = TestContext.Current.CancellationToken;
    }

    [Fact]
    public async Task TransactionsByAccountId_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("transaction/account/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TransactionById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("transaction/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        TransactionCreateRequest request = new()
        {
            AccountId = 1,
            Name = "Groceries",
            Amount = 42.50m
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Transaction.CreateTransaction, request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        TransactionUpdateRequest request = new()
        {
            Name = "Updated",
            Amount = 10m
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("transaction/update/1", request, _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        HttpResponseMessage response =
            await client.DeleteAsync("transaction/delete/1", _cancelToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TransactionsByAccountId_ReturnsList_WhenAuthenticatedAndOwnsAccount(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client, "Checking");

        HttpResponseMessage response =
            await client.GetAsync($"transaction/account/{account.AccountId}", _cancelToken);

        List<Transaction>? result =
            await response.Content.ReadFromJsonAsync<List<Transaction>>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TransactionsByAccountId_ReturnsNotFound_WhenAccountMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.GetAsync("transaction/account/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateTransaction_ReturnsTransaction_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client, "Spending");

        TransactionCreateRequest request = new()
        {
            AccountId = account.AccountId,
            Name = "Groceries",
            Amount = 42.50m
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Transaction.CreateTransaction, request, _cancelToken);

        Transaction? created =
            await response.Content.ReadFromJsonAsync<Transaction>(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.TransactionId > 0);
        Assert.Equal("Groceries", created.Name);
        Assert.Equal(account.AccountId, created.AccountId);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateTransaction_ReturnsNotFound_WhenAccountMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        TransactionCreateRequest request = new()
        {
            AccountId = 999999,
            Name = "Bad Transaction",
            Amount = 12.34m
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Transaction.CreateTransaction, request, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TransactionById_ReturnsTransaction_WhenItExists(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client, "Main");
        Transaction created = await CreateTransactionAsync(client, account.AccountId, "Rent", 1200m);

        HttpResponseMessage response =
            await client.GetAsync($"transaction/{created.TransactionId}", _cancelToken);

        Transaction? result =
            await response.Content.ReadFromJsonAsync<Transaction>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.TransactionId, result.TransactionId);
        Assert.Equal(created.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TransactionById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.GetAsync("transaction/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTransaction_UpdatesTransaction_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client, "Bills");
        Transaction created = await CreateTransactionAsync(client, account.AccountId, "Power", 85m);

        TransactionUpdateRequest request = new()
        {
            Name = "Electric Bill",
            Amount = 90m
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync($"transaction/update/{created.TransactionId}", request, _cancelToken);

        Transaction? updated =
            await response.Content.ReadFromJsonAsync<Transaction>(_cancelToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.TransactionId, updated.TransactionId);
        Assert.Equal("Electric Bill", updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTransaction_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        TransactionUpdateRequest request = new()
        {
            Name = "Missing",
            Amount = 1m
        };

        HttpResponseMessage response =
            await client.PutAsJsonAsync("transaction/update/999999", request, _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTransaction_RemovesTransaction_WhenValid(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client, "Delete Account");
        Transaction created = await CreateTransactionAsync(client, account.AccountId, "Delete Me", 5m);

        HttpResponseMessage deleteResponse =
            await client.DeleteAsync($"transaction/delete/{created.TransactionId}", _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync($"transaction/{created.TransactionId}", _cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTransaction_ReturnsNotFound_WhenMissing(string username, string password)
    {
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        HttpResponseMessage response =
            await client.DeleteAsync("transaction/delete/999999", _cancelToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersTransaction()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "transactionuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "transactionuser2", "Password123!", _cancelToken);

        BankAccount account = await CreateBankAccountAsync(user1, "Private Checking");
        Transaction created = await CreateTransactionAsync(user1, account.AccountId, "Private Purchase", 25m);

        HttpResponseMessage response =
            await user2.GetAsync($"transaction/{created.TransactionId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OneUser_CannotList_AnotherUsersAccountTransactions()
    {
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "accounttxuser1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "accounttxuser2", "Password123!", _cancelToken);

        BankAccount account = await CreateBankAccountAsync(user1, "Private Account");

        HttpResponseMessage response =
            await user2.GetAsync($"transaction/account/{account.AccountId}", _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }

    private async Task<BankAccount> CreateBankAccountAsync(HttpClient client, string name)
    {
        CreateBankAccountRequest request = new()
        {
            Name = name,
            Description = $"{name} description"
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Bank.CreateAccount, request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        BankAccount? created =
            await response.Content.ReadFromJsonAsync<BankAccount>(_cancelToken);

        Assert.NotNull(created);
        return created;
    }

    private async Task<Transaction> CreateTransactionAsync(
        HttpClient client,
        int accountId,
        string name,
        decimal amount)
    {
        TransactionCreateRequest request = new()
        {
            AccountId = accountId,
            Name = name,
            Amount = amount
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Transaction.CreateTransaction, request, _cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Transaction? created =
            await response.Content.ReadFromJsonAsync<Transaction>(_cancelToken);

        Assert.NotNull(created);
        return created;
    }
}