using System;

namespace ThatDeveloperDad.iFX.ServiceModel;

public abstract class OperationRequest
{
    public OperationRequest(string workloadName)
    {
        WorkloadId = Guid.NewGuid();
        WorkloadName = workloadName;
        InvocationTimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public OperationRequest(OperationRequest parent)
    {
        WorkloadId = parent.WorkloadId;
        WorkloadName = parent.WorkloadName;
        InvocationTimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Identifies the instance of an overall Workload Execution.
    /// </summary>
    public Guid WorkloadId { get; protected set; }

    /// <summary>
    /// Identifies the name of the Workload that includes this Operation.
    /// </summary>
    public string WorkloadName{get; protected set;}

    /// <summary>
    /// Identifies the name of the Operation to which this request is being made.
    /// </summary>
    public abstract string OperationName { get; }

    /// <summary>
    /// Identifies the UtcTime (in ticks) at which the request was created.
    /// </summary>
    public long InvocationTimestampUtc {get; private set;}
}

public abstract class OperationRequest<T>
    :OperationRequest
{
    /// <summary>
    /// Carries the data required by the requested Operation.
    /// </summary>
    public T? Payload { get; set; }

    public OperationRequest(string workloadName, T? payload = default(T)):base(workloadName)
    {
        Payload = payload;
    }

    public OperationRequest(OperationRequest parent, T? payload = default(T))
        :base(parent)
    {
        Payload = payload;
    }
}

