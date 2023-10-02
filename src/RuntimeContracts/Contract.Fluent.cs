using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.ContractsLight;

// This API will be officially obsolete soon because of new interpolated string improvements solve the issue this API is solving but in a more elegant way.
public partial class Contract
{
    /// <summary>
    /// [Obsolete] Allocation-free contract check that must follow by
    /// <see cref="ContractFluentExtensions.Requires(in AssertionFailure,string)"/>
    /// or <see cref="ContractFluentExtensions.Assert(in AssertionFailure, string)"/>
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Requires(bool,string,string,string,int)"/> method, the message for this contract check is constructed
    /// only if the contract is violated, making it possible to use the checks like
    /// <code>Contract.Check(x > 0)?.Assert($"x > 0, x = [{x}]")</code> on application
    /// critical paths and not worry about excessive allocations.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AssertionFailure? Check(
        [DoesNotReturnIf(false)]
        bool condition,
        [CallerArgumentExpression("condition")] string conditionText = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!condition)
        {
            return new AssertionFailure(path, lineNumber, conditionText);
        }

        return null;
    }

    /// <summary>
    /// [Obsolete] Debug-only allocation-free contract check that must follow by
    /// <see cref="ContractFluentExtensions.Requires(in AssertionFailure, string)"/>
    /// or <see cref="ContractFluentExtensions.Assert(in AssertionFailure, string)"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AssertionDebugFailure? CheckDebug(
        [DoesNotReturnIf(false)]
        bool condition,
        [CallerArgumentExpression("condition")] string conditionText = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!condition)
        {
            return new AssertionDebugFailure(path, lineNumber, conditionText);
        }

        return null;
    }
}

/// <summary>
/// Set of extension methods for structs provided by <see cref="Contract"/> class that enforce contract invariants.
/// </summary>
/// <remarks>
/// With C#10 amazing interpolated string improvements the fluent API is obsolete and will be officially obsolete in the upcoming versions.
/// </remarks>
public static class ContractFluentExtensions
{
    /// <summary>
    /// [Obsolete] Generates a contract exception if a contract is violated.
    /// </summary>
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
    public static void Requires(this in AssertionFailure result, string message)
    {
        ContractRuntimeHelper.ReportFailure(
            ContractFailureKind.Precondition,
            message,
            conditionTxt: result.ConditionText,
            provenance: new Provenance(result.Path, result.LineNumber));
    }

    /// <summary>
    /// [Obsolete] Generates a contract exception if a contract is violated.
    /// </summary>
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS_DEBUG")]
    public static void Requires(this in AssertionDebugFailure result, string message)
    {
        ContractRuntimeHelper.ReportFailure(
            ContractFailureKind.Precondition,
            message,
            conditionTxt: result.ConditionText,
            provenance: new Provenance(result.Path, result.LineNumber));
    }

    /// <summary>
    /// [Obsolete] Generates a contract exception if a contract is violated.
    /// </summary>
    [Conditional("CONTRACTS_LIGHT_ASSERTS")]
    public static void Assert(this in AssertionFailure result, string message)
    {
        ContractRuntimeHelper.ReportFailure(
            ContractFailureKind.Assert,
            message,
            conditionTxt: result.ConditionText,
            provenance: new Provenance(result.Path, result.LineNumber));
    }

    /// <summary>
    /// [Obsolete] Generates a contract exception if a contract is violated.
    /// </summary>
    [Conditional("CONTRACTS_LIGHT_ASSERTS_DEBUG")]
    public static void Assert(this in AssertionDebugFailure result, string message)
    {
        ContractRuntimeHelper.ReportFailure(
            ContractFailureKind.Assert,
            message,
            conditionTxt: result.ConditionText,
            provenance: new Provenance(result.Path, result.LineNumber));
    }
}