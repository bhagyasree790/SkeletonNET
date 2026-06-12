using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]

public abstract class HttpMethodAttribute : Attribute
{
    public string Path { get; }

    public string Method { get; }
    // Constructor used by derived attributes
    protected HttpMethodAttribute(string method, string path)
    {
        Method = method;
        Path = path;
    }
}

public sealed class HttpGetAttribute : HttpMethodAttribute
{
    // Calls the base constructor with HTTP method = GET
    public HttpGetAttribute(string path) : base("GET", path) { }
}

public sealed class HttpPostAttribute : HttpMethodAttribute
{
    // Calls the base constructor with HTTP method = POST
    public HttpPostAttribute(string path) : base("POST", path) { }
}

public sealed class HttpPutAttribute : HttpMethodAttribute
{
    // Calls the base constructor with HTTP method = PUT
    public HttpPutAttribute(string path) : base("PUT", path) { }
}

public sealed class HttpDeleteAttribute : HttpMethodAttribute
{
    // Calls the base constructor with HTTP method = DELETE
    public HttpDeleteAttribute(string path) : base("DELETE", path) { }
}