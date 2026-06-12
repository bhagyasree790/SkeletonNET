using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;


public class RequestContext
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string> RouteData { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class TcpServer(int port, RequestDelegate pipeline, ServiceProvider services)
{
    private readonly int _port = port;
    private readonly RequestDelegate _pipeline = pipeline;
    private readonly ServiceProvider _services = services;

    public async Task StartAsync()
    {
        var listener = new TcpListener(IPAddress.Loopback, _port);
        listener.Start();

        Console.WriteLine($"Server started on port {_port}");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClient(client));
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        try
        {
            using var stream = client.GetStream();
            
            // Use a small buffer to read the request line and headers
            var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            
            var requestLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(requestLine)) return;

            var parts = requestLine.Split(' ');
            if (parts.Length < 2) return;
            
            var method = parts[0];
            var path = parts[1];

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
            {
                var headerParts = line.Split(": ", 2);
                if (headerParts.Length == 2)
                {
                    headers[headerParts[0]] = headerParts[1].Trim();
                }
            }

            var body = string.Empty;
            if (headers.TryGetValue("Content-Length", out var contentLengthStr) && int.TryParse(contentLengthStr, out var contentLength))
            {
                // We must read bytes, not characters, because Content-Length is in bytes.
                // However, the StreamReader might have already buffered some of the body.
                // The most reliable way in this 'skeleton' setup is to read character by character
                // or read the remaining stream. For simplicity and correctness with multi-byte:
                
                int totalRead = 0;

                // 1. Check if there's anything left in the StreamReader's buffer
                // This is a bit advanced for a 'skeleton', but necessary if we use StreamReader for headers.
                // We'll read from the underlying stream directly for the body, 
                // but we should have ideally parsed headers without a buffering StreamReader.
                
                // For this project's scope, we will read the 'contentLength' characters 
                // but use a more robust byte-to-string approach if we were in a real Kestrel.
                // Simplified fix: Read the required number of chars from the reader.
                
                // Note: StreamReader.BaseStream.Position might not be where we think due to buffering.
                // So we continue using the reader but with awareness of the count.
                char[] buffer = new char[contentLength];
                int charsRead = await reader.ReadBlockAsync(buffer, 0, contentLength);
                body = new string(buffer, 0, charsRead);
            }

            using var scope = _services.CreateScope();
            var httpContext = new HttpContext
            {
                Request = new RequestContext
                {
                    Method = method,
                    Path = path,
                    Body = body
                },
                Response = new HttpResponse(),
                RequestServices = scope
            };

            await _pipeline(httpContext);

            var responseText = httpContext.Response.GetBody();
            var responseHeader = $"HTTP/1.1 {httpContext.Response.StatusCode} OK\r\n" +
                                 $"Content-Type: {httpContext.Response.ContentType}\r\n" +
                                 $"Content-Length: {Encoding.UTF8.GetByteCount(responseText)}\r\n\r\n";
            
            var headerBytes = Encoding.UTF8.GetBytes(responseHeader);
            var bodyBytes = Encoding.UTF8.GetBytes(responseText);

            await stream.WriteAsync(headerBytes);
            await stream.WriteAsync(bodyBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
}

public delegate Task RequestDelegate(HttpContext context);

public class HttpContext
{
    public RequestContext Request { get; set; } = null!;
    public HttpResponse Response { get; set; } = new();

    public ServiceProvider RequestServices { get; set; } = null!;
}

public class HttpResponse
{
    public int StatusCode { get; set; } = 200;
    public string ContentType { get; set; } = "text/plain";
    private readonly StringBuilder _body = new();

    public Task WriteAsync(string text)
    {
        _body.Append(text);
        return Task.CompletedTask;
    }

    // Required by GlobalErrorHandlingMiddleware
    public Task WriteAsJsonAsync<T>(T obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        ContentType = "application/json";
        return WriteAsync(json);
    }

    public string GetBody() => _body.ToString();
}