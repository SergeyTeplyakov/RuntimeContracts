using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using RuntimeContracts.Analyzer.Utilities;

namespace RuntimeContracts.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRuntimeContractsCodeFixProvider)), Shared]
    public class UseRuntimeContractsCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use System.Diagnostics.ContractsLight namespace.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotUseStandardContractAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            System.Diagnostics.Debugger.Launch();
            return;
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();

            // The call to Contract.* could be a fully-qualified one or via the using statement.
            var declaration = root.FindNode(diagnostic.Location.SourceSpan);
            
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => AddOrReplaceUsingsAsync(context.Document, c), 
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> AddOrReplaceUsingsAsync(Document document, CancellationToken cancellationToken)
        {
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(SyntaxTreeUtilities.AddOrReplaceContractNamespaceUsings(oldRoot));
        }
    }
}
