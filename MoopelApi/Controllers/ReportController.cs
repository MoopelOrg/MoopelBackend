using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoopelApi.Controllers;

[Authorize]
[ApiController]
public class ReportController : ControllerBase
{
}
