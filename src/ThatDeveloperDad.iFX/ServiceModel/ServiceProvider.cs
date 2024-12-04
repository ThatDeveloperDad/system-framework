using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThatDeveloperDad.iFX.Behaviors;
using ThatDeveloperDad.iFX.ServiceModel.Taxonomy;

namespace ThatDeveloperDad.iFX.ServiceModel;


public abstract class ServiceProvider:IDisposable
{
    public Type ModuleType {get; private set;}
    public Type Contract { get; protected set; }
    public Type Implementation { get; protected set; }
    public ModuleSpecification Specification { get; protected set; }
    protected IServiceProvider Services { get; set; }
    protected ServiceLifetime Lifetime { get; set; }
    protected IOperationBehavior[] Behaviors { get; set; }
    
    public ServiceProvider(
        ModuleSpecification moduleSpec, 
        IServiceProvider globalUtilities,
        Type moduleContractType,
        IConfiguration configuration)
    {
        Specification = moduleSpec;
        Contract = moduleContractType;
        ModuleType = DeriveModuleType();
        Implementation = ParseImplementationType(moduleSpec);
        Lifetime = ParseServiceLifetime(moduleSpec);
        Services = ConfigureServices(moduleSpec, globalUtilities, configuration);
        Behaviors = ConfigureBehaviors(moduleSpec.Behaviors, globalUtilities);
    }

    public abstract object Acquire();

    protected abstract IServiceProvider ConfigureServices(
        ModuleSpecification moduleSpec, 
        IServiceProvider sharedServices,
        IConfiguration configuration);

    protected abstract IOperationBehavior[] ConfigureBehaviors(
        BehaviorSpec[] moduleBehaviors,
        IServiceProvider globalUtilities);

    public ServiceDescriptor AsServiceDescriptor()
    {
        ServiceDescriptor descriptor 
            = new(
                serviceType: Contract,
                implementationType: Implementation,
                lifetime: Lifetime);
        

        return descriptor;
    }

    public ServiceDescriptor AsAcquirer()
    {
        Func<IServiceProvider, object> svcFactory 
            = (sp) =>
            {
                var serviceProvider = sp.GetRequiredService(ModuleType);
                var service = ((ServiceProvider)serviceProvider).Acquire();
                
                return service;
            }; 
        
        ServiceDescriptor descriptor 
            = new(
                serviceType: Contract,
                factory: svcFactory,
                lifetime: Lifetime);
        
        return descriptor;
    }
    protected IServiceCollection RegisterModule(IServiceCollection services)
    {
        services.AddSingleton(ModuleType, this);
        services.Add(this.AsAcquirer());
        return services;
    }

    private Type ParseImplementationType(ModuleSpecification moduleSpec)
    {
        string assemblyName = moduleSpec.Implementation.Assembly;
        string contractName = moduleSpec.Contract;
        
        var assembly = Assembly.Load(assemblyName);
        if(assembly == null)
        {
            throw new Exception($"Could not load assembly {assemblyName}");
        }

        var assemblyTypes = assembly.GetTypes()
            .Where(at=> at.GetInterfaces().Any(ati=> ati == Contract)
                    && at.IsClass);
        var result = assemblyTypes.FirstOrDefault();

        if(result == null)
        {
            throw new Exception($"Could not find an implementation type for {contractName}");
        }

        return result;
    }

    private Type DeriveModuleType()
    {
        Type moduleType = typeof(ServiceProvider<>).MakeGenericType(Contract);
        return moduleType;
    }

    private ServiceLifetime ParseServiceLifetime(ModuleSpecification moduleSpec)
    {
        ServiceLifetime result = moduleSpec.Lifetime switch
                {
                    "Singleton" => ServiceLifetime.Singleton,
                    "Scoped" => ServiceLifetime.Scoped,
                    "Transient" => ServiceLifetime.Transient,
                    _ => throw new NotSupportedException($"Lifetime {moduleSpec.Lifetime} is not supported.")
                };

        return result;
    }

#region IDisposable Support
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
#endregion
}

