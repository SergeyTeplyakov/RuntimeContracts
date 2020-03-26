using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable
namespace System.Diagnostics.ContractsLight
{
    public partial class Contract
    {
        /// <summary>
        /// Allocation-free contract check that must follow by
        /// <see cref="ContractFluentExtensions.Requires(in AssertionFailure, string)"/>
        /// or <see cref="ContractFluentExtensions.Assert(in AssertionFailure, string)"/>
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="Requires"/> method, the message for this contract check is constructed
        /// only if the contract is violated, making it possible to use the checks like
        /// <code>Contract.Check(x > 0)?.Assert($"x > 0, x = [{x}]")</code> on application
        /// critical paths and not worry about excessive allocations.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssertionFailure? Check(
            [DoesNotReturnIf(false)]
            bool condition,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!condition)
            {
                return new AssertionFailure(path, lineNumber);
            }

            return null;
        }

        /// <summary>
        /// Debug-only allocation-free contract check that must follow by
        /// <see cref="ContractFluentExtensions.Requires(in AssertionFailure, string)"/>
        /// or <see cref="ContractFluentExtensions.Assert(in AssertionFailure, string)"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AssertionDebugFailure? CheckDebug(
            [DoesNotReturnIf(false)]
            bool condition,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!condition)
            {
                return new AssertionDebugFailure(path, lineNumber);
            }

            return null;
        }
    }

    /// <summary>
    /// Set of extension methods for structs provided by <see cref="Contract"/> class that enforce contract invariants.
    /// </summary>
    public static class ContractFluentExtensions
    {
        /// <summary>
        /// Generates a contract exception if a contract is violated.
        /// </summary>
        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void Requires(this in AssertionFailure result, string message)
        {
            ContractRuntimeHelper.ReportFailure(
                ContractFailureKind.Precondition,
                message,
                conditionTxt: null,
                provenance: new Provenance(result.Path, result.LineNumber));
        }

        /// <summary>
        /// Generates a contract exception if a contract is violated.
        /// </summary>
        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS_DEBUG")]
        public static void Requires(this in AssertionDebugFailure result, string message)
        {
            ContractRuntimeHelper.ReportFailure(
                ContractFailureKind.Precondition,
                message,
                conditionTxt: null,
                provenance: new Provenance(result.Path, result.LineNumber));
        }

        /// <summary>
        /// Generates a contract exception if a contract is violated.
        /// </summary>
        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        public static void Assert(this in AssertionFailure result, string message)
        {
            ContractRuntimeHelper.ReportFailure(
                ContractFailureKind.Assert,
                message,
                conditionTxt: null,
                provenance: new Provenance(result.Path, result.LineNumber));
        }

        /// <summary>
        /// Generates a contract exception if a contract is violated.
        /// </summary>
        [Conditional("CONTRACTS_LIGHT_ASSERTS_DEBUG")]
        public static void Assert(this in AssertionDebugFailure result, string message)
        {
            ContractRuntimeHelper.ReportFailure(
                ContractFailureKind.Assert,
                message,
                conditionTxt: null,
                provenance: new Provenance(result.Path, result.LineNumber));
        }
    }
}
