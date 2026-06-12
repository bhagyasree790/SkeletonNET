// Router class responsible for managing route registrations and request resolution
public class Router
{
    private readonly List<Endpoint> _endpoints = [];

    public void Map(string method, string path, Func<HttpContext, string> handler)
    {
        _endpoints.Add(new Endpoint(path, method, handler));
    }

    public void MapGet(string path, Func<HttpContext, string> handler) => Map("GET", path, handler);
    public void MapPost(string path, Func<HttpContext, string> handler) => Map("POST", path, handler);
    public void MapPut(string path, Func<HttpContext, string> handler) => Map("PUT", path, handler);
    public void MapDelete(string path, Func<HttpContext, string> handler) => Map("DELETE", path, handler);

    public string Resolve(HttpContext context)
    {
        var endpoint = _endpoints.FirstOrDefault(ep => ep.Matches(context.Request));

        if (endpoint != null)
        {
            endpoint.ExtractRouteData(context.Request);
            return endpoint.Handler(context);
        }
        
        return "404 Not Found";
    }
}