namespace MoopelApi.Exceptions;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound) { }
}