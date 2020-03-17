using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using RuntimeContracts.Analyzer.Core;

namespace RuntimeContracts.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRuntimeContractsCodeFixProvider)), Shared]
    public class GenerateMessageCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Generate message based on the predicate.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ProvideMessageAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            // Looking for contract check.
            var declaration = (InvocationExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan);

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => GenerateMessageBasedOnPredicateAsync(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> GenerateMessageBasedOnPredicateAsync(Document document, InvocationExpressionSyntax invocationExpression, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var operation = (IInvocationOperation)semanticModel.GetOperation(invocationExpression);

            var arguments = ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add((ArgumentSyntax)operation.Arguments[0].Syntax));

            var message = operation.Arguments[0].Syntax.ToFullString();
            var contractMethod = ContractResolver.ParseContractMethodName(operation.TargetMethod.Name);

            if ((contractMethod & ContractMethodNames.RequiresNotNull) != ContractMethodNames.None ||
                (contractMethod & ContractMethodNames.AssertNotNull) != ContractMethodNames.None)
            {
                message += " is not null";
            }

            if ((contractMethod & ContractMethodNames.RequiresNotNullOrEmpty) != ContractMethodNames.None ||
                (contractMethod & ContractMethodNames.AssertNotNullOrEmpty) != ContractMethodNames.None)
            {
                message += " is not null or empty";
            }

            if ((contractMethod & ContractMethodNames.RequiresNotNullOrWhiteSpace) != ContractMethodNames.None ||
                (contractMethod & ContractMethodNames.AssertNotNullOrWhiteSpace) != ContractMethodNames.None)
            {
                message += " is not null or whitespace";
            }

            var predicate = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(message));

            arguments = arguments.AddArguments(Argument(predicate));

            var simplifiedContractCheck = invocationExpression.WithArgumentList(arguments);
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            root = root.ReplaceNode(invocationExpression, simplifiedContractCheck);
            return document.WithSyntaxRoot(root);
        }
    }
}
