using Microsoft.Data.SqlClient;
using MoopelApi.Models;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SharpImage = SixLabors.ImageSharp.Image;

using static MoopelApi.ConstantValues.Database;

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
        if (imageFile is null || imageFile.Length == 0)
        {
            throw new ArgumentException("Invalid image file");
        }

        if (imageFile.Length > 5 * 1024 * 1024)
        {
            throw new ArgumentException("File too large");
        }

        if (!imageFile.ContentType.StartsWith("image/"))
        {
            throw new ArgumentException("File must be an image");
        }

        using var memoryStream = new MemoryStream();
        await imageFile.CopyToAsync(memoryStream);

        byte[] imageBytes = memoryStream.ToArray();

        try
        {
            using SharpImage img = SharpImage.Load(imageBytes);
        }
        catch
        {
            throw new ArgumentException("Invalid image file");
        }

        byte[] thumbnailBytes = GenerateThumbnail(imageBytes);

        await using SqlConnection connection = new(_connection.ConnectionString);
        await connection.OpenAsync();

        await using SqlCommand command = connection.CreateCommand();

        command.CommandText =
            $"""

                         INSERT INTO {ImagesTable.SelectName}
                         (
                             {ImageNameField.SelectName},
                             {ImageContentTypeField.SelectName},
                             {ImageDataField.SelectName},
                             {ImageThumbnailField.SelectName}
                         )
                         OUTPUT INSERTED.{ImageIdField.SelectName}
                         VALUES
                         (
                             @Name,
                             @ContentType,
                             @Data,
                             @Thumbnail
                         );
                     
             """;

        command.Parameters.Add(ImageNameField.GenerateParameter("@Name", imageFile.FileName));
        command.Parameters.Add(ImageContentTypeField.GenerateParameter("@ContentType", imageFile.ContentType));
        command.Parameters.Add(ImageDataField.GenerateParameter("@Data", imageBytes));
        command.Parameters.Add(ImageThumbnailField.GenerateParameter("@Thumbnail", thumbnailBytes));

        int imageId = (int)(await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException("Failed to upload image."));

        return imageId;
    }

    public async Task<MoopelImage> GetImage(int id)
    {
        await using SqlConnection connection = new(_connection.ConnectionString);
        await connection.OpenAsync();

        await using SqlCommand command = connection.CreateCommand();

        command.CommandText =
            $"""

                         SELECT 
                             {ImageIdField.SelectName},
                             {ImageContentTypeField.SelectName},
                             {ImageDataField.SelectName},
                             {ImageThumbnailField.SelectName}
                         FROM {ImagesTable.SelectName}
                         WHERE {ImageIdField.SelectName} = @ImageId;
                     
             """;

        command.Parameters.Add(ImageIdField.GenerateParameter("@ImageId", id));

        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            throw new KeyNotFoundException($"Image [{id}] was not found.");
        }

        string contentType = reader.GetString(reader.GetOrdinal(ImageContentTypeField.Name));
        byte[] imageData = (byte[])reader[ImageDataField.Name];
        byte[]? thumbnailData = reader[ImageThumbnailField.Name] is DBNull
            ? null
            : (byte[])reader[ImageThumbnailField.Name];

        return new MoopelImage
        {
            Id = id,
            FullImage = new MoopelFile
            {
                ContentType = contentType,
                Data = imageData
            },
            Thumbnail = thumbnailData is null
                ? null
                : new MoopelFile
                {
                    ContentType = "image/jpeg",
                    Data = thumbnailData
                }
        };
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
