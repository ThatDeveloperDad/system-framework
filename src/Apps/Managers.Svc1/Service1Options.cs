using ThatDeveloperDad.iFX.ServiceModel;

namespace Managers.Svc1;

/// <summary>
/// Using this as a proof of concept for setting up Module Configuration Options.
/// </summary>
public struct Service1Options:IServiceOptions
{
    public Service1Options()
    {
        StringOption = string.Empty;
        SomeSecret = string.Empty;
    }

    public string StringOption { get; set; }
    public int IntOption { get; set; }

    public string SomeSecret { get; set; }
}
