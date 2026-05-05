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
public class StoragesController : ControllerBase
{
    private readonly ILogger<StoragesController> _logger;
    private readonly IContextService _contextService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IStorageService _storageService;

    public StoragesController(ILogger<StoragesController> logger,
        IContextService contextService,
        IAuthService authService,
        IUserService userService,
        IStorageService storageService)
    {
        _logger = logger;
        _contextService = contextService;
        _authService = authService;
        _userService = userService;
        _storageService = storageService;
    }

    [HttpGet(Routes.Storage.MyStorages)]
    public async Task<ActionResult<List<Storage>>> GetUserStorages(CancellationToken cancelToken)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to get storages for user[{Id}] from requestContext[{Context}]",
            contextUser.UserId, requestContext);

        List<Storage>? userStorages = await _storageService.GetStoragesByUserId(contextUser.UserId, cancelToken);

        return Ok(userStorages ?? []);
    }

    [HttpGet(Routes.Storage.StorageById)]
    public async Task<ActionResult<Storage>> GetStorageById(int storageId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to get storage[{Id}] from requestContext[{Context}]",
            storageId, requestContext);

        Storage? storage = await _storageService.GetStorageById(storageId);

        if (storage is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get storage[{Id}] which was not found.",
                requestContext, storageId);
            return NotFound();
        }
        if (storage.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to get storage[{Id}] but was forbidden.",
                requestContext, storageId);
            return Forbid();
        }

        return Ok(storage);
    }

    [HttpPost(Routes.Storage.CreateStorage)]
    public async Task<ActionResult<Storage>> CreateStorage(StorageCreateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to create storage[{Request}] from requestContext[{Context}]",
            request, requestContext);

        Storage storage = await _storageService.CreateStorage(contextUser.UserId, request);

        return CreatedAtAction(nameof(GetStorageById), new { storageId = storage.StorageId }, storage);
    }

    [HttpPatch(Routes.Storage.UpdateStorage)]
    public async Task<ActionResult<Storage>> UpdateStorage(int storageId, StorageUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to update storage[{Id}] using[{Request}] from requestContext[{Context}]",
            storageId, request, requestContext);

        Storage? storage = await _storageService.GetStorageById(storageId);

        if (storage is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get storage[{Id}] which was not found.",
                requestContext, storageId);
            return NotFound();
        }
        if (storage.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to get storage[{Id}] but was forbidden.",
                requestContext, storageId);
            return Forbid();
        }

        Storage updatedStorage = await _storageService.UpdateStorage(storageId, request);

        return Ok(updatedStorage);
    }

    [HttpDelete(Routes.Storage.DeleteStorage)]
    public async Task<ActionResult> DeleteStorage(int storageId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to delete storage[{Id}] from requestContext[{Context}]",
            storageId, requestContext);

        Storage? storage = await _storageService.GetStorageById(storageId);

        if (storage is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get storage[{Id}] which was not found.",
                requestContext, storageId);
            return NotFound();
        }
        if (storage.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to get storage[{Id}] but was forbidden.",
                requestContext, storageId);
            return Forbid();
        }

        await _storageService.DeleteStorage(storageId);

        return NoContent();
    }

    [HttpPost(Routes.Storage.CreateStorageItem)]
    public async Task<ActionResult<StorageItem>> CreateStorageItem(int storageId, StorageCreateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to create storage[{Id}] from requestContext[{Context}]",
            storageId, requestContext);

        Storage? storage = await _storageService.GetStorageById(storageId);

        if (storage is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get storage[{Id}] which was not found.",
                requestContext, storageId);
            return NotFound();
        }
        if (storage.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to get storage[{Id}] but was forbidden.",
                requestContext, storageId);
            return Forbid();
        }

        StorageItem createdItem = await _storageService.CreateStorageItem(storageId, request);

        return CreatedAtAction(nameof(GetStorageById), new { storageId }, createdItem);
    }

    [HttpPatch(Routes.Storage.UpdateStorageItem)]
    public async Task<ActionResult<StorageItem>> UpdateStorageItem(int storageItemId, StorageUpdateRequest request)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to update storage item[{Id}] from requestContext[{Context}]",
            storageItemId, requestContext);

        StorageItem? item = await _storageService.GetStorageItemById(storageItemId);
        if (item is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get storage item[{Id}] which was not found.",
                requestContext, storageItemId);
            return NotFound();
        }

        Storage? storage = await _storageService.GetStorageById(item.StorageId);

        if (storage is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get storage[{Id}] which was not found.",
                requestContext, item.StorageId);
            return NotFound();
        }
        if (storage.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to update an item in storage[{Id}] but was forbidden.",
                requestContext, item.StorageId);
            return Forbid();
        }

        StorageItem updatedItem = await _storageService.UpdateStorageItem(item.ItemId, request);

        return Ok(updatedItem);
    }

    [HttpDelete(Routes.Storage.DeleteStorageItem)]
    public async Task<ActionResult> DeleteStorageItem(int storageItemId)
    {
        User contextUser = await _contextService.GetContextUser(HttpContext);
        RequestContext requestContext = await _contextService.GetRequestContext(HttpContext);

        _logger.LogInformation("Received request to delete storage item[{Id}] from requestContext[{Context}]",
            storageItemId, requestContext);

        StorageItem? item = await _storageService.GetStorageItemById(storageItemId);
        if (item is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get storage item[{Id}] which was not found.",
                requestContext, storageItemId);
            return NotFound();
        }

        Storage? storage = await _storageService.GetStorageById(item.StorageId);

        if (storage is null)
        {
            _logger.LogInformation("Request[{Context}] attempted to get storage[{Id}] which was not found.",
                requestContext, item.StorageId);
            return NotFound();
        }
        if (storage.UserId != contextUser.UserId)
        {
            _logger.LogWarning("Request[{Context}] attempted to delete an item in storage[{Id}] but was forbidden.",
                requestContext, item.StorageId);
            return Forbid();
        }

        await _storageService.DeleteStorageItem(storageItemId);

        return NoContent();
    }
}
