using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

using static MoopelApi.ConstantValues.Database;
using SharpImage = SixLabors.ImageSharp.Image;

namespace MoopelApi.Services;

public class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;
    private readonly INotificationService _notificationService;
    private readonly DatabaseConnection _connection;

    public ImageService(ILogger<ImageService> logger, INotificationService notificationService, DatabaseConnection connection)
    {
        _logger = logger;
        _notificationService = notificationService;
        _connection = connection;
    }

    public async Task<int> UploadImage(IFormFile imageFile)
    {
        if (imageFile.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File too large");

        if (imageFile == null || imageFile.Length == 0)
            throw new ArgumentException("Invalid image file");

        if (!imageFile.ContentType.StartsWith("image/"))
            throw new ArgumentException("File must be an image");

        using var memoryStream = new MemoryStream();
        await imageFile.CopyToAsync(memoryStream);

        byte[] imageBytes = memoryStream.ToArray();

        try
        {
            using var img = SharpImage.Load(imageBytes);
        }
        catch
        {
            throw new ArgumentException("Invalid image file");
        }

        // Generate thumbnail
        byte[] thumbnailBytes = GenerateThumbnail(imageBytes);

        throw new NotImplementedException();
    }
    public async Task<MoopelImage> GetImage(int id)
    {
        throw new NotImplementedException();
    }

    private byte[] GenerateThumbnail(byte[] imageBytes)
    {
        using SharpImage image = SharpImage.Load(imageBytes);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(200, 200),
            Mode = ResizeMode.Max // keeps aspect ratio
        }));

        using var ms = new MemoryStream();
        image.Save(ms, new JpegEncoder { Quality = 75 });

        return ms.ToArray();
    }
}
