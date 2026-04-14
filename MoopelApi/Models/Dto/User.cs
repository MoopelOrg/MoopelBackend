using MoopelApi.Models.Dto.Read;

namespace MoopelApi.Models.Dto;

public record User : UserRead
{
    public required string PasswordHash { get; set; }
}
