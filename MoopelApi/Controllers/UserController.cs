using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoopelApi.Controllers;

[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Routes.User.Login)]
    public async Task Login()
    {
        _logger.LogInformation("Login");
    }

    //[Authorize]
    [HttpPost(Routes.User.Logout)]
    public async Task Logout()
    {
        _logger.LogInformation("Logout");
    }

    //[Authorize]
    [HttpGet(Routes.User.Me)]
    public async Task Me()
    {
        _logger.LogInformation("Me");
    }

    //[Authorize]
    [HttpGet(Routes.User.UserById)]
    public async Task GetUser(int userId)
    {
        _logger.LogInformation("Retrieve user[{Id}]", userId);
    }
}
