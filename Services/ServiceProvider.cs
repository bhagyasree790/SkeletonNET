
//Custom service provider to resolve dependencies and controllers
public class ServiceProvider : IDisposable
{
    // List of all registered services
    private readonly List<ServiceDescriptor> _services;

    // Cache for Singleton instances
    private readonly Dictionary<Type, object> _singletonInstances;

    // Cache for Scoped instances
    private readonly Dictionary<Type, object> _scopedInstances = [];

    public ServiceProvider(List<ServiceDescriptor> services, Dictionary<Type, object>? singletons = null)
    {
        _services = services;
        _singletonInstances = singletons ?? [];
    }

    public T GetService<T>() => (T)GetService(typeof(T));

    public object GetService(Type serviceType)
    {
        // Find the registered service descriptor
        var descriptor = _services.FirstOrDefault(x => x.ServiceType == serviceType)
            ?? throw new Exception($"Service {serviceType.Name} isn't registered");

        // Resolve service based on its lifetime
        return descriptor.LifeTime switch
        {
            ServiceLifetime.Transient => CreateInstance(descriptor.ImplementationType),
            ServiceLifetime.Singleton => GetFromCache(_singletonInstances, descriptor),
            ServiceLifetime.Scoped => GetFromCache(_scopedInstances, descriptor),
            _ => throw new Exception("Unknown lifetime")
        };
    }

    private object GetFromCache(Dictionary<Type, object> cache, ServiceDescriptor descriptor)
    {
        if (cache.TryGetValue(descriptor.ServiceType, out var instance))
        {
            return instance;
        }

        var newInstance = CreateInstance(descriptor.ImplementationType);
        cache[descriptor.ServiceType] = newInstance;
        return newInstance;
    }

    public ServiceProvider CreateScope()
    {
        // Create a new ServiceProvider for the scope. 
        // It shares the same service definitions and Singletons, but starts with its own empty scoped cache.
        return new ServiceProvider(_services, _singletonInstances);
    }

    private object CreateInstance(Type implType)
    {
        // Get the first constructor of the type
        var ctors = implType.GetConstructors();
        if (ctors.Length == 0) return Activator.CreateInstance(implType)!;

        var firstConstructor = ctors.First();

        // Resolve all constructor parameters (dependencies)
        var deps = firstConstructor.GetParameters()
            .Select(p => GetService(p.ParameterType))
            .ToArray();

        // Create instance using resolved dependencies
        return Activator.CreateInstance(implType, deps)!;
    }


    public List<Type> GetControllerTypes()
    {
        return _services
            .Where(d => d.ServiceType.Name.EndsWith("Controller"))
            .Select(d => d.ServiceType)
            .ToList();
    }

    public void Dispose()
    {
        _scopedInstances.Clear();
    }
}