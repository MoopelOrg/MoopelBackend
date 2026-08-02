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

    #region Get Transactions

    [Fact]
    [TestDocumentation("Get transactions by account should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task TransactionsByAccountId_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Transaction.Generate.TransactionsByAccountId(TransactionTestData.MissingAccountId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [TestDocumentation("Get transactions by account should return transactions when the authenticated user owns the account", HttpStatusCode.OK)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TransactionsByAccountId_ReturnsList_WhenAuthenticatedAndOwnsAccount(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client);
        Transaction created = await CreateTransactionAsync(client, account.AccountId);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Transaction.Generate.TransactionsByAccountId(account.AccountId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        List<Transaction>? result = JsonSerializer.Deserialize<List<Transaction>>(responseBody);
        Assert.True(result is not null, $"Response body could not be deserialized into a List<Transaction>. Body: {responseBody}");
        Assert.Contains(result!, transaction => transaction.TransactionId == created.TransactionId);
        Assert.Contains(result, transaction => transaction.Name == TransactionTestData.TransactionName);
    }

    [Theory]
    [TestDocumentation("Get transactions by account should return not found when the account does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TransactionsByAccountId_ReturnsNotFound_WhenAccountMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Transaction.Generate.TransactionsByAccountId(TransactionTestData.MissingAccountId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Create Transaction

    [Fact]
    [TestDocumentation("Create transaction should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task CreateTransaction_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Transaction.CreateTransaction, TransactionTestData.ValidCreateRequest, _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [TestDocumentation("Create transaction should return created transaction when request is valid", HttpStatusCode.Created)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateTransaction_ReturnsTransaction_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                string.Concat(
                    Routes.Transaction.CreateTransaction,
                    "?",
                    TransactionTestData.AccountIdQueryKey,
                    "=",
                    account.AccountId.ToString()),
                TransactionTestData.ValidCreateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        Transaction? created = JsonSerializer.Deserialize<Transaction>(responseBody);
        Assert.True(created is not null, $"Response body could not be deserialized into a Transaction. Body: {responseBody}");
        Assert.True(created!.TransactionId > 0);
        Assert.Equal(account.AccountId, created.AccountId);
        Assert.Equal(TransactionTestData.TransactionName, created.Name);
        Assert.Equal(TransactionTestData.TransactionDescription, created.Description);
    }

    [Theory]
    [TestDocumentation("Create transaction should return not found when the account does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateTransaction_ReturnsNotFound_WhenAccountMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                string.Concat(
                    Routes.Transaction.CreateTransaction,
                    "?",
                    TransactionTestData.AccountIdQueryKey,
                    "=",
                    TransactionTestData.MissingAccountId.ToString()),
                TransactionTestData.ValidCreateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Get Transaction By Id

    [Fact]
    [TestDocumentation("Get transaction by id should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task TransactionById_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Transaction.Generate.TransactionById(TransactionTestData.MissingTransactionId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [TestDocumentation("Get transaction by id should return the transaction when it exists", HttpStatusCode.OK)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TransactionById_ReturnsTransaction_WhenItExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client);
        Transaction created = await CreateTransactionAsync(client, account.AccountId);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Transaction.Generate.TransactionById(created.TransactionId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        Transaction? result = JsonSerializer.Deserialize<Transaction>(responseBody);
        Assert.True(result is not null, $"Response body could not be deserialized into a Transaction. Body: {responseBody}");
        Assert.Equal(created.TransactionId, result!.TransactionId);
        Assert.Equal(created.AccountId, result.AccountId);
        Assert.Equal(created.Name, result.Name);
        Assert.Equal(created.Description, result.Description);
    }

    [Theory]
    [TestDocumentation("Get transaction by id should return not found when the transaction does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task TransactionById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.GetAsync(Routes.Transaction.Generate.TransactionById(TransactionTestData.MissingTransactionId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Delete Transaction

    [Fact]
    [TestDocumentation("Delete transaction should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task DeleteTransaction_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Transaction.Generate.DeleteTransaction(TransactionTestData.MissingTransactionId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [TestDocumentation("Delete transaction should remove the transaction when it exists and belongs to the user", HttpStatusCode.NoContent)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTransaction_RemovesTransaction_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client);
        Transaction created = await CreateTransactionAsync(client, account.AccountId);

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Transaction.Generate.DeleteTransaction(created.TransactionId), _cancelToken);
        HttpResponseMessage getResponse =
            await client.GetAsync(Routes.Transaction.Generate.TransactionById(created.TransactionId), _cancelToken);

        // Assert
        string deleteResponseBody = await deleteResponse.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent,

            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.NoContent}, actual {deleteResponse.StatusCode}, endpoint {deleteResponse.RequestMessage?.RequestUri}, raw body: {deleteResponseBody}");
        string getResponseBody = await getResponse.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(getResponse.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {getResponse.StatusCode}, endpoint {getResponse.RequestMessage?.RequestUri}, raw body: {getResponseBody}");
    }

    [Theory]
    [TestDocumentation("Delete transaction should return not found when the transaction does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTransaction_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Transaction.Generate.DeleteTransaction(TransactionTestData.MissingTransactionId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Create Transaction Item

    [Fact]
    [TestDocumentation("Create transaction item should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task CreateTransactionItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                Routes.Transaction.Generate.CreateTransactionItem(TransactionTestData.MissingAccountId),
                TransactionTestData.ValidItemCreateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [TestDocumentation("Create transaction item should return created item when request is valid", HttpStatusCode.Created)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateTransactionItem_ReturnsItem_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client);
        Transaction transaction = await CreateTransactionAsync(client, account.AccountId);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                string.Concat(
                    Routes.Transaction.Generate.CreateTransactionItem(account.AccountId),
                    "?",
                    TransactionTestData.TransactionIdQueryKey,
                    "=",
                    transaction.TransactionId.ToString()),
                TransactionTestData.ValidItemCreateRequest,
                _cancelToken);
        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        TransactionItem? created = JsonSerializer.Deserialize<TransactionItem>(responseBody);
        Assert.True(created is not null, $"Response body could not be deserialized into a TransactionItem. Body: {responseBody}");
        Assert.True(created!.ItemId > 0);
        Assert.Equal(transaction.TransactionId, created.TransactionId);
        Assert.Equal(TransactionTestData.TransactionItemName, created.Name);
        Assert.Equal(TransactionTestData.ValidItemCreateRequest.Amount, created.Amount);
    }

    [Theory]
    [TestDocumentation("Create transaction item should return not found when the account or transaction does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateTransactionItem_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                string.Concat(
                    Routes.Transaction.Generate.CreateTransactionItem(TransactionTestData.MissingAccountId),
                    "?",
                    TransactionTestData.TransactionIdQueryKey,
                    "=",
                    TransactionTestData.MissingTransactionId.ToString()),
                TransactionTestData.ValidItemCreateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Update Transaction Item

    [Fact]
    [TestDocumentation("Update transaction item should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task UpdateTransactionItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(
                Routes.Transaction.Generate.UpdateTransactionItem(TransactionTestData.MissingTransactionItemId),
                TransactionTestData.ValidItemUpdateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [TestDocumentation("Update transaction item should return updated item when request is valid", HttpStatusCode.OK)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTransactionItem_UpdatesItem_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client);
        Transaction transaction = await CreateTransactionAsync(client, account.AccountId);
        TransactionItem created = await CreateTransactionItemAsync(client, account.AccountId, transaction.TransactionId);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(
                Routes.Transaction.Generate.UpdateTransactionItem(created.ItemId),
                TransactionTestData.ValidItemUpdateRequest,
                _cancelToken);
        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected status {HttpStatusCode.OK}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        TransactionItem? updated = JsonSerializer.Deserialize<TransactionItem>(responseBody);
        Assert.True(updated is not null, $"Response body could not be deserialized into a TransactionItem. Body: {responseBody}");
        Assert.Equal(created.ItemId, updated!.ItemId);
        Assert.Equal(created.TransactionId, updated.TransactionId);
        Assert.Equal(TransactionTestData.UpdatedTransactionItemName, updated.Name);
        Assert.Equal(TransactionTestData.ValidItemUpdateRequest.Amount, updated.Amount);
    }

    [Theory]
    [TestDocumentation("Update transaction item should return not found when the item does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateTransactionItem_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.PatchAsJsonAsync(
                Routes.Transaction.Generate.UpdateTransactionItem(TransactionTestData.MissingTransactionItemId),
                TransactionTestData.ValidItemUpdateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Delete Transaction Item

    [Fact]
    [TestDocumentation("Delete transaction item should be unauthorized with no token", HttpStatusCode.Unauthorized)]
    public async Task DeleteTransactionItem_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Transaction.Generate.DeleteTransactionItem(TransactionTestData.MissingTransactionItemId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,

            $"Expected status {HttpStatusCode.Unauthorized}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Theory]
    [TestDocumentation("Delete transaction item should remove the item when it exists and belongs to the user", HttpStatusCode.NoContent)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTransactionItem_RemovesItem_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(client);
        Transaction transaction = await CreateTransactionAsync(client, account.AccountId);
        TransactionItem created = await CreateTransactionItemAsync(client, account.AccountId, transaction.TransactionId);

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync(Routes.Transaction.Generate.DeleteTransactionItem(created.ItemId), _cancelToken);
        HttpResponseMessage secondDeleteResponse =
            await client.DeleteAsync(Routes.Transaction.Generate.DeleteTransactionItem(created.ItemId), _cancelToken);

        // Assert
        string deleteResponseBody = await deleteResponse.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent,

            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.NoContent}, actual {deleteResponse.StatusCode}, endpoint {deleteResponse.RequestMessage?.RequestUri}, raw body: {deleteResponseBody}");
        string secondDeleteResponseBody = await secondDeleteResponse.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(secondDeleteResponse.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {secondDeleteResponse.StatusCode}, endpoint {secondDeleteResponse.RequestMessage?.RequestUri}, raw body: {secondDeleteResponseBody}");
    }

    [Theory]
    [TestDocumentation("Delete transaction item should return not found when the item does not exist", HttpStatusCode.NotFound)]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteTransactionItem_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();
        await GenericActions.Auth.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response =
            await client.DeleteAsync(Routes.Transaction.Generate.DeleteTransactionItem(TransactionTestData.MissingTransactionItemId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    #region Generic Access Control

    [Fact]
    [TestDocumentation("One user should not be able to retrieve another user's transaction", HttpStatusCode.Forbidden)]
    public async Task OneUser_CannotAccess_AnotherUsersTransaction()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(user1);
        Transaction created = await CreateTransactionAsync(user1, account.AccountId);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Transaction.Generate.TransactionById(created.TransactionId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    [TestDocumentation("One user should not be able to list another user's account transactions", HttpStatusCode.Forbidden)]
    public async Task OneUser_CannotList_AnotherUsersAccountTransactions()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(user1);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync(Routes.Transaction.Generate.TransactionsByAccountId(account.AccountId), _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    [Fact]
    [TestDocumentation("One user should not be able to update another user's transaction item", HttpStatusCode.Forbidden)]
    public async Task OneUser_CannotAccess_AnotherUsersTransactionItem()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericActions.Auth.CreateAuthenticatedClient(user1, AuthTestData.ValidUserName, AuthTestData.ValidPassword, _cancelToken);
        await GenericActions.Auth.CreateAuthenticatedClient(user2, AuthTestData.ValidUserName2, AuthTestData.ValidPassword2, _cancelToken);

        BankAccount account = await CreateBankAccountAsync(user1);
        Transaction transaction = await CreateTransactionAsync(user1, account.AccountId);
        TransactionItem created = await CreateTransactionItemAsync(user1, account.AccountId, transaction.TransactionId);

        // Act
        HttpResponseMessage response =
            await user2.PatchAsJsonAsync(
                Routes.Transaction.Generate.UpdateTransactionItem(created.ItemId),
                TransactionTestData.ValidItemUpdateRequest,
                _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,

            $"Expected status {HttpStatusCode.Forbidden} or {HttpStatusCode.NotFound}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
    }

    #endregion

    private async Task<BankAccount> CreateBankAccountAsync(HttpClient client)
    {
        BankAccountCreateRequest request = new()
        {
            Name = BankAccountTestData.ValidCreateRequest.Name,
            Description = BankAccountTestData.ValidCreateRequest.Description
        };

        HttpResponseMessage response =
            await client.PostAsJsonAsync(Routes.Bank.CreateAccount, request, _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        BankAccount? created = JsonSerializer.Deserialize<BankAccount>(responseBody);
        Assert.True(created is not null, $"Response body could not be deserialized into a BankAccount. Body: {responseBody}");
        return created!;
    }

    private async Task<Transaction> CreateTransactionAsync(HttpClient client, int accountId)
    {
        string endpoint = string.Concat(
            Routes.Transaction.CreateTransaction,
            "?",
            TransactionTestData.AccountIdQueryKey,
            "=",
            accountId.ToString());

        HttpResponseMessage response =
            await client.PostAsJsonAsync(endpoint, TransactionTestData.ValidCreateRequest, _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        Transaction? created = JsonSerializer.Deserialize<Transaction>(responseBody);
        Assert.True(created is not null, $"Response body could not be deserialized into a Transaction. Body: {responseBody}");
        return created!;
    }

    private async Task<TransactionItem> CreateTransactionItemAsync(HttpClient client, int accountId, int transactionId)
    {
        string endpoint = string.Concat(
            Routes.Transaction.Generate.CreateTransactionItem(accountId),
            "?",
            TransactionTestData.TransactionIdQueryKey,
            "=",
            transactionId.ToString());

        HttpResponseMessage response =
            await client.PostAsJsonAsync(endpoint, TransactionTestData.ValidItemCreateRequest, _cancelToken);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync(_cancelToken);
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected status {HttpStatusCode.OK} or {HttpStatusCode.Created}, actual {response.StatusCode}, endpoint {response.RequestMessage?.RequestUri}, raw body: {responseBody}");
        TransactionItem? created = JsonSerializer.Deserialize<TransactionItem>(responseBody);
        Assert.True(created is not null, $"Response body could not be deserialized into a TransactionItem. Body: {responseBody}");
        return created!;
    }
}
