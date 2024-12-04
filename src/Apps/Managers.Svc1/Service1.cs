using Apps.Engines.Engine1.Abstractions;
using Apps.Managers.Svc1.Abstractions;
using Microsoft.Extensions.Logging;
using ThatDeveloperDad.iFX.ServiceModel;

namespace Managers.Svc1;

public class Service1 : ISvc1
{
    private IEngine1 _engine;
    private ILogger? _logger;
    private Service1Options? _settings;

    public Service1
        (
            IEngine1 engine, 
            ILoggerFactory logFactory,
            Service1Options? settings
        )
    {
        _engine = engine;
        _settings = settings;
        _logger = logFactory.CreateLogger(this.GetType().Name);
        _logger?.LogInformation("Service1 created.");
    }

    public void DoSomething()
    {
        
        string theEquation;

        int[] numbers = {1,2,3,4,5};
        int sum = _engine.CalcSum(numbers);

        theEquation = $"The sum of {string.Join("+", numbers)} is {sum}.";
        Console.WriteLine(theEquation);


        // Test code that I'm using while I develop the ComponentSettings
        // story.
        /* if(_settings != null)
        {
            _logger?.LogInformation("I have some settings!.");
            _logger?.LogInformation($"StringOption: {_settings?.StringOption}");
            _logger?.LogInformation($"IntOption: {_settings?.IntOption}");
        }
        else
        {
            _logger?.LogInformation("No settings found.");
        }
        
        _logger?.LogInformation("Service1 did something."); */
    }

}
