using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Models.Dto;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
[Route("image")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(ILogger<ImageController> logger, IImageService imageService)
    {
        _logger = logger;
        _imageService = imageService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        int imageId = await _imageService.UploadImage(file);

        return Ok(new { ImageId = imageId });
    }

    [HttpGet(Routes.Image.ImageById)]
    public async Task<IActionResult> GetImage(int imageId)
    {
        MoopelImage image = await _imageService.GetImage(imageId);

        if (image.FullImage is null)
        {
            return NotFound();
        }

        return File(image.FullImage.Data, image.FullImage.ContentType);
    }

    [HttpGet(Routes.Image.ThumbnailById)]
    public async Task<IActionResult> GetThumbnail(int imageId)
    {
        MoopelImage image = await _imageService.GetImage(imageId);

        if (image.Thumbnail is null)
        {
            return NotFound();
        }

        return File(image.Thumbnail.Data, image.Thumbnail.ContentType);
    }
}