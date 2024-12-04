using System;
using System.Collections.Concurrent;
using System.Reflection;
using ThatDeveloperDad.iFX.Behaviors;
using ThatDeveloperDad.iFX.ServiceModel.Taxonomy;

namespace ThatDeveloperDad.iFX.ServiceModel;

public abstract class PassthroughProxy
    :DispatchProxy
{
    protected ConcurrentDictionary<string, List<IOperationBehavior>> _methodBehaviors
        = new();
    protected List<IOperationBehavior> _globalBehaviors = new();

    internal void AddBehavior(IOperationBehavior behavior, string? methodName = null)
    {
        if(methodName == null)
        {
            _globalBehaviors.Add(behavior);
            return;
        }

        if(!_methodBehaviors.ContainsKey(methodName))
        {
            _methodBehaviors[methodName] = new List<IOperationBehavior>();
        }

        _methodBehaviors[methodName].Add(behavior);
    }


    public static PassthroughProxy? Build(Type contractType, Type serviceType, object serviceInstance)
    {
        var proxyType = typeof(PassthroughProxy<,>).MakeGenericType(contractType, serviceType);
        
        //Because the Proxy is really only useful once it's been
        // materialized around the Contract and Implementation, 
        // we use a Default constructor to simplify instantiation.
        var proxyInstance = Activator.CreateInstance(proxyType);

        // Then, we get the actual "Typed" proxy by calling the CreateProxy 
        // method to configure our instance as a TypedProxy.
        var configureMethod = proxyType.GetMethod("CreateProxy");
        var proxy = configureMethod!.Invoke(proxyInstance, new object[]{serviceInstance});

        // And we can return it as an un-typed PassthroughProxy.
        return proxy as PassthroughProxy;
    }
}

internal class PassthroughProxy<TContract, TService>
    : PassthroughProxy
    where TContract: ISystemComponent
    where TService: class, TContract
{
    private TService? _serviceInstance;

    public TContract CreateProxy(TService service)
    {
        object proxy = Create<TContract, PassthroughProxy<TContract, TService>>();
        ((PassthroughProxy<TContract, TService>)proxy).SetServiceInstance(service);

        foreach(var behavior in _globalBehaviors)
        {
            ((PassthroughProxy<TContract, TService>)proxy).AddBehavior(behavior);
        }

        foreach(var key in _methodBehaviors.Keys)
        {
            foreach(var behavior in _methodBehaviors[key])
            {
                ((PassthroughProxy<TContract, TService>)proxy).AddBehavior(behavior, key);
            }
        }

        return (TContract)proxy;
    }

    /// <summary>
    /// Runs any Pre-Call Behaviors that have been injected to the proxy.
    /// Invokes the requested method on the service instance.
    /// Runs any Post-Call Behaviors that have been injected to the proxy.
    /// Returns the result of the call.
    /// </summary>
    /// <param name="targetMethod"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if(targetMethod == null)
        {
            throw new InvalidOperationException("You can't invoke a method without telling me which method to invoke.");
        }

        MethodContext context = new()
        {
            MethodName = targetMethod!.Name,
            Parameters = args
        };

        // Execute the Service-Global Pre-Method behaviors.
        foreach(IOperationBehavior behavior in _globalBehaviors)
        {
            behavior.OnMethodEntry(context);
        }

        // Execute the Method-Specific Pre-Method behaviors.
        if(_methodBehaviors.ContainsKey(targetMethod.Name))
        {
            foreach(IOperationBehavior behavior in _methodBehaviors[targetMethod.Name])
            {
                behavior.OnMethodEntry(context);
            }
        }

        var result = targetMethod.Invoke(_serviceInstance, args);

        if(result is Task taskResult)
        {
            taskResult.GetAwaiter().GetResult();
            context.ReturnValue = taskResult.GetType().GetProperty("Result")?.GetValue(taskResult);
        }
        else
        {
            context.ReturnValue = result;
        }

        // Execute the Method-Specific Post-Method behaviors.
        if(_methodBehaviors.ContainsKey(targetMethod.Name))
        {
            foreach(IOperationBehavior behavior in _methodBehaviors[targetMethod.Name])
            {
                behavior.OnMethodExit(context);
            }
        }

        // Execute the Service-Global Post-Method behaviors.
        foreach(IOperationBehavior behavior in _globalBehaviors)
        {
            behavior.OnMethodExit(context);
        }

        return result;
    }

    protected void SetServiceInstance(TService service)
    {
        _serviceInstance = service;
    }
}

