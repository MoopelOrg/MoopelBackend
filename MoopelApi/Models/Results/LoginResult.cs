using MoopelApi.Models.Dto;
using MoopelApi.Models.Dto.Read;

namespace MoopelApi.Models.Results;

public class LoginResult
{
    public LoginResult()
    {

    }

    public LoginResult(string? token, string? error, UserRead? user)
    {
        token ??= string.Empty;
        Token = token;
        Error = error;

        if (string.IsNullOrWhiteSpace(error))
        {
            User = user;
        }
    }

    public string Token { get; set; } = string.Empty;
    public string? Error { get; set; }

    public UserRead? User { get; set; } = null;

    public Login? Login { get; init; }
    public Session? Session { get; init; }

    public override string ToString()
    {
        return $"LoginResult: [Token: {(string.IsNullOrEmpty(Token) ? "null" : "****")}, Error: {Error ?? "null"}, User: {User}]";
    }
}
