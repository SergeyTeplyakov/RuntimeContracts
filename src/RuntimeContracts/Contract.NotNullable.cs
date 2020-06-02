// --------------------------------------------------------------------
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// --------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace System.Diagnostics.ContractsLight
{
    public partial class Contract
    {
        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void RequiresNotNull<T>(
            [NotNull]T? o, 
            string? userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0) where T : class
        {
            if (o == null)
            {
                userMessage ??= "The value should not be null.";
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, null, new Provenance(path, lineNumber));
            }
        }

        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void RequiresNotNull<T>(
            [NotNull]T? o,
            string? userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0) where T : struct
        {
            if (o == null)
            {
                userMessage ??= "The value should not be null.";
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, null, new Provenance(path, lineNumber));
            }
        }

        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void RequiresNotNullOrEmpty(
            [NotNull]string? o, 
            string? userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            // check for null is need to avoid a compilation warning
            if (o == null || string.IsNullOrEmpty(o))
            {
                userMessage ??= "The value should not be null or empty.";
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, null, new Provenance(path, lineNumber));
            }
        }

        [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
        public static void RequiresNotNullOrWhiteSpace(
            [NotNull]string? o, 
            string? userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            // check for null is need to avoid a compilation warning
            if (o == null || string.IsNullOrWhiteSpace(o))
            {
                userMessage ??= "The value should not be null or whitespace.";
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, null, new Provenance(path, lineNumber));
            }
        }
        
        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        public static void AssertNotNull<T>(
            [NotNull]T? value, 
            string? userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0) where T : class
        {
            if (value == null)
            {
                userMessage ??= "The value should not be null.";
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, null, new Provenance(path, lineNumber));
            }
        }

        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        public static void AssertNotNullOrEmpty(
            [NotNull]string? o, 
            string? userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            // check for null is need to avoid a compilation warning
            if (o == null || string.IsNullOrEmpty(o))
            {
                userMessage ??= "The value should not be null or empty.";
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, null, new Provenance(path, lineNumber));
            }
        }

        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        public static void AssertNotNullOrWhiteSpace(
            [NotNull]string? o, 
            string? userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            // check for null is need to avoid a compilation warning
            if (o == null || string.IsNullOrWhiteSpace(o))
            {
                userMessage ??= "The value should not be null or whitespace.";
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, null, new Provenance(path, lineNumber));
            }
        }

        [Conditional("CONTRACTS_LIGHT_ASSERTS")]
        public static void AssertNotNull<T>(
            [NotNull]T? value, 
            string? userMessage = null,
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0) where T : struct
        {
            if (value == null)
            {
                userMessage ??= "The value should not be null.";
                ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, null, new Provenance(path, lineNumber));
            }
        }
    }
}