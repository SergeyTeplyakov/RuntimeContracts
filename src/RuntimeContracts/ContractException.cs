namespace System.Diagnostics.ContractsLight;

/// <summary>
/// An exception that is generated when a contract violation occurs.
/// </summary>
/// <remarks>
/// This exception type in Code Contracts is internal, but we made it public intentionally
/// because even though it should not be happening in some cases it is useful to know that the
/// exception's type is a contract violation.
/// </remarks>
public sealed class ContractException : Exception
{
    private struct ContractExceptionData
    {
        public ContractFailureKind Kind;

        public string? UserMessage;

        public string? Condition;
    }

    private ContractExceptionData m_data = default(ContractExceptionData);

    public ContractFailureKind Kind => m_data.Kind;

    public string Failure => Message;

    public string? UserMessage => m_data.UserMessage;

    public string? Condition => m_data.Condition;

    public ContractException(ContractFailureKind kind, string? failure, string? userMessage, string? condition, Exception? innerException = null) : base(failure, innerException)
    {
        m_data.Kind = kind;
        m_data.UserMessage = userMessage;
        m_data.Condition = condition;
    }
}