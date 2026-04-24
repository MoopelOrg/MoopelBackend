using MoopelApi.Models;
using MoopelApi.Models.Dto;

namespace MoopelApi.Services.Interfaces;

public interface IContextService
{
    public Task<RequestContext> GetRequestContext(HttpContext context);
    public Task<User> GetContextUser(HttpContext context);
}
