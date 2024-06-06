using Newtonsoft.Json;

namespace Villa_API.Middlewares;

public class CustomExceptionMiddleware
{
    private readonly RequestDelegate _requestDelegate;

    public CustomExceptionMiddleware(RequestDelegate requestDelegate)
    {
        _requestDelegate = requestDelegate;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _requestDelegate(context);
        }
        catch (Exception ex)
        {
            await ProcessException(context, ex);
        }
    }

    private async Task ProcessException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        if (ex is BadImageFormatException badImageException)
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                //if you had custom exception where you are passing status code then you can pass here
                StatusCode = 776,
                ErrorMessage = "Hello From Custom Middleware! Image Format is invalid"
            }));
        else if (ex is FileNotFoundException fileNotFoundException)
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                //if you had custom exception where you are passing status code then you can pass here
                StatusCode = 777,
                ErrorMessage = "Hello From Custom Middleware! this is FileNotFoundException exception"
            }));
        else
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                context.Response.StatusCode,
                ErrorMessage = "Hello From Middleware! - Finale"
            }));
    }
}