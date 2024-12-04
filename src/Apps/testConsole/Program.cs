using Apps.Managers.Svc1.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThatDeveloperDad.iFX;

internal class Program
{
    private static void Main(string[] args)
    {
        ILogger<Program> logger = CreateAppLogger();
        Console.WriteLine("The Goal of this test is to set up configurable Application Modules.");
        
        IConfiguration appConfig = BuildConfiguration();
        IServiceProvider utilities = BuildUtilities(
            appConfig, 
            logger);

        IServiceCollection appServices = new ServiceCollection();
        appServices.AddAppArchitecture(appConfig, utilities, logger);

        IServiceProvider useCaseProviders = appServices.BuildServiceProvider();

        var exampleManager = useCaseProviders.GetService<ISvc1>();

        exampleManager?.DoSomething();

        Console.WriteLine("hello world.");

        if(useCaseProviders is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    

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

    private static IConfiguration BuildConfiguration(
        ILogger<Program>? logger = null)
    {
        #if DEBUG
           ThatDeveloperDad.iFX.DevTools.LocalSettingsReader
            .LoadLocalSettings("localsettings.env", 
                clobber: true,
                throwOnError: true,
                logger: logger);
        #endif

        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
            .AddEnvironmentVariables();
        
        

        logger?.LogInformation("App Configuration Loaded");
           
        return configBuilder.Build();
    }

    /// <summary>
    /// Add any "native" services that are used within the SystemComponents here.
    /// This includes Logging, Configuration, HttpClient Fcatories, etc...
    /// </summary>
    /// <param name="appConfig"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
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

        logger.LogInformation("Utility Services Built.");    
        return serviceBuilder.BuildServiceProvider();
    }

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
}