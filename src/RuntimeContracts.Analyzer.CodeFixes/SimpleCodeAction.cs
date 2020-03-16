using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace RuntimeContracts.Analyzer
{
    internal abstract class SimpleCodeAction : CodeAction
    {
        public SimpleCodeAction(string title, string equivalenceKey)
        {
            Title = title;
            EquivalenceKey = equivalenceKey;
        }

        public sealed override string Title { get; }
        public sealed override string EquivalenceKey { get; }

        protected override Task<Document?> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Document?>(null);
        }
    }
}
