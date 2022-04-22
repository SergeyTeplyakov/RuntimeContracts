using Microsoft.CodeAnalysis.CodeActions;

namespace RuntimeContracts.Analyzer;

internal abstract class SimpleCodeAction : CodeAction
{
    protected SimpleCodeAction(string title, string? equivalenceKey)
    {
        Title = title;
        EquivalenceKey = equivalenceKey;
    }

    public sealed override string Title { get; }
    public sealed override string? EquivalenceKey { get; }
}