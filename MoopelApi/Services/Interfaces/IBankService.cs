using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;

namespace MoopelApi.Services.Interfaces;

public interface IBankService
{
    public Task<List<BankAccount>?> GetUserBankAccounts(int userId, CancellationToken cancelToken);
    public Task<BankAccount?> GetBankAccountById(int accountId);

    public Task<BankAccount> CreateBankAccount(int userId, BankAccountCreateRequest request);

    public Task<BankAccount> UpdateBankAccount(int accountId, BankAccountUpdateRequest request);
    public Task DeleteBankAccount(int accountId);


    public Task<List<Transaction>?> GetTransactionsByAccount(int accountId, CancellationToken cancelToken);

    public Task<Transaction?> GetTransactionById(int transactionId);

    public Task<Transaction> CreateTransaction(int accountId, TransactionCreateRequest request);

    public Task DeleteTransaction(int transactionId);

    public Task<TransactionItem?> GetTransactionItem(int itemId);

    public Task<TransactionItem> CreateTransactionItem(int transactionId, TransactionItemCreateRequest request);

    public Task<TransactionItem> UpdateTransactionItem(int itemId, TransactionItemUpdateRequest request);

    public Task DeleteTransactionItem(int itemId);

}
