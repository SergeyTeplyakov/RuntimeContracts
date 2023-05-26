using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace System.Diagnostics.ContractsLight;

// This file contains a set of methods that allows using this version with versions 0.3.+ and 0.4.+ without recompilation.

public static partial class Contract
{
    /// <nodoc />
    [Pure]
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Requires(
        [DoesNotReturnIf(false)]
        bool condition,
        string userMessage,
        string path,
        int lineNumber)
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, conditionTxt: null, new Provenance(path, lineNumber));
        }
    }

    /// <nodoc />
    [Pure]
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS_DEBUG")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RequiresDebug(
        [DoesNotReturnIf(false)]
        bool condition,
        string userMessage,
        string path,
        int lineNumber)
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, conditionTxt: null, new Provenance(path, lineNumber));
        }
    }

    /// <nodoc />
    [Pure]
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Requires<TException>(
        [DoesNotReturnIf(false)]
        bool condition,
        string userMessage,
        string path,
        int lineNumber) where TException : Exception
#if !NETSTANDARD2_0
        // Previous version are relies on new constraint for exception construction.
        , new()
#endif
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportPreconditionFailure<TException>(userMessage, conditionTxt: null, new Provenance(path, lineNumber));
        }
    }

    /// <nodoc />
    [Pure]
    [Conditional("CONTRACTS_LIGHT_ASSERTS")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Assert(
        [DoesNotReturnIf(false)]
        bool condition,
        string userMessage,
        string path,
        int lineNumber)
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, conditionTxt: null, new Provenance(path, lineNumber));
        }
    }

    /// <nodoc />
    [Pure]
    [Conditional("CONTRACTS_LIGHT_ASSERTS_DEBUG")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void AssertDebug(
        [DoesNotReturnIf(false)]
        bool condition,
        string userMessage,
        string path,
        int lineNumber)
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, conditionTxt: null, new Provenance(path, lineNumber));
        }
    }

    /// <nodoc />
    [Pure]
    [Conditional("CONTRACTS_LIGHT_ASSERTS")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Assume(
        [DoesNotReturnIf(false)]
        bool condition,
#if NETSTANDARD2_0
            [Localizable(false)]
#endif
        string userMessage,
        string path,
        int lineNumber)
    {
        if (!condition)
        {
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assume, userMessage, conditionTxt: null, new Provenance(path, lineNumber));
        }
    }
}