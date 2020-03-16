using System.Diagnostics.ContractsLight;

#nullable enable

namespace System.Diagnostics.FluentContracts
{
    /// <summary>
    /// Set of extension methods for structs provided by <see cref="Contract"/> class that enforce contract invariants.
    /// </summary>
    public static class ContractFluentExtensions
    {
        /// <summary>
        /// Generates a contract exception if a contract is violated.
        /// </summary>
        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void IsTrue(this in PreconditionFailure result, string? message = null)
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
        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        [Conditional("DEBUG")]
        public static void IsTrue(this in PreconditionDebugFailure result, string? message = null)
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
        public static void IsTrue(this in AssertionFailure result, string? message = null)
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
        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        [Conditional("DEBUG")]
        public static void IsTrue(this in AssertionDebugFailure result, string? message = null)
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
        [Conditional("CONTRACTS_LIGHT_QUANTIFIERS")]
        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void IsTrue(this in PreconditionForAllFailure result, string? message = null)
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
        [Conditional("CONTRACTS_LIGHT_QUANTIFIERS")]
        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        public static void IsTrue(this in AssertionForAllFailure result, string? message = null)
        {
            ContractRuntimeHelper.ReportFailure(
                ContractFailureKind.Assert,
                message,
                conditionTxt: null,
                provenance: new Provenance(result.Path, result.LineNumber));
        }
    }
}
