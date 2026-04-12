namespace MoopelApi.Models.Configuration;

public class JwtSettings
{
    public required string Audience { get; set; }
    public required string Issuer { get; set; }
    public required string JwtKey { get; set; }
    public required int ExpiryMinutes { get; set; }
}
