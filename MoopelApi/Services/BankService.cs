using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Services;

public class BankService : IBankService
{
    private readonly ILogger<BankService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public BankService(ILogger<BankService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public Task<BankAccount> CreateBankAccount(BankAccountCreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Transaction> CreateTransaction(int userId, TransactionCreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<TransactionItem> CreateTransactionItem(int transactionId, TransactionItemCreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task DeleteBankAccount(int accountId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTransaction(int transactionId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTransactionItem(int itemId)
    {
        throw new NotImplementedException();
    }

    public Task<BankAccount?> GetBankAccountById(int accountId)
    {
        throw new NotImplementedException();
    }

    public Task<Transaction?> GetTransactionById(int transactionId)
    {
        throw new NotImplementedException();
    }

    public Task<TransactionItem?> GetTransactionItem(int itemId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Transaction>?> GetTransactionsByAccount(int accountId, CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<BankAccount>> GetUserBankAccounts(int userId, CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }

    public Task<BankAccount> UpdateBankAccount(int accountId, BankAccountUpdateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<TransactionItem> UpdateTransactionItem(int itemId, TransactionItemUpdateRequest request)
    {
        throw new NotImplementedException();
    }
}
