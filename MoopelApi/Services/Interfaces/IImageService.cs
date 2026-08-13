using MoopelObjects.Dto;

namespace MoopelApi.Services.Interfaces;

public interface IImageService
{
    Task<int> UploadImage(IFormFile imageFile);
    Task<MoopelImage> GetImage(int id);
}
