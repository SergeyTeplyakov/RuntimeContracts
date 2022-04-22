using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

#nullable enable

namespace RuntimeContracts.Core;

internal static class LanguageVersionHelper
{
    /// <summary>
    /// Gets the C# language version for a given <paramref name="operation"/>.
    /// </summary>
    public static LanguageVersion GetLanguageVersion(IOperation operation, LanguageVersion unknownVerson = LanguageVersion.Latest)
    {
        return (operation.Syntax.SyntaxTree.Options as CSharpParseOptions)?.LanguageVersion ?? unknownVerson;
    }
}