namespace MoopelApi.Models.Configuration;

public record DatabaseConnection
{
    public required string DatabaseName { get; init; }

    public required string ConnectionString { get; init; }

    public required int Timeout { get; init; }
}
