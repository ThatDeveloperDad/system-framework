[Table of Contents](./_ToC.md)  
# How do I use this?
This path works nicely in a .Net development flow.  It likely maps just as well to the Java ecosystem.  I can't speak to other languages or application styles.  The concepts might fit, but the terminology is likely very different.  

1. Drive out the "Static" design.  
- Go through the Volatility Decomposition exercises until you've identified WHAT components your system will have.
2. Choose ONE concrete use case.  Use that to help discover the necessary interaction patterns, behaviors, and Data Exchange types throughout your system.
- You'll use that selected use case as a "Vertical Slice" to prove and refine your Static Design.
3. Create the solution's folder and project structure for the Static Design parts required for the selected Vertical Slice.
4. Obtain the iFX code.
- Fork or download this code repository, and change the Project Name and namespace declarations to match your organization.  Add it to your solution, and keep it as a Project Reference for now, until you decide to adopt and adapt it to your company's needs.
5. Add the public Interfaces and Data classes to the appropriate projects.
6. Add a project reference from each of your application's projects to [YourCompany].iFX
- Use the "Marker" interfaces declared in ThatDeveloperDad.iFX.ServiceModel.Taxonomy to apply the component taxonomy to your system's components.
- This is shown in the [Component Classification](#component-classification) section of this document.
7. Set up the architecture configuration in the "Application Host" project(s).  This is your Console App, Web App, whatever you roll with.
- This is shown in the [Application Architecture Configuration](#application-architecture-configuration) section.  It's actually pretty easy once you're used to it.
8. Set up `public static void Main(object[] args)` in `Program.cs`
- This is shown in the [Set up Program.cs](#set-up-programcs) section.

## Component Classification  
**Before**
```csharp
namespace Company.Project.Managers.CustomerService;

public interface ICustomerProfileManager
{
    CustomerProfile LoadProfile(string id);

    IEnumerable<CustomerFilterResult> FilterCustomers(CustomerFilter[] filters);

    CustomerProfile SaveProfile(CustomerProfile profile);
}
```

**After**  
```csharp
using Company.iFX.ServiceModel.Taxonomy

namespace Company.Project.Managers.CustomerService;

public interface ICustomerProfileManager : IManagerService
{
    CustomerProfile LoadProfile(string id);

    IEnumerable<CustomerFilterResult> FilterCustomers(CustomerFilter[] filters);

    CustomerProfile SaveProfile(CustomerProfile profile);
}
```
As you can see, the only difference is the using statement, and that the interface inherits IManagerService.  

There's a marker interface for each archetype in the taxonomy.  Decorate your component contracts with those accordingly.

## Application Architecture Configuration
For each EXECUTABLE that your system will expose, you will:

1. Add references to each of the projects that define the contracts.  
    1a. (If different,) reference to the project that contains the IMPLEMENTATION of a set of contracts.
2. Use configuration to compose the parts you've defined in your static architecture into your running application.

This method uses `appsettings.json` to define the composition of your application, and to drive the build-out of the nested ServiceCollections we've enabled.

Refer to the testConsole project to see how it all comes together.

### Required Configuration Section:  "Architecture"

Within your `appSettings.json` file, add a top-level section called "Architecture"  
  
``` json
{
    "Logging":{...omitted for brevity...},
    "Architecture": {
        "GlobalBehaviors": [
            {"Name": "CallTimerBehavior", "Assembly": "YourCompany.iFX"},
            ... other global behaviors that you've defined ...,
        ],
        "Modules": [
            {
                "Contract": "ICustomerProfileManager",
                "ContractAssembly": "Company.Project.Managers.CustomerService",
                "Lifetime": "Scoped",
                "Implementation": {
                    "Source": "Module",
                    "Assembly": "Company.Project.Managers.CustomerService"
                },
                "Dependencies": [
                    {
                        "Contract": "ILoggerFactory",
                        "Implementation": {"Source": "Shared"}
                    },
                    {
                        "Contract": "IProfileBuilder",
                        "ContractAssembly": "Company.Project.Engines.ProfileBuilder",
                        "Lifetime": "Transient",
                        "Implementation": {
                            "Source": "Module",
                            "Assembly": "Company.Project.Engines.ProfileBuilder"
                        }
                    },
                    {
                        "Contract": "IProfileAccess",
                        "ContractAssembly": "Company.Project.Resources.Profiles.Abstractions",
                        "Lifetime": "Scoped",
                        "Implementation": {
                            "Source": "Module",
                            "Assembly": "Company.Project.Resources.Profiles.SqlServer"
                        }
                    },
                    ... Other components that the CustomerProfileManager depends on ...
                ]
            }
            {
                ... Some other "top-level" module ...
            }
        ]
    }
    ... other configuration sections ...
}
```

There is a small handful of rules that you need to keep in mind when composing your services into applications.  

1. Only Manager and Client components may be declared as Top-Level Modules in your Architecture configuration.
2. Engines and ResourceAccess may only exist as Dependencies of those Top-Level Modules.
3. "Utility" services are registered in their own DI container.  That container is provided to the Modules and their dependencies for "common" services like Logging, HttpsClients, etc...
4. Modules can each have their own Behaviors collection.  Any Behaviors that are declared as "Global" are passed through all Module and Dependency components.
    - ***Note:***  A Behavior will only be added to any given component once, even if the Behavior is declared as both a "Module" and a "Global" behavior.
5. Modules cannot share Dependencies.  If you have two modules that use the same Dependency, each Module requires its own entry in its Dependency configuration.
    - ***Note:*** I haven't implemented it, but I may identify a need to add a "SharedServices" node under the Architecture configuration node.  This collection would be an appropriate place to declare "Shared" dependency modules, as long as each instance of that Dependency is configured in the exact same way.

## Set up Program.cs  
I'm using the old-school, Program.cs style here and in my sample console app.  If you prefer top-level statements, do you.
1. Create an "Application" logger.  This is used during app build-up and Module construction.  You'll use a different logger instance for your actual Module & Dependency components.
2. Load the configuration.  I like to push this code into its own Function, just to keep `Main` tidy.
3. Build out the "Shared" services.  (These are the "Utility" services that are used by and injected into any Module or Dependency component as needed.)  
4. Set up the "Use Case" providers as a 2nd ServiceProvider.  This is a single call to an extension method in iFX to configure the ServiceCollection, then you invoke `BuildServiceProvider` to materialize it as a DI container.
5. Obtain instances of your Modules and use them as needed.

### Program.cs  Main
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YourCompany.iFX;

internal class Program
{
    private static void Main(string[] args)
    {
        // This is the logger that's used during application startup.
        // I like having this so I can more easily see what's going on
        // while my services are being discovered & configured.
        ILogger<Program> logger = CreateAppLogger();
        IConfiguration appCOnfig = BuildConfiguration();
        
        // Set up the "Global" utility services.
        IServiceProvider utilities = BuildUtilities(
            appConfig,
            logger);
        
        // Set up the App Architecture.
        IServiceCollection appServices = new ServiceCollection();
        // You'll see a WHOLE BUNCH of Log Messages coming from the appLogger here.
        appServices.AddAppArchitecture(appConfig, utilities, logger);

        // Then, to get to the Modules, you...
        IServiceProvider serviceProvider = appServices.BuildServiceProvider();

        var sampleService = serviceProvider.GetService<ICustomerProfileManager>();
        // Now you can "do stuff" with the CustomerProfileManager instance.
    }
}
```

### CreateAppLogger
```csharp
    private static ILogger<Program> CreateAppLogger()
    {
        IServiceCollection classServices = new ServiceCollection();
        IConfiguration appConfig = BuildConfiguration();
        classServices = ConfigureLogging(classServices, appConfig);
        var sp = classServices.BuildServiceProvider();

        ILogger<Program> appLogger = sp.GetRequiredService<ILogger<Program>>();
        appLogger.LogInformation("Application Logger Created");
        return appLogger;
    }

    // We have this because we'll use essentially the same code to create the
    // application logger used during the app's Bootstrapping Phase 
    // and the System Logger that's injected into our Components.
    private static IServiceCollection ConfigureLogging
        (IServiceCollection serviceBuilder,
        IConfiguration config,
        ILogger<Program>? logger = null)
    {
        try
        {
            serviceBuilder.AddLogging(logBuilder =>
            {
                var logConfig = config.GetSection("Logging");
                if(logConfig != null)
                {
                    logBuilder.AddConfiguration(logConfig);
                }
                logBuilder.AddConsole();
            });
            logger?.LogInformation("Global Logging Added to SharedServices.");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Global logging could not be added.  System will not at runtime.");
        }

        return serviceBuilder;
    }    
```

### BuildConfiguration
```csharp
    private static IConfiguration BuildConfiguration(
        ILogger<Program>? logger = null)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

        // Add Environment Variables, UserSecrets, KeyVault, etc.... 
        // here as needed for your Module & Dependency Settings.

        logger?.LogInformation("App Configuration Loaded");
           
        return configBuilder.Build();
    }
```

### Build out "Shared" services:
Here's where you'll set up the Logging that would be injected into your Modules.  You'd also add things like HttpClient to obtain a single IHttpClientFactory, etc...
```csharp
    private static IServiceProvider BuildUtilities(
        IConfiguration appConfig,
        ILogger<Program> logger)
    {
        logger.LogInformation("Add Utility Services.");

        IServiceCollection serviceBuilder = new ServiceCollection();

        serviceBuilder = ConfigureLogging(
            serviceBuilder, 
            appConfig,
            logger);

        // HttpClient, UNTYPED HttpClient or DbContext, etc... can be added now.
        // Remember that anything you add here is available to every Module and 
        // Dependency of your Application.

        // DO NOT ADD SERVICES that should be "Hidden" within a Module or Dependency.

        logger.LogInformation("Utility Services Built.");    
        return serviceBuilder.BuildServiceProvider();
    }
```

And that's the basic idea.

## Things I've not gotten to yet

### Type Caching
I might set up a TypeCache service in the Utilities collection, if only to cut down on some of the Reflection that has to happen when instantiating Modules and Dependencies.  There's a good bit of Assembly Scanning going on, and if I can set things up so that it only has to happen once when the app Starts, that'll make me pretty happy.

### ModuleSettings
As of this writing, I've not solved the ModuleSettings story.  I visualize another complex property, perhaps within the Implementation specification of each Module or Dependency that holds a set of Key-Value pairs that would be bound to some "Options" type that's specific to the Implementation classes.  

I'll need to include some facility to point the Options binder to additional ConfigurationSources for storage and retrieval of more variable (Environment) and/or sensitive (Secret) configuration values.

I'll update this document once I've told that story.  (It'll be "Coming Soon<sup>TM</sup>")

### More Interaction Kinds, and More Component Behaviors

I'd love to be able to expose a Top-Level module as a set of API Endpoints via "Hosting" behaviors.  That's going to get complex though, and I may not go there just yet.  

I'd also like to experiment with Interface wrapping, using the Transparent Proxy concept so that we can wrap Input & Output parameters on a Contract as Request/Response types and provide some richer handling, without polluting the component code with Request and Response interrogation.  Again, if I can't do this invisibly, I'm likely to either use Request/Response DTOs on the Business Interfaces directly, or throw that concept away entirely.  

(Request/Response wrappers allow us to add contextual information about the calls and use cases and workflows at execution time.  Very useful for Observability, and vital for some kinds of Interactions.)

