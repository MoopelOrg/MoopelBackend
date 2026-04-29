using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoopelApi.Services.Interfaces;
using SixLabors.ImageSharp;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(ILogger<ImageController> logger,
                           IImageService imageService)
    {
        _logger = logger;
        _imageService = imageService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        byte[] imageBytes = memoryStream.ToArray();

        //byte[] thumbnailBytes = GenerateThumbnail(imageBytes);

        throw new NotImplementedException();
        //return Ok(new { image.ImageId });
    }
}
