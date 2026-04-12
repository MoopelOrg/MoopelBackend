using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoopelApi.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Routes.Auth.Login)]
    public async Task Login()
    {
        _logger.LogInformation("Login");
    }

    //[Authorize]
    [HttpPost(Routes.Auth.Logout)]
    public async Task Logout()
    {
        _logger.LogInformation("Logout");
    }

    //[Authorize]
    [HttpGet(Routes.Auth.Me)]
    public async Task Me()
    {
        _logger.LogInformation("Me");
    }

    //[Authorize]
    [HttpGet(Routes.Auth.UserById)]
    public async Task GetUser(int userId)
    {
        _logger.LogInformation("Retrieve user[{Id}]", userId);
    }
}
