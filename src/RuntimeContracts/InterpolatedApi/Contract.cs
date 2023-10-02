using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.ContractsLight;

public static partial class Contract
{
    /// <summary>
    /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.</param>
    /// <param name="userMessage">User-provided error message.</param>
    /// <param name="conditionText">A compiler generated string for the predicate expression.</param>
    /// <param name="path">Compiler generated path to the file with the assertion.</param>
    /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// Use this form when backward compatibility does not force you to throw a particular exception.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
    [DebuggerStepThrough]
    public static void Requires(
        [DoesNotReturnIf(false)]
        bool condition,
        [InterpolatedStringHandlerArgument("condition")]ref ContractMessageInterpolatedStringHandler userMessage,
        [CallerArgumentExpression("condition")] string conditionText = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage.ToString(), conditionText, new Provenance(path, lineNumber));
        }
    }

    /// <summary>
    /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.</param>
    /// <param name="userMessage">User-provided error message.</param>
    /// <param name="conditionText">A compiler generated string for the predicate expression.</param>
    /// <param name="path">Compiler generated path to the file with the assertion.</param>
    /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// Use this form when backward compatibility does not force you to throw a particular exception.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS_DEBUG")]
    [DebuggerStepThrough]
    public static void RequiresDebug(
        [DoesNotReturnIf(false)]
        bool condition,
        [InterpolatedStringHandlerArgument("condition")]ref ContractMessageInterpolatedStringHandler userMessage,
        [CallerArgumentExpression("condition")] string conditionText = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage.ToString(), conditionText, new Provenance(path, lineNumber));
        }
    }

    /// <summary>
    /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.</param>
    /// <param name="userMessage">User-provided error message.</param>
    /// <param name="conditionText">A compiler generated string for the predicate expression.</param>
    /// <param name="path">Compiler generated path to the file with the assertion.</param>
    /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// Use this form when you want to throw a particular exception.
    /// </remarks>
    /// <typeparam name="TException">Exception type that will be thrown if a precondition is failed.</typeparam>
    [Pure]
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
    [DebuggerStepThrough]
    public static void Requires<TException>(
        [DoesNotReturnIf(false)]
        bool condition,
        [InterpolatedStringHandlerArgument("condition")]ref ContractMessageInterpolatedStringHandler userMessage,
        [CallerArgumentExpression("condition")] string conditionText = "",
        [CallerFilePath] string path = "", 
        [CallerLineNumber] int lineNumber = 0) where TException : Exception
#if !NETSTANDARD2_0
        // Previous version are relies on new constraint for exception construction.
        , new()
#endif
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportPreconditionFailure<TException>(userMessage.ToString(), conditionText, new Provenance(path, lineNumber));
        }
    }

    /// <summary>
    /// Perform a runtime check that <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">Expression to check to always be true.</param>
    /// <param name="userMessage">User-provided error message.</param>
    /// <param name="conditionText">A compiler generated string for the predicate expression.</param>
    /// <param name="path">Compiler generated path to the file with the assertion.</param>
    /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
    [Pure]
    [Conditional("CONTRACTS_LIGHT_ASSERTS")]
    [DebuggerStepThrough]
    public static void Assert(
        [DoesNotReturnIf(false)]
        bool condition,
        [InterpolatedStringHandlerArgument("condition")]ref ContractMessageInterpolatedStringHandler userMessage,
        [CallerArgumentExpression("condition")] string conditionText = "",
        [CallerFilePath] string path = "", 
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage.ToString(), conditionText, new Provenance(path, lineNumber));
        }
    }

    /// <summary>
    /// Perform a runtime check that <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">Expression to check to always be true.</param>
    /// <param name="userMessage">User-provided error message.</param>
    /// <param name="conditionText">A compiler generated string for the predicate expression.</param>
    /// <param name="path">Compiler generated path to the file with the assertion.</param>
    /// <param name="lineNumber">Compiler generated line number of the assertion.</param>
    [Pure]
    [Conditional("CONTRACTS_LIGHT_ASSERTS_DEBUG")]
    [DebuggerStepThrough]
    public static void AssertDebug(
        [DoesNotReturnIf(false)]
        bool condition,
        [InterpolatedStringHandlerArgument("condition")]ref ContractMessageInterpolatedStringHandler userMessage,
        [CallerArgumentExpression("condition")] string conditionText = "",
        [CallerFilePath] string path = "", 
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage.ToString(), conditionText, new Provenance(path, lineNumber));
        }
    }
}