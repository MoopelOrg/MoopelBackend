using MoopelApi.Models;

namespace MoopelApi.Services.Interfaces;

public interface IContextService
{
    public Task<RequestContext> GetRequestContext(HttpContext context);
}
