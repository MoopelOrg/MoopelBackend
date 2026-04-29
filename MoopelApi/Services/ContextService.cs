using MoopelApi.Models;
using MoopelApi.Models.Dto;
using MoopelApi.Services.Interfaces;

namespace MoopelApi.Services;

public class ContextService : IContextService
{
    public Task<User> GetContextUser(HttpContext context)
    {
        throw new NotImplementedException();
    }

    public Task<RequestContext> GetRequestContext(HttpContext context)
    {
        throw new NotImplementedException();
    }
}
