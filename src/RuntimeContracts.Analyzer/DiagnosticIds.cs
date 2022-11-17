#nullable enable

using Microsoft.CodeAnalysis;

namespace RuntimeContracts.Analyzer;

public class DiagnosticIds
{
    /// <nodoc />
    public static readonly DiagnosticDescriptor RA001 = new DiagnosticDescriptor(
        id: nameof(RA001),
        title: "Do not use System.Diagnostics.Contract class",
        messageFormat: "Do not use System.Diagnostics.Contract class",
        category: "Correctness",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "System.Diagnostics.Contract class can be used only with ccrewrite enabled.");

    /// <summary>
    /// Warns when 'Contract.Assert(s > 0, string.Format("s is {0}", s)'.
    /// </summary>
    public static readonly DiagnosticDescriptor RA002 = new DiagnosticDescriptor(
        id: nameof(RA002),
        title: "Do not construct contract message programmatically",
        messageFormat: "Do not construct contract message programmatically for performance reasons",
        "Performance",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Create message using interpolated string expressions because those messages are created lazily only when a contract is violated.");

    // No longer needed when Fluent API is used
    // public const string UseSimplifiedNullCheckId = "RA003";

    /// <summary>
    /// Obsolete. The predicate string is computed by the compiler.
    /// </summary>
    public const string ProvideMessageId = "RA004";

    /// <summary>
    /// Obsolete. Fluent API will be removed.
    /// </summary>
    public const string FluentAssertionResultIsNotObserved = "RA005";

    /// <summary>
    /// Obsolete. Fluent API will be removed.
    /// </summary>
    public const string UseFluentContracts = "RA006";

    /// <nodoc />
    public static readonly DiagnosticDescriptor RA007 =
        new DiagnosticDescriptor(
            id: nameof(RA007),
            title: "Do not use simplified null-check contracts",
            messageFormat: "Do not use simplified null-check contracts", 
            category: "Usability",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "The API like 'Contract.RequiresNotNull' will be obsolete and should not be used.");

    /// <summary>
    /// This version of the library requires C#10+ version.
    /// </summary>
    public static readonly DiagnosticDescriptor RA008 = new DiagnosticDescriptor(
        id: nameof(RA008),
        title: "Interpolated string expression is used with an older C# version",
        messageFormat: "An allocation will happen during message construction that can be avoided by switching to C# 10 or above",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The current version of the library relies on interpolated string improvements that only available in C# 10 or later.");

    /// <summary>
    /// Detects that fluent API is used and suggests migrating it back to a normal API with a message constructed using interpolated string.
    /// </summary>
    public static readonly DiagnosticDescriptor RA009 = new DiagnosticDescriptor(
        id: nameof(RA009),
        title: "Do not use Contracts Fluent API",
        messageFormat: "Do not use Contracts Fluent API",
        category: "Usability",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true,
        description: "The fluent API like Contract.Check(cond)?.Assert(message) is obsolete because interpolated strings in C# 10 solve the memory allocation issue out of the box.");
}