using System;

namespace ThatDeveloperDad.iFX.ServiceModel;

public class ServiceError
{
    public string Site { get; set; } = string.Empty;

    public string ErrorKind { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;

    public ErrorSeverity Severity { get; set; }
}

public enum ErrorSeverity
{
    /// <summary>
    /// Identifies a condition that does not signal that a process should be halted,
    /// but may result in a problems further on.
    /// </summary>
    Warning,
    /// <summary>
    /// Identifies a condition that signals a process should be halted immediately
    /// due to a critical error.
    /// </summary>
    Error
}
