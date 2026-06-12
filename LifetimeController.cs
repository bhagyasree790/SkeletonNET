using System.Text;

public class LifetimeController(
    ITransientService transientService,
    IScopedService scopedService,
    ISingletonService singletonService,
    IInternalService internalService)
{
    [HttpGet("/lifetimes")]
    public string GetLifetimes()
    {
        var sb = new StringBuilder();
        sb.AppendLine("DI Lifetime Comparison:");
        sb.AppendLine("-----------------------");
        
        sb.AppendLine($"Transient (Controller): {transientService.Id}");
        sb.AppendLine($"Transient (Internal):   {internalService.TransientService.Id}");
        sb.AppendLine(transientService.Id == internalService.TransientService.Id 
            ? "=> FAIL: Transient should be different!" 
            : "=> SUCCESS: Transient are different.");
        
        sb.AppendLine();
        sb.AppendLine($"Scoped (Controller):    {scopedService.Id}");
        sb.AppendLine($"Scoped (Internal):      {internalService.ScopedService.Id}");
        sb.AppendLine(scopedService.Id == internalService.ScopedService.Id 
            ? "=> SUCCESS: Scoped are the same." 
            : "=> FAIL: Scoped should be the same!");

        sb.AppendLine();
        sb.AppendLine($"Singleton (Controller): {singletonService.Id}");
        sb.AppendLine($"Singleton (Internal):   {internalService.SingletonService.Id}");
        sb.AppendLine(singletonService.Id == internalService.SingletonService.Id 
            ? "=> SUCCESS: Singleton are the same." 
            : "=> FAIL: Singleton should be the same!");

        sb.AppendLine();
        sb.AppendLine("Note: Refresh the page to see Scoped and Transient change, but Singleton stay the same.");
        
        return sb.ToString();
    }
}