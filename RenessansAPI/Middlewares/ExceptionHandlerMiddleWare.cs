using RenessansAPI.Service.Exceptions;

namespace RenessansAPI.Middlewares;

public class ExceptionHandlerMiddleWare
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlerMiddleWare> logger;

    public ExceptionHandlerMiddleWare(RequestDelegate next, ILogger<ExceptionHandlerMiddleWare> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {

        try
        {
            await next.Invoke(context);
        }
        catch (HttpStatusCodeException ex)
        {
            await HandleException(context, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            //Log
            logger.LogError(ex.ToString());

            await HandleException(context, 500, ex.Message);
        }
    }

    public async Task HandleException(HttpContext context, int code, string message)
    {
        context.Response.StatusCode = code;
        await context.Response.WriteAsJsonAsync(new
        {
            Code = code,
            Message = message
        });
    }
}
