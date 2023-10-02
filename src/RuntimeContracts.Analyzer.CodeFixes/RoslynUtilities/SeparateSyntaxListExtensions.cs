using Microsoft.CodeAnalysis;

namespace RuntimeContracts.Analyzer;

public static class SeparateSyntaxListExtensions
{
    public static SeparatedSyntaxList<T> AddIfNotNull<T>(this SeparatedSyntaxList<T> list, T? argument) where T : SyntaxNode
    {
        if (argument != null)
        {
            return list.Add(argument);
        }

        return list;
    }
}