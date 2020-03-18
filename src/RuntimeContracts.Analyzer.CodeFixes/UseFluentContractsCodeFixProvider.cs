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
using RuntimeContracts.Analyzer.Core;
using Microsoft.CodeAnalysis.CSharp;

using System;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static RuntimeContracts.Analyzer.Core.ContractMethodNames;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeStyle;

namespace RuntimeContracts.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseFluentContractsCodeFixProvider)), Shared]
    public class UseFluentContractsCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use fluent API for contract validation.";

        /// <inheritdoc />
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseFluentContractsAnalyzer.DiagnosticId);

        /// <inheritdoc />
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // Using a custom batch fixer, becuase the default one won't work.
            // The fixer changes a shared state (replaces a using directive) and this won't
            // allow a default fixer to run more than one fixer.
            return FixAll.Instance;
        }

        /// <inheritdoc />
        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => FixDocumentAsync(context.Document, context.Diagnostics, c),
                    equivalenceKey: Title),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> UseFluentContractsOrRemovePostconditionsAsync(
            Document document, 
            List<InvocationExpressionSyntax> invocationExpressions, 
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var nodeTranslationMap = new Dictionary<SyntaxNode, SyntaxNode>();

            var nodesToRemove = new List<SyntaxNode>();
            foreach (var invocationExpression in invocationExpressions)
            {
                var operation = (IInvocationOperation)semanticModel.GetOperation(invocationExpression);
                var contractResolver = new ContractResolver(semanticModel);

                if (contractResolver.GetContractInvocation(operation.TargetMethod, out var contractMethod))
                {
                    if (contractMethod.IsPostcondition())
                    {
                        var nodeToRemove = operation.Syntax.Parent;
                        nodesToRemove.Add(nodeToRemove);
                    }
                    else
                    {
                        var (source, replacement) = GetFluentContractsReplacements(root, contractResolver, operation, contractMethod);
                        nodeTranslationMap[source] = replacement;
                    }
                }
            }

            root = root.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia);

            root = root.ReplaceNodes(nodeTranslationMap.Keys, (source, temp) => nodeTranslationMap[source]);
            root = SyntaxTreeUtilities.ReplaceNamespaceUsings(root,
                originalNamespace: FluentContractNames.OldRuntimeContractsNamespace,
                newNamespace: FluentContractNames.FluentContractsNamespace);

            return document.WithSyntaxRoot(root);
        }

        private static async Task<Document> FixDocumentAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken token)
        {
            // Ensure that diagnostics for this document are always in document location
            // order.  This provides a consistent and deterministic order for fixers
            // that want to update a document.
            // Also ensure that we do not pass in duplicates by invoking Distinct.
            // See https://github.com/dotnet/roslyn/issues/31381, that seems to be causing duplicate diagnostics.
            var filteredDiagnostics =
                diagnostics
                    .Distinct()
                    .ToList();
            filteredDiagnostics
                    .Sort((d1, d2) => d1.Location.SourceSpan.Start - d2.Location.SourceSpan.Start);

            // PERF: Do not invoke FixAllAsync on the code fix provider if there are no diagnostics to be fixed.
            if (filteredDiagnostics.Count == 0)
            {
                return document;
            }

            var root = await document.GetSyntaxRootAsync(token);
            var declarations = filteredDiagnostics.Select(d => (InvocationExpressionSyntax)root.FindNode(d.Location.SourceSpan)).ToList();
            return await UseFluentContractsOrRemovePostconditionsAsync(document, declarations, token);
        }

        private static SyntaxNode UseFluentContracts(
            SyntaxNode root,
            ContractResolver contractResolver,
            IInvocationOperation operation,
            ContractMethodNames contractMethod)
        {
            var (source, finalNode) = GetFluentContractsReplacements(root, contractResolver, operation, contractMethod);
            return root.ReplaceNode(source, finalNode);
        }

        private static (SyntaxNode source, SyntaxNode destination) GetFluentContractsReplacements(
            SyntaxNode root, 
            ContractResolver contractResolver, 
            IInvocationOperation operation,
            ContractMethodNames contractMethod)
        {
            var invocationExpression = operation.Syntax;
            var targetMethodName = GetTargetMethod(contractMethod);

            // Gettng the original predicate.
            var predicateArgument = (ArgumentSyntax)operation.Arguments[0].Syntax;
                
            ArgumentSyntax? extraForAllArgument = null;
            int messageArgumentIndex = 1;
            // We need to mutate it for the cases like RequiresNotNull and RequiresNotNullOrEmpty

            if (contractMethod.IsNullCheck())
            {
                predicateArgument =
                    Argument(
                        // Changing NotNull(x) to x != null
                        BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            predicateArgument.Expression,
                            LiteralExpression(SyntaxKind.NullLiteralExpression)))
                    .NormalizeWhitespace();
            }
            else if (contractMethod.IsNotNullOrEmpty() || contractMethod.IsNotNullOrWhiteSpace())
            {
                var stringMethodName = contractMethod.IsNotNullOrEmpty() ? nameof(string.IsNullOrEmpty) : nameof(string.IsNullOrWhiteSpace);

                // Targeting a full framework can cause an issue for null-ness analysis
                // because string.IsNullOrEmpty and string.IsNullOrWhiteSpace is not annotated with any attributes.
                // It means that the compiler can't recognize that the following code is correct and still emit the warning:
                // if (!string.IsNullOrEmpty(str)) return str.Length;
                predicateArgument =
                    Argument(
                        PrefixUnaryExpression(
                            SyntaxKind.LogicalNotExpression,
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                                    IdentifierName(stringMethodName)))
                                .WithArgumentList(
                                    ArgumentList(SingletonSeparatedList(predicateArgument))))
                        ).NormalizeWhitespace();
            }
            else if (contractMethod.IsForAll())
            {
                extraForAllArgument = (ArgumentSyntax)operation.Arguments[1].Syntax;
                messageArgumentIndex++;
            }

            var originalMessageArgument = operation.Arguments[messageArgumentIndex];
            Func<string> nonDefaultArgument =
                () => contractMethod.IsForAll()
                    ? operation.Arguments[1].Syntax.ToFullString()
                    : predicateArgument.ToFullString();

            // Using an original message if provided.
            // Otherwise using a predicate as the new message.
            var messageArgument =
                originalMessageArgument.IsImplicit == false
                ? (ArgumentSyntax)originalMessageArgument.Syntax
                : Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(nonDefaultArgument())));

            var arguments = 
                new SeparatedSyntaxList<ArgumentSyntax>()
                .Add(predicateArgument)
                .AddIfNotNull(extraForAllArgument);

            // Generating Contract.ContractMethod(predicate)?.IsTrue(message)
            var contractCall =
                ExpressionStatement(
                    ConditionalAccessExpression(
                        // Contract.Requires(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(FluentContractNames.ContractClassName),
                                IdentifierName(targetMethodName)))
                        // (predicate)
                        .WithArgumentList(
                            ArgumentList(arguments)),
                        // ?.IsTrue(message)
                        InvocationExpression(
                                    MemberBindingExpression(
                                        IdentifierName(FluentContractNames.CheckMethodName)))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(messageArgument)))
                                )
                    );

            var trivia = invocationExpression.Parent.GetLeadingTrivia();
            var finalNode = contractCall.WithLeadingTrivia(trivia);
            return (invocationExpression.Parent, finalNode);
        }

        private static string GetTargetMethod(ContractMethodNames contractMethod)
        {
            return contractMethod switch
            {
                Requires => nameof(Requires),
                RequiresNotNull => nameof(Requires),
                RequiresNotNullOrEmpty => nameof(Requires),
                RequiresNotNullOrWhiteSpace => nameof(Requires),

                RequiresDebug => nameof(RequiresDebug),

                Assert => nameof(Assert),
                AssertNotNull => nameof(Assert),
                AssertNotNullOrEmpty => nameof(Assert),
                AssertNotNullOrWhiteSpace => nameof(Assert),

                AssertDebug => nameof(AssertDebug),

                Assume => nameof(Assert),
                
                _ => contractMethod.ToString(),
            };
        }

        private class FixAll : FixAllProvider
        {
            public static FixAll Instance { get; } = new FixAll();

            /// <inheritdoc />
            public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
            {
                yield return FixAllScope.Document;
                yield return FixAllScope.Project;
            }

            /// <inheritdoc />
            public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                var documentsAndDiagnosticsToFixMap = await FixAllContextHelper.GetDocumentDiagnosticsToFixAsync(fixAllContext);

                var updatedDocumentTasks = 
                    documentsAndDiagnosticsToFixMap
                        .Where(kvp => kvp.Key != null)
                        .Select(
                            kvp => FixDocumentAsync(kvp.Key!, kvp.Value, fixAllContext.CancellationToken))
                        .ToList();

                await Task.WhenAll(updatedDocumentTasks).ConfigureAwait(false);

                var currentSolution = fixAllContext.Solution;
                foreach (var task in updatedDocumentTasks)
                {
                    // 'await' the tasks so that if any completed in a canceled manner then we'll
                    // throw the right exception here.  Calling .Result on the tasks might end up
                    // with AggregateExceptions being thrown instead.
                    var updatedDocument = await task.ConfigureAwait(false);
                    currentSolution = currentSolution.WithDocumentSyntaxRoot(
                        updatedDocument.Id,
                        await updatedDocument.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false));
                }

                return new SolutionChangeAction(Title, _ => Task.FromResult(currentSolution));
            }
        }
    }
}
