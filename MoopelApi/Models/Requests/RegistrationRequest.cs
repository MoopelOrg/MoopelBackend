namespace MoopelApi.Models.Requests;

public record RegistrationRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}
