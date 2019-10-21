using System;
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
using RuntimeContracts.Analyzer.Utilities;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RuntimeContracts.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RuntimeContractsAnalyzerCodeFixProvider)), Shared]
    public class UseSimplifiedNullCheckCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use simplified null check.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseSimplifiedNullCheckAnalyzer.DiagnosticId);

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
                    createChangedDocument: c => UseSimplifiedNullContractCheckAsync(context.Document, declaration, c), 
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> UseSimplifiedNullContractCheckAsync(Document document, InvocationExpressionSyntax invocationExpression, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var operation = (IInvocationOperation)semanticModel.GetOperation(invocationExpression);

            var (argument, suffix) = ProcessContractOperation(operation);

            // Replace (x != null) to '(x)' for regular null check or '!string.IsNullOrEmpty(x)' to 'x'
            var arguments = ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(argument));
            if (operation.Arguments[1].IsImplicit == false)
            {
                // Need to keep an optional second argument.
                arguments = arguments.AddArguments((ArgumentSyntax)operation.Arguments[1].Syntax);
            }

            var simplifiedContractCheck =
                invocationExpression
                    .WithArgumentList(arguments)
                    // Replace Requires to RequiresNotNull etc.
                    .WithExpression(
                        invocationExpression
                            .Expression.As(e => (MemberAccessExpressionSyntax)e)
                            .WithName(IdentifierName($"{operation.TargetMethod.Name}{suffix}")));
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            root = root.ReplaceNode(invocationExpression, simplifiedContractCheck);
            return document.WithSyntaxRoot(root);
        }

        private (ArgumentSyntax argument, string simplifiedContractSuffix) ProcessContractOperation(IInvocationOperation contractInvocation)
        {
            var firstArgument = contractInvocation.Arguments[0].Value;
            if (firstArgument is IBinaryOperation binary &&
                UseSimplifiedNullCheckAnalyzer.IsNullCheck(binary, out var operand))
            {
                var argument = Argument((ExpressionSyntax)operand!.Syntax);
                return (argument: argument, "NotNull");
            }

            if (firstArgument is IUnaryOperation unary &&
                UseSimplifiedNullCheckAnalyzer.IsNullOrEmptyCheck(unary, out var nullOrEmptyArgument))
            {
                var invocation = (IInvocationOperation)nullOrEmptyArgument!.Parent;
                string simplifiedContractSuffix = invocation.TargetMethod.Name.EndsWith("WhiteSpace") ? "NotNullOrWhiteSpace" : "NotNullOrEmpty";
                return (argument: (ArgumentSyntax)nullOrEmptyArgument!.Syntax, simplifiedContractSuffix);
            }

            throw new InvalidOperationException($"Unreachable.");
        }
    }
}
