using System.Reflection;

public class ServiceCollection
{

    private readonly List<ServiceDescriptor> _services = [];

    public void AddTransient<TService>()
    {
        var typeOfService = typeof(TService);
        AddTransient(typeOfService, typeOfService);
    }
    
    public void AddTransient<TService, TImplementation>()
    {
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient));
    }

    public void AddTransient(Type serviceType, Type implementationType)
    {
        _services.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient));
    }

    public void AddSingleton<TService, TImplementation>()
    {
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton));
    }

    public void AddScoped<TService, TImplementation>()
    {
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped));
    }
    public void AddControllers()
    {
        var controllers = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Controller"));

        foreach(var ctrl in controllers)
        {
            AddTransient(ctrl, ctrl);
        }
    }

    public ServiceProvider BuildServiceProvider() => new(_services);
}