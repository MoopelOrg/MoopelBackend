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
public class FridgesController : ControllerBase
{
    private readonly ILogger<FridgesController> _logger;
    private readonly IContextService _contextService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IFridgeService _fridgeService;

    public FridgesController(ILogger<FridgesController> logger,
        IContextService contextService,
        IAuthService authService,
        IUserService userService,
        IFridgeService fridgeService)
    {
        _logger = logger;
        _contextService = contextService;
        _authService = authService;
        _userService = userService;
        _fridgeService = fridgeService;
    }

    [HttpGet(Routes.Fridge.MyFridges)]
    public async Task<ActionResult<List<Fridge>>> GetUserFridges(CancellationToken cancelToken)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to get fridges for user[{Id}] from requestContext[{Context}]",
            contextUser.UserId, requestContext);

        List<Fridge>? fridges = await _fridgeService.GetFridgesByUserId(contextUser.UserId, cancelToken);

        return Ok(fridges ?? []);
    }

    [HttpGet(Routes.Fridge.FridgeById)]
    public async Task<ActionResult<Fridge>> GetFridgeById(int fridgeId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to get fridge[{Id}] from requestContext[{Context}]",
            fridgeId, requestContext);

        Fridge? fridge = await _fridgeService.GetFridgeById(fridgeId);

        if (fridge is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get fridge[{Id}] which was not found.",
                requestContext, fridgeId);
            return NotFound();
        }
        if (fridge.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to get fridge[{Id}] but was forbidden.",
                requestContext, fridgeId);
            return Forbid();
        }

        return Ok(fridge);
    }

    [HttpPost(Routes.Fridge.CreateFridge)]
    public async Task<ActionResult<Fridge>> CreateFridge(FridgeCreateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to create fridge[{Request}] from requestContext[{Context}]",
            request, requestContext);

        Fridge fridge = await _fridgeService.CreateFridge(contextUser.UserId, request);

        return CreatedAtAction(nameof(GetFridgeById), new { fridgeId = fridge.FridgeId }, fridge);
    }

    [HttpPatch(Routes.Fridge.UpdateFridge)]
    public async Task<ActionResult<Fridge>> UpdateFridge(int fridgeId, FridgeUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to update fridge[{Id}] from requestContext[{Context}]",
            fridgeId, requestContext);

        Fridge? fridge = await _fridgeService.GetFridgeById(fridgeId);

        if (fridge is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get fridge[{Id}] which was not found.",
                requestContext, fridgeId);
            return NotFound();
        }
        if (fridge.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to update fridge[{Id}] but was forbidden.",
                requestContext, fridgeId);
            return Forbid();
        }

        Fridge updatedFridge = await _fridgeService.UpdateFridge(fridgeId, request);

        return Ok(updatedFridge);
    }

    [HttpDelete(Routes.Fridge.DeleteFridge)]
    public async Task<ActionResult> DeleteFridge(int fridgeId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to delete fridge[{Id}] from requestContext[{Context}]",
            fridgeId, requestContext);

        Fridge? fridge = await _fridgeService.GetFridgeById(fridgeId);

        if (fridge is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get fridge[{Id}] which was not found.",
                requestContext, fridgeId);
            return NotFound();
        }
        if (fridge.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to delete fridge[{Id}] but was forbidden.",
                requestContext, fridgeId);
            return Forbid();
        }

        await _fridgeService.DeleteFridge(fridgeId);

        return NoContent();
    }

    [HttpPost(Routes.Fridge.CreateFridgeItem)]
    public async Task<ActionResult<FridgeItem>> CreateFridgeItem(int fridgeId, FridgeItemCreateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to create item for fridge[{Id}] from requestContext[{Context}]",
            fridgeId, requestContext);

        Fridge? fridge = await _fridgeService.GetFridgeById(fridgeId);

        if (fridge is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get fridge[{Id}] which was not found.",
                requestContext, fridgeId);
            return NotFound();
        }
        if (fridge.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to create an item for fridge[{Id}] but was forbidden.",
                requestContext, fridgeId);
            return Forbid();
        }

        FridgeItem createdItem = await _fridgeService.CreateFridgeItem(fridgeId, request);

        return CreatedAtAction(nameof(GetFridgeById), new { fridgeId }, createdItem);
    }

    [HttpPatch(Routes.Fridge.UpdateFridgeItem)]
    public async Task<ActionResult<FridgeItem>> UpdateFridgeItem(int fridgeItemId, FridgeItemUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to update fridge item[{Id}] from requestContext[{Context}]",
            fridgeItemId, requestContext);

        FridgeItem? item = await _fridgeService.GetFridgeItemById(fridgeItemId);
        if (item is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get fridge item[{Id}] which was not found.",
                requestContext, fridgeItemId);
            return NotFound();
        }

        Fridge? fridge = await _fridgeService.GetFridgeById(item.FridgeId);

        if (fridge is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get fridge[{Id}] which was not found.",
                requestContext, item.FridgeId);
            return NotFound();
        }
        if (fridge.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to update an item[{Id}] for fridge[{Id}] but was forbidden.",
                requestContext, fridgeItemId, item.FridgeId);
            return Forbid();
        }

        FridgeItem updatedItem = await _fridgeService.UpdateFridgeItem(fridgeItemId, request);

        return Ok(updatedItem);
    }

    [HttpDelete(Routes.Fridge.DeleteFridgeItem)]
    public async Task<ActionResult> DeleteFridgeItem(int fridgeItemId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to delete fridge item[{Id}] from requestContext[{Context}]",
            fridgeItemId, requestContext);

        FridgeItem? item = await _fridgeService.GetFridgeItemById(fridgeItemId);
        if (item is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get fridge item[{Id}] which was not found.",
                requestContext, fridgeItemId);
            return NotFound();
        }

        Fridge? fridge = await _fridgeService.GetFridgeById(item.FridgeId);

        if (fridge is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get fridge[{Id}] which was not found.",
                requestContext, item.FridgeId);
            return NotFound();
        }
        if (fridge.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to delete an item[{Id}] for fridge[{Id}] but was forbidden.",
                requestContext, fridgeItemId, item.FridgeId);
            return Forbid();
        }

        await _fridgeService.DeleteFridgeItem(fridgeItemId);

        return NoContent();
    }
}
