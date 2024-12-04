using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ThatDeveloperDad.iFX.ServiceModel;

public class ServiceBuilder
{

    public static ServiceProvider BuildService(
        ModuleSpecification moduleSpec, 
        IServiceProvider sharedServices,
        IConfiguration configuration,
        ILogger? logger)
    {
        logger?.LogInformation($"Building Module: {moduleSpec.Contract}");

        Type moduleContractType = TypeHelper.LoadType(moduleSpec.Contract, moduleSpec.ContractAssembly);
        if(moduleContractType == null)
        {
            throw new Exception($"Unknown contract type: {moduleSpec.Contract}.  Cannot build module.");
        }
        
        Type moduleProviderType = typeof(ServiceProvider<>).MakeGenericType(moduleContractType);
        
        var moduleProvider = CreateProvider(moduleProviderType, moduleSpec, sharedServices, configuration);

        return moduleProvider;
    }
    
    private static ServiceProvider CreateProvider(
        Type moduleProviderType, 
        ModuleSpecification moduleSpec, 
        IServiceProvider sharedServices,
        IConfiguration configuration)
    {
        var constructorTypeParams = new Type[] 
            { 
                typeof(ModuleSpecification), 
                typeof(IServiceProvider), 
                typeof(IConfiguration), 
                typeof(ILoggerFactory)
            };

        ILoggerFactory? loggerFactory = sharedServices.GetService<ILoggerFactory>();

        var constructorParams = new object[] 
            { 
                moduleSpec, 
                sharedServices, 
                configuration,
                loggerFactory!  // The constructor will accept a null loggerFactory.
            };

        ConstructorInfo? constructor = moduleProviderType.GetConstructor(constructorTypeParams);

        if(constructor == null)
        {
            throw new Exception($"Could not find a constructor for {moduleProviderType.Name}");
        }

        var moduleProviderInstance = constructor.Invoke(constructorParams) as ServiceProvider;

        if(moduleProviderInstance == null)
        {
            throw new Exception($"Could not create an instance of {moduleProviderType.Name}");
        }

        return moduleProviderInstance;
    }
}

