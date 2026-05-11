using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Requests.Creation;
using MoopelApi.Models.Requests.Update;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
public class BanksController : ControllerBase
{
    private readonly ILogger<BanksController> _logger;
    private readonly IContextService _contextService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IBankService _bankService;

    public BanksController(ILogger<BanksController> logger,
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

    [HttpGet(Routes.Bank.MyAccounts)]
    public async Task<ActionResult<List<BankAccount>>> GetMyAccounts(CancellationToken cancelToken)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested their bank accounts", contextUser.UserId);

        List<BankAccount> accounts = await _bankService.GetUserBankAccounts(contextUser.UserId, cancelToken);
        return Ok(accounts);
    }

    [HttpGet(Routes.Bank.AccountById)]
    public async Task<ActionResult<BankAccount>> GetAccountById(int accountId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} requested bank account {AccountId}", contextUser.UserId, accountId);

        BankAccount? account = await _bankService.GetBankAccountById(accountId);

        if (account is null)
        {
            return NotFound();
        }
        if (account.UserId != contextUser.UserId)
        {
            return Forbid();
        }

        return Ok(account);
    }

    [HttpPost(Routes.Bank.CreateAccount)]
    public async Task<ActionResult<BankAccount>> CreateAccount([FromBody] BankAccountCreateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is creating a new bank account", contextUser.UserId);

        BankAccount newAccount = await _bankService.CreateBankAccount(contextUser.UserId, request);

        return Ok(newAccount);
    }

    [HttpPatch(Routes.Bank.UpdateAccount)]
    public async Task<ActionResult<BankAccount>> UpdateAccount(int accountId, [FromBody] BankAccountUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is updating bank account {AccountId}", contextUser.UserId, accountId);

        BankAccount? existingAccount = await _bankService.GetBankAccountById(accountId);
        if (existingAccount is null)
        {
            return NotFound();
        }
        if (existingAccount.UserId != contextUser.UserId)
        {
            return Forbid();
        }

        BankAccount updatedAccount = await _bankService.UpdateBankAccount(accountId, request);
        return Ok(updatedAccount);
    }

    [HttpDelete(Routes.Bank.DeleteAccount)]
    public async Task<ActionResult> DeleteAccount(int accountId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        _logger.LogInformation("User {UserId} is deleting bank account {AccountId}", contextUser.UserId, accountId);

        BankAccount? existingAccount = await _bankService.GetBankAccountById(accountId);
        if (existingAccount is null)
        {
            return NotFound();
        }
        if (existingAccount.UserId != contextUser.UserId)
        {
            return Forbid();
        }

        await _bankService.DeleteBankAccount(accountId);
        return NoContent();
    }
}