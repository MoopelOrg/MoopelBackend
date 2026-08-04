using Microsoft.Data.SqlClient;

using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

using static MoopelApi.ConstantValues.Database;

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

    public async Task<BankAccount> CreateBankAccount(int userId, BankAccountCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Creating bank account for user [{UserId}]", userId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 INSERT INTO {BankAccountsTable.SelectName}
                                 ({BankAccountNameField.SelectName},
                                  {BankAccountDescriptionField.SelectName},
                                  {BankAccountUserIdField.SelectName})
                                 OUTPUT INSERTED.{BankAccountIdField.SelectName} 
                                 VALUES
                                 (@AccountName, @Description, @UserId)
                             
                 """;
            command.Parameters.Add(BankAccountNameField.GenerateParameter("@AccountName", request.Name));
            command.Parameters.Add(BankAccountDescriptionField.GenerateParameter("@Description", request.Description));
            command.Parameters.Add(BankAccountUserIdField.GenerateParameter("@UserId", userId));

            int accountId = (int)(await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create bank account."));

            await transaction.CommitAsync();

            return new()
            {
                AccountId = accountId,
                UserId = userId,

                Name = request.Name,
                Description = request.Description,
                Balance = 0m,

                CreatedAtUtc = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Bank Account",
                ex.Message,
                [
                    ("UserId", userId),
                    ("CreateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Transaction> CreateTransaction(int accountId, TransactionCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Creating transaction for account [{AccountId}]", accountId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 INSERT INTO {TransactionsTable.SelectName}
                                 ({TransactionNameField.SelectName}, 
                                  {TransactionDescriptionField.SelectName})
                                 OUTPUT INSERTED.{TransactionIdField.SelectName} 
                                 VALUES
                                 (@Name, @Description)
                             
                 """;
            command.Parameters.Add(TransactionNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(TransactionDescriptionField.GenerateParameter("@Description", request.Description));

            int transactionId = (int)(await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create transaction."));

            await transaction.CommitAsync();

            return new()
            {
                TransactionId = transactionId,
                AccountId = accountId,

                Name = request.Name,
                Description = request.Description,

                Amount = 0m,

                CreatedAtUtc = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Transaction",
                ex.Message,
                [
                    ("AccountId", accountId),
                    ("CreateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<TransactionItem> CreateTransactionItem(int transactionId, TransactionItemCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Creating transaction item for transaction [{TransactionId}]", transactionId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 INSERT INTO {TransactionItemsTable.SelectName}
                                 ({TransactionItemTransactionIdField.SelectName}, 
                                  {TransactionItemNameField.SelectName},
                                  {TransactionItemAmountField.SelectName})
                                 OUTPUT INSERTED.{TransactionIdField.SelectName} 
                                 VALUES
                                 (@TransactionId, @Name, @Amount)
                             
                 """;
            command.Parameters.Add(TransactionItemTransactionIdField.GenerateParameter("@TransactionId", transactionId));
            command.Parameters.Add(TransactionItemNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(TransactionItemAmountField.GenerateParameter("@Amount", request.Amount));

            int itemId = (int)(await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException("Failed to create transaction item."));

            await transaction.CommitAsync();

            return new()
            {
                TransactionId = transactionId,
                ItemId = itemId,

                Name = request.Name,

                Amount = request.Amount,

                CreatedAtUtc = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Creating Transaction Item",
                ex.Message,
                [
                    ("TransactionId", transactionId),
                    ("CreateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteBankAccount(int accountId)
    {
        _logger.LogInformation("Deleting bank account [{AccountId}]", accountId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 DELETE FROM {BankAccountsTable.SelectName}
                                 WHERE {BankAccountIdField.SelectName} = @AccountId
                             
                 """;
            command.Parameters.Add(BankAccountIdField.GenerateParameter("@AccountId", accountId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Bank Account",
                ex.Message,
                [
                    ("AccountId", accountId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteTransaction(int transactionId)
    {
        _logger.LogInformation("Deleting transaction [{TransactionId}]", transactionId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 DELETE FROM {TransactionsTable.SelectName}
                                 WHERE {TransactionIdField.SelectName} = @TransactionId
                             
                 """;
            command.Parameters.Add(TransactionIdField.GenerateParameter("@TransactionId", transactionId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Transaction",
                ex.Message,
                [
                    ("TransactionId", transactionId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task DeleteTransactionItem(int itemId)
    {
        _logger.LogInformation("Deleting transaction item [{ItemId}]", itemId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 DELETE FROM {TransactionItemsTable.SelectName}
                                 WHERE {TransactionItemIdField.SelectName} = @ItemId
                             
                 """;
            command.Parameters.Add(TransactionItemIdField.GenerateParameter("@ItemId", itemId));

            int rowsAffected = await command.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Deleting Transaction Item",
                ex.Message,
                [
                    ("ItemId", itemId),
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<BankAccount?> GetBankAccountById(int accountId)
    {
        _logger.LogInformation("Getting bank account [{AccountId}]", accountId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {BankAccountsTable.GenerateSelect()}
                                 WHERE {BankAccountIdField.SelectName} = @AccountId
                             
                 """;
            command.Parameters.Add(BankAccountIdField.GenerateParameter("@AccountId", accountId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                BankAccount account = DtoReaders.ReadBankAccount(reader);
                _logger.LogInformation("Found [{Account}] for [{AccountId}]", account, accountId);
                return account;
            }
            else
            {
                _logger.LogInformation("No item found for [{AccountId}]", accountId);
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Bank Account",
                ex.Message,
                [
                    ("AccountId", accountId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<Transaction?> GetTransactionById(int transactionId)
    {
        _logger.LogInformation("Getting transaction [{TransactionId}]", transactionId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {TransactionsTable.GenerateSelect()}
                                 WHERE {TransactionIdField.SelectName} = @TransactionId
                             
                 """;
            command.Parameters.Add(TransactionIdField.GenerateParameter("@TransactionId", transactionId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Transaction transaction = DtoReaders.ReadTransaction(reader);
                _logger.LogInformation("Found [{Transaction}] for [{TransactionId}]", transaction, transactionId);
                return transaction;
            }
            else
            {
                _logger.LogInformation("No item found for [{TransactionId}]", transactionId);
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Transaction",
                ex.Message,
                [
                    ("AccountId", transactionId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<TransactionItem?> GetTransactionItem(int itemId)
    {
        _logger.LogInformation("Getting transaction item [{ItemId}]", itemId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {TransactionItemsTable.GenerateSelect()}
                                 WHERE {TransactionItemIdField.SelectName} = @ItemId
                             
                 """;
            command.Parameters.Add(TransactionItemIdField.GenerateParameter("@ItemId", itemId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                TransactionItem item = DtoReaders.ReadTransactionItem(reader);
                _logger.LogInformation("Found [{TransactionItem}] for [{ItemId}]", item, itemId);
                return item;
            }
            else
            {
                _logger.LogInformation("No item found for [{TransactionItemId}]", itemId);
                return null;
            }
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Transaction Item",
                ex.Message,
                [
                    ("ItemId", itemId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<Transaction>?> GetTransactionsByAccount(int accountId, CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting transactions for account [{AccountId}]", accountId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 SELECT {BankAccountsTable.GetFields(BankAccountsTable.Alias)},
                                        {TransactionsTable.GetFields(TransactionsTable.Alias)}
                                 FROM {BankAccountsTable.SelectName} AS {BankAccountsTable.Alias}
                                 JOIN {TransactionsTable.SelectName} AS {TransactionsTable.Alias}
                                     ON {BankAccountIdField.SelectName} = {TransactionBankAccountIdField.SelectName}
                                 WHERE {BankAccountIdField.SelectName} = @AccountId
                             
                 """;
            command.Parameters.Add(BankAccountIdField.GenerateParameter("@AccountId", accountId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            if (reader.HasRows is false)
            {
                _logger.LogInformation("No transactions found for account [{AccountId}]", accountId);
                return null;
            }

            List<Transaction> transactions = [];
            while (await reader.ReadAsync(cancelToken))
            {
                transactions.Add(DtoReaders.ReadTransaction(reader, TransactionsTable.Alias));
            }

            return transactions;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request to get all transactions for Account[{AccountId}] was canceled",
                accountId);
            return [];
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Transactions By Account",
                ex.Message,
                [
                    ("AccountId", accountId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<List<BankAccount>?> GetUserBankAccounts(int userId, CancellationToken cancelToken)
    {
        _logger.LogInformation("Getting bank accounts for user [{UserId}]", userId);
        SqlCommand? command = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync(cancelToken);

            command = connection.CreateCommand();

            command.CommandText =
                $"""

                                 {BankAccountsTable.GenerateSelect()}
                                 WHERE {BankAccountUserIdField.SelectName} = @UserId
                             
                 """;
            command.Parameters.Add(BankAccountUserIdField.GenerateParameter("@UserId", userId));

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancelToken);

            if (reader.HasRows is false)
            {
                _logger.LogInformation("No accounts found for user [{UserId}]", userId);
                return null;
            }

            List<BankAccount> transactions = [];
            while (await reader.ReadAsync(cancelToken))
            {
                transactions.Add(DtoReaders.ReadBankAccount(reader));
            }

            return transactions;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request to get all bank accounts for user [{UserId}] was canceled",
                userId);
            return [];
        }
        catch (Exception ex)
        {

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Getting Bank Accounts By User",
                ex.Message,
                [
                    ("UserId", userId)
                ],
                command: command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<BankAccount> UpdateBankAccount(int accountId, BankAccountUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Updating bank account [{AccountId}]", accountId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 UPDATE {BankAccountsTable.SelectName}
                                 SET {BankAccountNameField.SelectName} = @Name
                                 OUTPUT INSERTED.{BankAccountsTable.GetFields()}
                                 WHERE {BankAccountIdField.SelectName} = @AccountId
                             
                 """;
            command.Parameters.Add(BankAccountNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(BankAccountIdField.GenerateParameter("@AccountId", accountId));

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            BankAccount account = DtoReaders.ReadBankAccount(reader);

            await transaction.CommitAsync();

            return account;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Bank Account",
                ex.Message,
                [
                    ("AccountId", accountId),
                    ("UpdateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }

    public async Task<TransactionItem> UpdateTransactionItem(int itemId, TransactionItemUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Updating transaction item [{ItemId}]", itemId);
        SqlCommand? command = null;
        SqlTransaction? transaction = null;

        try
        {
            await using SqlConnection connection = new(_connection.ConnectionString);
            await connection.OpenAsync();

            transaction = connection.BeginTransaction();
            command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText =
                $"""

                                 UPDATE {TransactionItemsTable.SelectName}
                                 SET {TransactionItemNameField.SelectName} = @Name,
                                     {TransactionItemAmountField.SelectName} = @Amount
                                 OUTPUT INSERTED.{TransactionItemsTable.GetFields()}
                                 WHERE {TransactionItemIdField.SelectName} = @ItemId
                             
                 """;
            command.Parameters.Add(TransactionItemNameField.GenerateParameter("@Name", request.Name));
            command.Parameters.Add(TransactionItemAmountField.GenerateParameter("@Amount", request.Amount));
            command.Parameters.Add(TransactionItemIdField.GenerateParameter("@ItemId", itemId));

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            TransactionItem item = DtoReaders.ReadTransactionItem(reader);
            await transaction.CommitAsync();

            return item;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync();

            string logErrorMessage = FormatHelper.FormatErrorLogMessage(
                "Updating Transaction Item",
                ex.Message,
                [
                    ("ItemId", itemId),
                    ("UpdateRequest", request)
                ],
                command);

            _logger.LogCritical(ex, "{Error}", logErrorMessage);

            throw;
        }
    }
}
