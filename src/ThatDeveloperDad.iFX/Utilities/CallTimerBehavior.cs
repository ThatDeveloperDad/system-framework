
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThatDeveloperDad.iFX.Behaviors;
using ThatDeveloperDad.iFX.ServiceModel.Taxonomy;

namespace ThatDeveloperDad.iFX.Utilities;
/// <summary>
/// Specifies behavior that will execute before and/or after a method is invoked.
/// </summary>
public class CallTimerBehavior
    : IOperationBehavior
    
{
    private ILogger? _logger;
    private ILoggerFactory _logFactory;

    public CallTimerBehavior(ILoggerFactory logFactory)
    {
        _logFactory = logFactory;
        var logger = logFactory.CreateLogger($"CallTimer");

        _logger = logger;
    }

    public void SetBehaviorLabel(string label)
    {
        // Replaces the default logger with a new logger that has the provided label.
        _logger = _logFactory.CreateLogger(label);
    }

    private long? _methodStartedAt;

    public Task OnMethodEntryAsync(MethodContext context)
    {
        if(_logger == null)
        {
            return Task.CompletedTask;
        }
        _methodStartedAt = DateTime.Now.Ticks;
        _logger.LogInformation($"Entering {context.MethodName} with parameters {context.Parameters}");
        return Task.CompletedTask;
    }

    public Task OnMethodExitAsync(MethodContext context)
    {
        if(_logger == null)
        {
            return Task.CompletedTask;
        }
        
        var methodDuration = DateTime.Now.Ticks - _methodStartedAt;
        TimeSpan execTime = new TimeSpan(methodDuration??0);
        _logger.LogInformation($"Exiting {context.MethodName} with parameters {context.Parameters} in {execTime.Milliseconds} ms");
        return Task.CompletedTask;
    }
}
