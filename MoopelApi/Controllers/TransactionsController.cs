using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
public class TransactionsController : ControllerBase
{
    private readonly ILogger<TransactionsController> _logger;
    private readonly IContextService _contextService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IBankService _bankService;

    public TransactionsController(ILogger<TransactionsController> logger,
                                  IContextService contextService,
                                  IAuthService authService,
                                  IUserService userService,
                                  IBankService bankService)
    {
        _logger = logger;
        _contextService = contextService;
        _authService = authService;
        _userService = userService;
        _bankService = bankService;
    }

    [HttpGet(Routes.Transaction.TransactionsByAccountId)]
    public async Task<ActionResult<List<Transaction>>> GetTransactionsForAccount(int accountId, CancellationToken cancelToken)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to get transactions for account[{AccountId}] from requestContext[{Context}]",
            accountId, requestContext);

        BankAccount? account = await _bankService.GetBankAccountById(accountId);

        if (account is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get account[{Id}] which was not found.",
                requestContext, accountId);
            return NotFound();
        }
        if (account.UserId != contextUser.UserId)
        {
            _logger.LogInformation("Request[{Context}] attempted to get account[{Id}] which was not assigned to them.",
                requestContext, accountId);
            return Forbid();
        }

        List<Transaction>? transactions = await _bankService.GetTransactionsByAccount(accountId, cancelToken);

        return Ok(transactions ?? []);
    }

    [HttpGet(Routes.Transaction.TransactionById)]
    public async Task<ActionResult<Transaction>> GetTransactionById(int transactionId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to get transaction[{Id}] from requestContext[{Context}]",
            transactionId, requestContext);

        Transaction? transaction = await _bankService.GetTransactionById(transactionId);

        if (transaction is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get transaction[{Id}] which was not found.",
                requestContext, transactionId);
            return NotFound();
        }

        BankAccount? account = await _bankService.GetBankAccountById(transaction.AccountId);

        if (account is null)
        {
            _logger.LogError("Transaction[{Model}] was found, but the account[{Id}] attached was not.",
                transaction, transaction.AccountId);
            return NotFound();
        }
        if (account.UserId != contextUser.UserId)
        {
            _logger.LogInformation("Request[{Context}] attempted to get transaction[{Id}] which was not assigned to them.",
                requestContext, transactionId);
        }

        return Ok(transaction);
    }

    [HttpPost(Routes.Transaction.CreateTransaction)]
    public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] TransactionCreateRequest createRequest)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to create transaction from [{Context}]",
            requestContext);

        BankAccount? account = await _bankService.GetBankAccountById(createRequest.AccountId);

        if (account is null || account.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to create a transaction for account[{Id}] but was forbidden",
                requestContext, createRequest.AccountId);
            return Forbid();
        }

        Transaction createdTransaction = await _bankService.CreateTransaction(contextUser.UserId, createRequest);

        return CreatedAtAction(nameof(GetTransactionById),
            new { transactionId = createdTransaction.TransactionId },
            createdTransaction);
    }

    [HttpDelete(Routes.Transaction.DeleteTransaction)]
    public async Task<ActionResult> DeleteTransaction(int transactionId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to delete transaction[{Id}] from [{Context}]",
            transactionId, requestContext);

        Transaction? transaction = await _bankService.GetTransactionById(transactionId);

        if (transaction is null)
        {
            _logger.LogWarning("[{Context}] attempted to delete transaction[{Id}] which does not exist.",
                requestContext, transactionId);
            return NotFound();
        }

        BankAccount? account = await _bankService.GetBankAccountById(transaction.AccountId);

        if (account is null || account.UserId != contextUser.UserId)
        {
            _logger.LogWarning("[{Context}] attempted to delete transaction[{Id}] but was forbidden",
                requestContext, transactionId);
            return Forbid();
        }

        await _bankService.DeleteTransaction(transactionId);

        return NoContent();
    }

    [HttpPost(Routes.Transaction.CreateTransactionItem)]
    public async Task<ActionResult<TransactionItem>> CreateTransactionItem(int transactionId, TransactionItemCreateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to create item for transaction[{Id}] from [{Context}]",
            transactionId, requestContext);

        Transaction? transaction = await _bankService.GetTransactionById(transactionId);
        if (transaction is null)
        {
            _logger.LogWarning("[{Context}] attempted to add item to transaction[{Id}] which does not exist.",
                requestContext, transactionId);
            return NotFound();
        }

        BankAccount? account = await _bankService.GetBankAccountById(transaction.AccountId);
        if (account is null || account.UserId != contextUser.UserId)
        {
            _logger.LogWarning("[{Context}] attempted to add item to transaction[{Id}] but was forbidden.",
                requestContext, transactionId);
            return Forbid();
        }

        TransactionItem item = await _bankService.CreateTransactionItem(transactionId, request);

        return CreatedAtAction(nameof(GetTransactionById), new { transactionId }, item);
    }

    [HttpPatch(Routes.Transaction.UpdateTransactionItem)]
    public async Task<ActionResult<TransactionItem>> UpdateTransactionItem(int transactionItemId, TransactionItemUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to update transaction item[{Id}] from [{Context}]",
            transactionItemId, requestContext);

        TransactionItem? item = await _bankService.GetTransactionItem(transactionItemId);
        if (item is null)
        {
            _logger.LogWarning("[{Context}] attempted to find transaction item[{Id}] which does not exist.",
                requestContext, transactionItemId);
            return NotFound();
        }

        Transaction? transaction = await _bankService.GetTransactionById(item.TransactionId);
        if (transaction is null)
        {
            _logger.LogWarning("[{Context}] attempted to add item to transaction[{Id}] which does not exist.",
                requestContext, item.TransactionId);
            return NotFound();
        }

        BankAccount? account = await _bankService.GetBankAccountById(transaction.AccountId);
        if (account is null || account.UserId != contextUser.UserId)
        {
            _logger.LogWarning("[{Context}] attempted to add item to transaction[{Id}] but was forbidden.",
                requestContext, item.TransactionId);
            return Forbid();
        }

        TransactionItem updatedItem = await _bankService.UpdateTransactionItem(transactionItemId, request);

        return Ok(updatedItem);
    }

    [HttpDelete(Routes.Transaction.DeleteTransactionItem)]
    public async Task<ActionResult> DeleteTransactionItem(int transactionItemId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to delete transaction item[{Id}] from [{Context}]",
            transactionItemId, requestContext);

        TransactionItem? item = await _bankService.GetTransactionItem(transactionItemId);
        if (item is null)
        {
            _logger.LogWarning("[{Context}] attempted to find transaction item[{Id}] which does not exist.",
                requestContext, transactionItemId);
            return NotFound();
        }

        Transaction? transaction = await _bankService.GetTransactionById(item.TransactionId);
        if (transaction is null)
        {
            _logger.LogWarning("[{Context}] attempted to add item to transaction[{Id}] which does not exist.",
                requestContext, item.TransactionId);
            return NotFound();
        }

        BankAccount? account = await _bankService.GetBankAccountById(transaction.AccountId);
        if (account is null || account.UserId != contextUser.UserId)
        {
            _logger.LogWarning("[{Context}] attempted to add item to transaction[{Id}] but was forbidden.",
                requestContext, item.TransactionId);
            return Forbid();
        }

        await _bankService.DeleteTransactionItem(transactionItemId);

        return NoContent();
    }
}