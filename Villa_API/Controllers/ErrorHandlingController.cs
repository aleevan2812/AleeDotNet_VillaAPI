using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
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
    public IActionResult ProcessError([FromServices] IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment.IsDevelopment())
        {
            //custom logic
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            return Problem(
                feature.Error.StackTrace,
                title: feature.Error.Message,
                instance: hostEnvironment.EnvironmentName
            );
        }

        return Problem();
    }
}