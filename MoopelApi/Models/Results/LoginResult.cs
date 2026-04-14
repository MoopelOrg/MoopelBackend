using MoopelApi.Models.Dto.Read;

namespace MoopelApi.Models.Results;

public class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public string? Error { get; set; }

    public UserRead? User { get; set; } = null;

    public override string ToString()
    {
        return $"LoginResult: [Token: {(string.IsNullOrEmpty(Token) ? "null" : "****")}, Error: {Error ?? "null"}, User: {User}]";
    }
}
