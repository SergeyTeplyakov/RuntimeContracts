using Microsoft.CodeAnalysis;

#nullable enable

namespace RuntimeContracts.Analyzer.Core;

internal static class SymbolCompareExtensions
{
    public static bool SymbolEquals(this ISymbol lhs, ISymbol rhs) => SymbolEqualityComparer.Default.Equals(lhs, rhs);
}