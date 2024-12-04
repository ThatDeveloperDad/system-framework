using System;

namespace ThatDeveloperDad.iFX.ServiceModel;

public abstract class OperationResponse
{
    protected  List<ServiceError> _errorsCollection = new();
    public OperationRequest? Request { get; protected set; }
    

    public bool HasWarnings => _errorsCollection.Any(e => e.Severity == ErrorSeverity.Warning);

    public bool HasErrors => _errorsCollection.Any(e => e.Severity == ErrorSeverity.Error);

    public void AddError(ServiceError error)
    {
        _errorsCollection.Add(error);
    }

    public void AddErrors(OperationResponse donor)
    {
        _errorsCollection.AddRange(donor._errorsCollection);
    }

}

public abstract class OperationResponse<T>:OperationResponse
{
        public OperationResponse(OperationRequest request, T? payload)
    {
        Request = request;
        Payload = payload;
    }
        
    public bool Successful => HasErrors==false && Payload!=null;

    public T? Payload { get; set; }

}
