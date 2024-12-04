using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThatDeveloperDad.iFX.ServiceModel;
using ThatDeveloperDad.iFX.ServiceModel.Taxonomy;

namespace ThatDeveloperDad.iFX;

public static class AppArchitectureExtensions
{
    public static IServiceCollection AddAppArchitecture(
        this IServiceCollection services, 
        IConfiguration appConfig,
        IServiceProvider sharedServices,
        ILogger? appLogger = null)
    {
        appLogger?.LogInformation("Building Application Modules Started.");
        ILoggerFactory builderLog = sharedServices.GetRequiredService<ILoggerFactory>();
        
        // First, we'll load in the application's Architectecture Configuration
        // and get the manifest of Global Behaviors and Modules.
        var section = appConfig.GetSection("Architecture");
        
        var appModulesConfig = section.GetSection("Modules");
        var modules = appModulesConfig.Get<ModuleSpecification[]>();

        var globalBehaviorsConfig = section.GetSection("GlobalBehaviors");
        var globalBehaviors = globalBehaviorsConfig.Get<BehaviorSpec[]>();
        
        // Now, we build the TOP LEVEL modules, and add them to the 
        // services collection used by the application.
        if(modules !=null)
        {
            foreach (var module in modules)
            {
                ValidateModuleArchetype(module, appLogger);

                // Transfer the global behaviors to the ModuleSpecification.
                if(globalBehaviors?.Length>0)
                {
                    module.AddGlobalBehaviors(globalBehaviors);
                }
                
                // Our services manifest for the system as individual "providers"
                // Rather than adding the <IService, Service> pair that we
                // see most commonly.
                //
                // This allows us to isololate a service's dependencies
                // to that specific service, preventing their accidental use
                // in other application modules.
                var provider = ServiceBuilder.BuildService(module, sharedServices, appConfig, appLogger);

                // THese Providers are added as Singletons so that the Factories 
                // are cached when the application bootstraps.
                services.AddSingleton(provider.ModuleType, provider);

                // Finally, the "Service" that the provider manages for us
                // is exposed for DI directly.
                // the AsAcquirer() method provides the 
                // Contract exposed by the Provider, the Lifetime,
                // and a Factory method that supplies the materialized
                // Contract implementation.
                services.Add(provider.AsAcquirer());
            }

            appLogger?.LogInformation("Application Modules Registered.");
            
        }
        else
        {
            appLogger?.LogWarning("No Modules Found in Configuration.");
            throw new Exception("No Modules Found in Configuration.");
        }
        
        return services;
    }

    /// <summary>
    /// Validates that the Top-Level Modules are of the appropriate Archetype.
    /// </summary>
    /// <param name="module">The Specification for the Module.</param>
    /// <param name="logger">The application Logger to be used if we need to log a problem.</param>
    private static void ValidateModuleArchetype(
        ModuleSpecification module, 
        ILogger? logger)
    {
        
        Type contract = TypeHelper.LoadType(module.Contract, module.ContractAssembly);
        string contractArchetype = 
            TypeHelper.GetArchetype(contract)?.Name??"NonSystemComponent"
            .Replace("I", string.Empty)
            .Replace("Service", string.Empty);


        // Top-Level modules may be one of the following:
        // IManagerService, IClientService.
        if(TypeHelper.ReferenceIsValid(typeof(IApplicationContainer), contract) == false)
        {
            logger?.LogError($"Module {module.Contract} is a {contractArchetype} and cannot be added as a direct dependency of the Application Container.");
            throw new Exception($"Module {module.Contract} is not a valid Archetype for the current dependency context.");
        }
        
    }

}
