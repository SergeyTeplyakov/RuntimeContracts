// --------------------------------------------------------------------
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// --------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.ContractsLight;

public partial class Contract
{
    /// <summary>
    /// Checks that an object <paramref name="o"/> is not null.
    /// </summary>
    /// <remarks>
    /// Please use normal null check instead. This method will be obsolete in the next release.
    /// </remarks>
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
    public static void RequiresNotNull<T>(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
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

    /// <summary>
    /// Checks that an object <paramref name="o"/> is not null.
    /// </summary>
    /// <remarks>
    /// Please use normal null check instead. This method will be obsolete in the next release.
    /// </remarks>
    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
    public static void RequiresNotNull<T>(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
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
        if (string.IsNullOrEmpty(o))
        {
            userMessage ??= "The value should not be null or empty.";
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, null, new Provenance(path, lineNumber));
        }
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
    }
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.

    [Conditional("CONTRACTS_LIGHT_PRECONDITIONS")]
    public static void RequiresNotNullOrWhiteSpace(
        [NotNull]string? o, 
        string? userMessage = null,
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (string.IsNullOrWhiteSpace(o))
        {
            userMessage ??= "The value should not be null or whitespace.";
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Precondition, userMessage, null, new Provenance(path, lineNumber));
        }
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
    }
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.

    /// <summary>
    /// Checks that an object <paramref name="o"/> is not null.
    /// </summary>
    /// <remarks>
    /// Please use normal null check instead. This method will be obsolete in the next release.
    /// </remarks>
    [Conditional("CONTRACTS_LIGHT_ASSERTS")]
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
    public static void AssertNotNull<T>(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
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

    /// <summary>
    /// Checks that an object <paramref name="o"/> is not null or empty.
    /// </summary>
    /// <remarks>
    /// Please use normal null or empty check instead. This method will be obsolete in the next release.
    /// </remarks>
    [Conditional("CONTRACTS_LIGHT_ASSERTS")]
    public static void AssertNotNullOrEmpty(
        [NotNull]string? o, 
        string? userMessage = null,
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (string.IsNullOrEmpty(o))
        {
            userMessage ??= "The value should not be null or empty.";
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, null, new Provenance(path, lineNumber));
        }
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
    }
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.

    /// <summary>
    /// Checks that an object <paramref name="o"/> is not null or whitespace.
    /// </summary>
    /// <remarks>
    /// Please use normal null check instead. This method will be obsolete in the next release.
    /// </remarks>
    [Conditional("CONTRACTS_LIGHT_ASSERTS")]
    public static void AssertNotNullOrWhiteSpace(
        [NotNull]string? o, 
        string? userMessage = null,
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (string.IsNullOrWhiteSpace(o))
        {
            userMessage ??= "The value should not be null or whitespace.";
            ContractRuntimeHelper.ReportFailure(ContractFailureKind.Assert, userMessage, null, new Provenance(path, lineNumber));
        }
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
    }
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.

    /// <summary>
    /// Checks that an object <paramref name="o"/> is not null.
    /// </summary>
    /// <remarks>
    /// Please use normal null check instead. This method will be obsolete in the next release.
    /// </remarks>
    [Conditional("CONTRACTS_LIGHT_ASSERTS")]
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
    public static void AssertNotNull<T>(
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters
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