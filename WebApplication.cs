using System.Reflection;

class WebApplication(ServiceProvider services)
{

    public readonly ServiceProvider Services = services;

    private readonly Router _router = new();

    // Manually register a GET route
    public WebApplication MapGet(string pattern, Func<HttpContext, string> handler)
    {
        _router.MapGet(pattern, handler);  // Add route to router
        return this;                       // Return "this" to allow chaining
    }

    // Manually register a POST route
    public WebApplication MapPost(string pattern, Func<HttpContext, string> handler)
    {
        _router.MapPost(pattern, handler); // Add route to router
        return this;                       // Return "this" to allow chaining
    }

    // Automatically map controller methods as routes
    public WebApplication MapControllers()
    {
        // Get all controller classes discovered by ServiceCollection
        var controllerTypes = Services.GetControllerTypes();

        // Loop through each controller class
        foreach (var controller in controllerTypes)
        {
            // Get all public instance methods in the controller
            var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            foreach (var method in methods)
            {
                // Check if the method has an HTTP attribute (HttpGet, HttpPost, HttpPut, HttpDelete)
                var attr = method.GetCustomAttributes<HttpMethodAttribute>().FirstOrDefault();

                if (attr != null)
                {
                    _router.Map(attr.Method, attr.Path, ctx =>
                    {
                        // Resolve the controller from the request-scoped services
                        var instance = ctx.RequestServices.GetService(controller);

                        var methodParams = method.GetParameters();
                        object?[]? parameters = null;
                        
                        if (methodParams.Length > 0)
                        {
                            parameters = new object?[methodParams.Length];
                            for (int i = 0; i < methodParams.Length; i++)
                            {
                                var param = methodParams[i];
                                // Check if it's in RouteData
                                if (ctx.Request.RouteData.TryGetValue(param.Name ?? "", out var routeValue))
                                {
                                    parameters[i] = Convert.ChangeType(routeValue, param.ParameterType);
                                }
                                // Otherwise try Body if available
                                else if (!string.IsNullOrEmpty(ctx.Request.Body))
                                {
                                    try
                                    {
                                        var options = new System.Text.Json.JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        };
                                        parameters[i] = System.Text.Json.JsonSerializer.Deserialize(ctx.Request.Body, param.ParameterType, options);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"[JSON Error] Failed to deserialize body for parameter '{param.Name}' of type '{param.ParameterType.Name}'.");
                                        Console.WriteLine($"[JSON Error] Body content: {ctx.Request.Body}");
                                        Console.WriteLine($"[JSON Error] Exception: {ex.Message}");
                                        throw; // Re-throw to be caught by middleware
                                    }
                                }
                            }
                        }

                        // Invoke the method and get the result
                        var result = method.Invoke(instance, parameters);

                        // Return the result as a string
                        return result?.ToString() ?? "";
                    });
                }
            }
        }

        return this; // Allow method chaining
    }

    private readonly List<Func<RequestDelegate, RequestDelegate>> _middlewares = new();

    public WebApplication UseMiddleware<T>()
    {
        _middlewares.Add(next =>
        {
            return async context =>
                 {
                     // Instantiate the middleware and pass the 'next' delegate to its constructor
                     var middleware = (dynamic)Activator.CreateInstance(typeof(T), next)!;
                     await middleware.InvokeAsync(context);
                 };
        });
        return this;
    }

    private RequestDelegate BuildPipeline()
    {
        // The "Terminal" middleware: This is the very last step that calls the Router
        RequestDelegate pipeline = async context =>
        {
            var result = _router.Resolve(context);
            await context.Response.WriteAsync(result);
        };

        // Chain the middlewares in reverse order (Russian Doll pattern)
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            pipeline = _middlewares[i](pipeline);
        }

        return pipeline;
    }

    public async Task RunAsync(int port = 5005)
    {
        // Build the pipeline before starting
        var pipeline = BuildPipeline();
        var server = new TcpServer(port, pipeline, Services);
        await server.StartAsync();
    }

}


class WebApplicationBuilder
{
    // Create a ServiceCollection to hold controllers
    public ServiceCollection Services { get; } = new ServiceCollection();

    // Build a MiniWebApplication using the registered services
    public WebApplication Build()
    {
        var provider = Services.BuildServiceProvider();
        return new(provider);
    }
}

// Factory class to start building an app
static class WebApplicationFactory
{
    // Create a new builder
    public static WebApplicationBuilder CreateBuilder()
    {
        return new WebApplicationBuilder();
    }
}