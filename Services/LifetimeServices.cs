using System;

public interface ITransientService { Guid Id { get; } }
public interface IScopedService { Guid Id { get; } }
public interface ISingletonService { Guid Id { get; } }

public class TransientService : ITransientService { public Guid Id { get; } = Guid.NewGuid(); }
public class ScopedService : IScopedService { public Guid Id { get; } = Guid.NewGuid(); }
public class SingletonService : ISingletonService { public Guid Id { get; } = Guid.NewGuid(); }

public interface IInternalService
{
    ITransientService TransientService { get; }
    IScopedService ScopedService { get; }
    ISingletonService SingletonService { get; }
}

public class InternalService(ITransientService transientService, IScopedService scopedService, ISingletonService singletonService) : IInternalService
{
    public ITransientService TransientService { get; } = transientService;
    public IScopedService ScopedService { get; } = scopedService;
    public ISingletonService SingletonService { get; } = singletonService;
}
