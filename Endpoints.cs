using System.Text.RegularExpressions;

public class Endpoint
{
    public string Path { get; }
    public string Method { get; }
    public Func<HttpContext, string> Handler { get; }
    
    private readonly Regex _pathRegex;
    private readonly List<string> _parameterNames = new();

    public Endpoint(string path, string method, Func<HttpContext, string> handler)
    {
        Path = path;
        Method = method;
        Handler = handler;

        // Convert /users/{id} to ^/users/(?<id>[^/]+)$
        var pattern = "^" + Regex.Replace(path, "{([^}]+)}", m =>
        {
            var paramName = m.Groups[1].Value;
            _parameterNames.Add(paramName);
            return $"(?<{paramName}>[^/]+)";
        }) + "$";
        
        _pathRegex = new Regex(pattern, RegexOptions.IgnoreCase);
    }

    public bool Matches(RequestContext ctx)
        => _pathRegex.IsMatch(ctx.Path) &&
           ctx.Method!.Equals(Method, StringComparison.OrdinalIgnoreCase);

    public void ExtractRouteData(RequestContext ctx)
    {
        var match = _pathRegex.Match(ctx.Path);
        if (match.Success)
        {
            foreach (var name in _parameterNames)
            {
                ctx.RouteData[name] = match.Groups[name].Value;
            }
        }
    }
}