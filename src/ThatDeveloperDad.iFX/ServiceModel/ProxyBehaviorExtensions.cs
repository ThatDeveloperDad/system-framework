using System;
using System.ComponentModel.Design;
using ThatDeveloperDad.iFX.Behaviors;
using ThatDeveloperDad.iFX.ServiceModel.Taxonomy;

namespace ThatDeveloperDad.iFX.ServiceModel;

internal static class ProxyBehaviorExtensions
{
    public static PassthroughProxy<TContract, TService> AddBehavior<TContract, TService>
        (
            this PassthroughProxy<TContract, TService> proxy, 
            IOperationBehavior? behavior, 
            string? methodName = null
        )
        where TContract: ISystemComponent
        where TService: class, TContract
    {
        if(behavior == null)
        {
            return proxy;
        }

        proxy.AddBehavior(behavior, methodName);
        return proxy;
    }

}