public class ServiceProvider<TServiceContract>
    :ServiceProvider
    where TServiceContract:ISystemComponent
{
    private ILogger? _logger;
    
    public ServiceProvider(
            ModuleSpecification moduleSpec, 
            IServiceProvider sharedServices,
            IConfiguration configuration,
            ILoggerFactory? logFactory)
        :base(moduleSpec, sharedServices, typeof(TServiceContract), configuration)
    {
        _logger = logFactory?.CreateLogger($"ModuleProvider: {Contract.Name}");
        _logger?.LogInformation($"Created ModuleProvider for {moduleSpec.Contract}");
    }

    /// <summary>
    /// Used to acquire an initialized instance of the service provided
    /// by the module.
    /// 
    /// The instance is not returned directly, but rather wrapped in a transparent proxy.
    /// By default, that Proxy is a PassthroughProxy.
    /// </summary>
    /// <returns></returns>
    public override object Acquire()
    {
        object? concreteImplementation = null;
        _logger?.LogInformation($"Acquiring {Contract.Name} service.");
        if(Lifetime == ServiceLifetime.Singleton)
        {
            concreteImplementation = MySingleton;
        }
        else
        {
            concreteImplementation = GetModuleService();
        }

        if(concreteImplementation == null)
        {
            throw new Exception($"Could not acquire an instance of the requested {Contract.Name} service.");
        }

        PassthroughProxy? proxy = PassthroughProxy.Build(Contract, Implementation, concreteImplementation);

        if(proxy == null)
        {
            throw new Exception($"Could not create a proxy for {Contract.Name}:{Implementation.Name}");
        }
        
        if(Behaviors.Length > 0)
        {
            foreach(var behavior in Behaviors)
            {
                proxy.AddBehavior(behavior);
            }
        }

        _logger?.LogInformation($"Serving proxy for {Contract.Name}.");
        return proxy;
    }

    private TServiceContract? _singletonInstance;
    private TServiceContract? MySingleton
    {
        get
        {
            if(_singletonInstance == null)
            {
                //Instantiate Singleton.
                _logger?.LogInformation($"Lazy Creating Singleton {Implementation.Name}:{Contract.Name}");
                _singletonInstance = (TServiceContract?)GetModuleService();
            }
            return _singletonInstance;
        }
        set
        {
            if(Lifetime != ServiceLifetime.Singleton)
            {
                throw new InvalidOperationException("Cannot set a singleton instance on a non-singleton service.");
            }
            _singletonInstance = value;
        }
    }

    private object? GetServiceFrom(
        Type implementationType, 
        IServiceProvider serviceStore)
    {
        object? result = null;
        var ctors = implementationType.GetConstructors()
            .OrderByDescending(c=> c.GetParameters().Length)
            .ToArray();

        if(ctors.Length == 0)
        {
            result = Activator.CreateInstance(implementationType);
        }
        else
        {
            foreach(var ctor in ctors)
            {
                var parameters = ctor.GetParameters();
                object[] args = new object[parameters.Length];
                for(int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    // the paramType might be a generic.  In that case, we'll need to dig deeper.
                    if(paramType.IsGenericType)
                    {
                        paramType = paramType.GenericTypeArguments[0];
                    }
                    args[i] = serviceStore.GetRequiredService(paramType);
                }
                result = Activator.CreateInstance(implementationType, args);
                if(result != null)
                {
                    break;
                }
            }
        }
        return result;
    }

    private object? GetModuleService()
    {
        object? module = GetServiceFrom(Implementation, Services);


        _logger?.LogInformation($"Created {Implementation.Name}:{Contract.Name} successfully.");
        return module;
    }

    protected override IServiceProvider ConfigureServices(
        ModuleSpecification moduleSpec, 
        IServiceProvider sharedServices,
        IConfiguration configuration)
    {
        IServiceCollection moduleServices = new ServiceCollection();
        ILoggerFactory builderLog = sharedServices.GetRequiredService<ILoggerFactory>();
        var globalBehaviors = Specification.Behaviors.Where(b=> b.IsGlobal).ToArray();
        
        moduleServices = ConfigureSettings(
            moduleSpec.Implementation, 
            moduleServices,
            configuration);

        // Add module Dependencies to the serviceCollection.
        foreach(var dependency in moduleSpec.Dependencies)
        {
            string contractName = dependency.Contract;
            string contractAssembly = dependency.ContractAssembly;
            if(dependency.Implementation.Source == "Shared")
            {
                var sharedType = TypeHelper.LoadType(contractName, contractAssembly);
                var sharedService = sharedServices.GetRequiredService(sharedType);
                moduleServices.AddSingleton(sharedType, sharedService);
            }
            else if(dependency.Implementation.Source == "Module")
            {
                Type? dependencyType = TypeHelper.LoadType(
                    dependency.Contract, 
                    dependency.ContractAssembly);
                if(TypeHelper.ReferenceIsValid(Contract, dependencyType) == false)
                {
                    var contractArchetype = TypeHelper.GetArchetype(Contract)?.Name??"NonSystemComponent";
                    var dependencyArchetype = TypeHelper.GetArchetype(dependencyType)?.Name??"NonSystemComponent";
                    
                    string error = $"{contractArchetype} Modules like {Contract.Name} may not depend on {dependencyArchetype} Modules such as {dependencyType.Name}";
                    
                    throw new InvalidOperationException(error);
                }

                dependency.AddGlobalBehaviors(globalBehaviors);
                var provider = ServiceBuilder.BuildService(dependency, sharedServices, configuration, _logger);
                moduleServices.AddSingleton(provider.ModuleType, provider);
                moduleServices.Add(provider.AsAcquirer());
            }
            else
            {
                throw new NotSupportedException($"Implementation source {dependency.Implementation.Source} is not supported.");
            }
        }
        
        // Register this with the private service collection so it can be
        // added via the Acquire method.
        moduleServices = RegisterModule(moduleServices);

        return moduleServices.BuildServiceProvider();
    }

    private IServiceCollection ConfigureSettings(
        ImplementationSpec implementationSpec,
        IServiceCollection services,
        IConfiguration configuration)
    {
        if(implementationSpec.Source == "Shared")
        {
            return services;
        }
        else if(implementationSpec.ServiceOptions == null || implementationSpec.ServiceOptions.GetChildren().Any() == false)
        {
            return services;
        }
        
        // Get the IServiceOptions type for the module.
        // There will be exactly one of these per service contract per implementation assembly.
        var optionsType = TypeHelper.LoadImplementingType(nameof(IServiceOptions), implementationSpec.Assembly);
        var optionsInstance = Activator.CreateInstance(optionsType);
        
        // We're not going to map the options via Binding, because we might need to
        // do something different, if a Setting has some "Look here instead" values.
        if(optionsInstance != null)
        {
            optionsInstance = PopulateProperties(optionsInstance, 
                implementationSpec.ServiceOptions,
                configuration);

            services.AddSingleton(optionsType, optionsInstance!);
        }
        else
        {
            _logger?.LogWarning($"Could not create IServiceOptions for {Implementation.Name}.  Settings are specified, but the Options object was not initialized.");
        }

        return services;
    }

    private object? PopulateProperties(object? optionsInstance, 
        IConfigurationSection serviceSettings,
        IConfiguration configuration)
    {
        if(optionsInstance == null)
        {
            return null;
        }

        var properties = optionsInstance.GetType().GetProperties();
        foreach(var prop in properties)
        {
            string externalSettingFlag = "EXT:";
            var settingValue = serviceSettings[prop.Name];
            
            if(settingValue != null)
            {
                // This part handles the "Flagged" settings from 
                // appsettings.json.
                // When we need to keep more volatile, or more sensitive
                // settings, we can IDENTIFY them in appSettings,
                // but we need to retrieve them from the "ambient"
                // configuration.
                // So, we'll use "EXT:" (for "Extenrnal") followed by
                // the ConfigurationPath to the actual Value that's
                // loaded into IConfiguration from wherever it may be.
                if(settingValue.StartsWith(externalSettingFlag))
                {
                    var externalKeyName = settingValue.Substring(externalSettingFlag.Length);
                    settingValue = configuration[externalKeyName];
                    if(settingValue == null)
                    {
                        _logger?.LogWarning($"Could not find external setting {externalKeyName} for {prop.Name} in {Implementation.Name}");
                        continue;
                    }
                }

                // Config values are usually "Stringly-Typed".
                // If the Options property type is the same as the 
                // retrieved settingValue, we'll do a straignt assignment.
                if(prop.PropertyType == settingValue.GetType())
                {
                    prop.SetValue(optionsInstance, settingValue);
                    continue;
                }
                
                // Otherwise, we need to convert the setting Value to the
                // target Option Type.
                var propValue = Convert.ChangeType(settingValue, prop.PropertyType);
                prop.SetValue(optionsInstance, propValue);
            }
        }
        _logger?.LogInformation($"Populated settings for {optionsInstance?.GetType().Name}");
        return optionsInstance;
    }

    protected override IOperationBehavior[] ConfigureBehaviors(
        BehaviorSpec[] moduleBehaviors,
        IServiceProvider utilityProvider)
    {
        List<IOperationBehavior> behaviors = new();
        foreach(var behaviorName in moduleBehaviors)
        {
            var behaviorType = TypeHelper.LoadType(behaviorName.Name, behaviorName.AssemblyName);
            if(behaviorType == null)
            {
                _logger?.LogWarning($"Could not load behavior {behaviorName.Name} from {behaviorName.AssemblyName}");
                continue;
            }
            var behavior = (IOperationBehavior?)GetServiceFrom(behaviorType, utilityProvider);

            if(behavior == null)
            {
                _logger?.LogWarning($"Could not create behavior {behaviorName.Name} from {behaviorName.AssemblyName}");
                continue;
            }
            
            string behaviorLabel = $":{behaviorName.Name} for {Contract.Name}:{Implementation.Name}";
            behavior.SetBehaviorLabel(behaviorLabel);
            behaviors.Add(behavior);
        }

        return behaviors.ToArray();
    }

}
