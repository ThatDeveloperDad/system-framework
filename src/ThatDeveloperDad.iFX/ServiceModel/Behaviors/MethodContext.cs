using System;

namespace ThatDeveloperDad.iFX.Behaviors;

public class MethodContext
{
    public string MethodName { get; set; } = string.Empty;
    public object?[]? Parameters { get; set; } = Array.Empty<object>();
    public object? ReturnValue { get; set; } = null;
    public Exception? Exception { get; set; }
}
