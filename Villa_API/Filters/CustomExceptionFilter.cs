using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Villa_API.Filters;

public class CustomExceptionFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if(context.Exception is FileNotFoundException fileNotFoundException)
        {
            context.Result = new ObjectResult("File Not found but handled in filter")
            {
                StatusCode = 503
            };
            
            // exceptionHandled has been handled, dont move forward to the default handler
            context.ExceptionHandled = true;
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {

    }
}