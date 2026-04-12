using MoopelApi.Models;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Services;

public class ContextService : IContextService
{
    public Task<RequestContext> GetRequestContext(HttpContext context)
    {
        throw new NotImplementedException();
    }
}
