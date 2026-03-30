namespace MoopelApi.Models.Configuration;

public class DeploymentInfo
{
    public required string Environment { get; init; }
    public required string Version { get; init; }
    public required string Tag { get; init; }
    public required string BuildDate { get; init; }
    public required string CommitName { get; init; }
}
