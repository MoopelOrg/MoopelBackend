using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    private readonly IContextService _contextService;

    public UserController(ILogger<UserController> logger,
                          IUserService userService,
                          IContextService contextService)
    {
        _logger = logger;
        _userService = userService;
        _contextService = contextService;
    }

    [HttpGet(Routes.User.UserById)]
    public async Task GetUser(int userId)
    {
        _logger.LogInformation("Retrieve user[{Id}]", userId);
    }
}
