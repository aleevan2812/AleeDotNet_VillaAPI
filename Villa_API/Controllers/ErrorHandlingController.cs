using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Villa_API.Controllers;

[Route("ErrorHandling")]
[ApiController]
[AllowAnonymous]
[ApiVersionNeutral]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorHandlingController : ControllerBase
{
    [Route("ProcessError")]
    public IActionResult ProcessError()
    {
        return Problem();
    }
}