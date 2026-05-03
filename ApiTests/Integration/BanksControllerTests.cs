using System.Net;
using System.Net.Http.Json;
using ApiTests.Integration.Generic;
using ApiTests.TestData;
using MoopelApi;

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

    [Fact]
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

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task MyAccounts_ReturnsList_WhenAuthenticated(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Bank.MyAccounts, _cancelToken);
        List<BankAccountRead>? accounts = await response.Content.ReadFromJsonAsync<List<BankAccountRead>>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(accounts);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task CreateAccount_ReturnsAccount_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        var request = new CreateBankAccountRequest
        {
            Name = "Checking",
            Description = "Main account"
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(
            Routes.Bank.CreateAccount,
            request,
            _cancelToken);

        BankAccountRead? result = await response.Content.ReadFromJsonAsync<BankAccountRead>(_cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        Assert.NotNull(result);
        Assert.True(result.AccountId > 0);
        Assert.Equal("Checking", result.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task AccountById_ReturnsAccount_WhenExists(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        var createRequest = new CreateBankAccountRequest
        {
            Name = "Savings",
            Description = "Emergency"
        };

        HttpResponseMessage createResponse = await client.PostAsJsonAsync(
            Routes.Bank.CreateAccount,
            createRequest,
            _cancelToken);

        BankAccountRead? created = await createResponse.Content.ReadFromJsonAsync<BankAccountRead>(_cancelToken);
        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await client.GetAsync($"bank/accounts/{created.AccountId}", _cancelToken);

        BankAccountRead? result =
            await response.Content.ReadFromJsonAsync<BankAccountRead>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(created.AccountId, result.AccountId);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task AccountById_ReturnsNotFound_WhenMissing(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        // Act
        HttpResponseMessage response = await client.GetAsync("bank/accounts/999999", _cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task UpdateAccount_UpdatesAccount_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        var createRequest = new CreateBankAccountRequest
        {
            Name = "Checking",
            Description = "Before"
        };

        HttpResponseMessage createResponse = await client.PostAsJsonAsync(
            Routes.Bank.CreateAccount,
            createRequest,
            _cancelToken);

        BankAccountRead? created = await createResponse.Content.ReadFromJsonAsync<BankAccountRead>(_cancelToken);
        Assert.NotNull(created);

        var updateRequest = new UpdateBankAccountRequest
        {
            Name = "Updated",
            Description = "After"
        };

        // Act
        HttpResponseMessage updateResponse = await client.PutAsJsonAsync(
            $"bank/accounts/update/{created.AccountId}",
            updateRequest,
            _cancelToken);

        BankAccountRead? updated =
            await updateResponse.Content.ReadFromJsonAsync<BankAccountRead>(_cancelToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Name);
    }

    [Theory]
    [MemberData(nameof(AuthTestData.ValidUsers), MemberType = typeof(AuthTestData))]
    public async Task DeleteAccount_RemovesAccount_WhenValid(string username, string password)
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();
        HttpClient client = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(client, username, password, _cancelToken);

        var createRequest = new CreateBankAccountRequest
        {
            Name = "DeleteMe",
            Description = "Temp"
        };

        HttpResponseMessage createResponse = await client.PostAsJsonAsync(
            Routes.Bank.CreateAccount,
            createRequest,
            _cancelToken);

        BankAccountRead? created = await createResponse.Content.ReadFromJsonAsync<BankAccountRead>(_cancelToken);
        Assert.NotNull(created);

        // Act
        HttpResponseMessage deleteResponse =
            await client.DeleteAsync($"bank/accounts/delete/{created.AccountId}", _cancelToken);

        HttpResponseMessage getResponse =
            await client.GetAsync($"bank/accounts/{created.AccountId}", _cancelToken);

        // Assert
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK ||
            deleteResponse.StatusCode == HttpStatusCode.NoContent);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task OneUser_CannotAccess_AnotherUsersAccount()
    {
        // Arrange
        await _apiFactory.ResetDatabaseAsync();

        HttpClient user1 = _apiFactory.CreateClient();
        HttpClient user2 = _apiFactory.CreateClient();

        await GenericAuthActions.CreateAuthenticatedClient(user1, "user1", "Password123!", _cancelToken);
        await GenericAuthActions.CreateAuthenticatedClient(user2, "user2", "Password123!", _cancelToken);

        var request = new CreateBankAccountRequest
        {
            Name = "Private",
            Description = "User1 only"
        };

        HttpResponseMessage createResponse =
            await user1.PostAsJsonAsync(Routes.Bank.CreateAccount, request, _cancelToken);

        BankAccountRead? created =
            await createResponse.Content.ReadFromJsonAsync<BankAccountRead>(_cancelToken);

        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await user2.GetAsync($"bank/accounts/{created.AccountId}", _cancelToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound);
    }
}