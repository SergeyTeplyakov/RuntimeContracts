namespace System.Diagnostics.ContractsLight;

/// <summary>
/// Carries information about a failed contract.
/// </summary>
public sealed class ContractFailedEventArgs : EventArgs
{
    internal Exception ThrownDuringHandler;

    /// <nodoc />
    public ContractFailedEventArgs(ContractFailureKind failureKind, string message, string condition)
    {
        FailureKind = failureKind;
        Message = message;
        Condition = condition;
    }

    /// <nodoc />
    public string Message { get; }

    /// <nodoc />
    public string Condition { get; }

    /// <nodoc />
    public ContractFailureKind FailureKind { get; }

    /// <summary>
    /// Whether the event handler "handles" this contract failure, or to fail via escalation policy.
    /// </summary>
    public bool Handled { get; private set; }

    /// <nodoc />
    public void SetHandled()
    {
        Handled = true;
    }

    /// <nodoc />
    public bool Unwind { get; private set; }

    /// <nodoc />
    public void SetUnwind()
    {
        Unwind = true;
    }
}