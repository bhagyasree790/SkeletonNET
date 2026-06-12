using System;
using System.Threading.Tasks;

public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;        
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[Middleware Error] {ex.Message}");

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                Message = "An error occured: " + ex.Message
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}