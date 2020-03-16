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

    internal class SolutionChangeAction : SimpleCodeAction
    {
        private readonly Func<CancellationToken, Task<Solution>> _createChangedSolution;

        public SolutionChangeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey = null)
            : base(title, equivalenceKey)
        {
            _createChangedSolution = createChangedSolution;
        }

        protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            return _createChangedSolution(cancellationToken);
        }
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseFluentContractsCodeFixProvider)), Shared]
    public class UseFluentContractsCodeFixProvider : CodeFixProvider
    {
        private class FixAll : FixAllProvider
        {
            public static FixAll Instance { get; } = new FixAll();
            
            public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
            {
                yield return FixAllScope.Document;
                yield return FixAllScope.Project;
            }

            public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                var documentsAndDiagnosticsToFixMap = await FixAllContextHelper.GetDocumentDiagnosticsToFixAsync(fixAllContext);
                
                var updatedDocumentTasks = documentsAndDiagnosticsToFixMap.Select(
                    kvp => FixDocumentAsync(kvp.Key, kvp.Value, fixAllContext));

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
                //var allDiagnostics = await fixAllContext.GetAllDiagnosticsAsync(fixAllContext.Document.Project);
                //var root = await fixAllContext.Document.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);

                //List<InvocationExpressionSyntax> declarations = new List<InvocationExpressionSyntax>();
                //foreach (var d in allDiagnostics)
                //{
                //    var node = root.FindNode(d.Location.SourceSpan);
                //    // For some reason in batch mode error locations can be off.
                //    // Using safe cast to avoid runtime failures.
                //    if (node is InvocationExpressionSyntax declaration)
                //    {
                //        declarations.Add(declaration);
                //    }
                //    else
                //    {

                //    }
                //    //var declaration = (InvocationExpressionSyntax)node;
                //}

                ////List<InvocationExpressionSyntax> declarations = allDiagnostics.Select(d => (InvocationExpressionSyntax)root.FindNode(d.Location.SourceSpan)).ToList();

                //// Register a code action that will invoke the fix.
                //return CodeAction.Create(
                //    title: Title,
                //    createChangedDocument: c => UseFluentContractsOrRemovePostconditions(fixAllContext.Document, declarations, c),
                //    equivalenceKey: Title);
            }
            private async Task<Document> FixDocumentAsync(
                Document document, ImmutableArray<Diagnostic> diagnostics, FixAllContext fixAllContext)
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
                
                var root = await document.GetSyntaxRootAsync(fixAllContext.CancellationToken);
                List<InvocationExpressionSyntax> declarations = filteredDiagnostics.Select(d => (InvocationExpressionSyntax)root.FindNode(d.Location.SourceSpan)).ToList();
                return await UseFluentContractsOrRemovePostconditions(document, declarations, fixAllContext.CancellationToken);
            }
        }

        private const string Title = "Use fluent API for contract validation.";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseFluentContractsAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            //return WellKnownFixAllProviders.BatchFixer;
            //return new FixAll();
            return FixAll.Instance;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            //context.
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            //System.Diagnostics.Debugger.Launch();
            var diagnostic = context.Diagnostics.First();

            // Looking for contract check.
            var declaration = (InvocationExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan);

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => UseFluentContractsOrRemovePostconditions(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> UseFluentContractsOrRemovePostconditions(
            Document document, 
            List<InvocationExpressionSyntax> invocationExpressions, 
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            Dictionary<SyntaxNode, SyntaxNode> map = new Dictionary<SyntaxNode, SyntaxNode>();

            //return await UseFluentContractsOrRemovePostconditions(
            //    document, invocationExpressions[0], cancellationToken);

            foreach (var invocationExpression in invocationExpressions)
            {
                var operation = (IInvocationOperation)semanticModel.GetOperation(invocationExpression);
                var contractResolver = new ContractResolver(semanticModel);

                if (contractResolver.GetContractInvocation(operation.TargetMethod, out var contractMethod))
                {
                    
                    if (contractMethod.IsPostcondition())
                    {
                        //root = RemovePostcondition(root, operation);
                    }
                    else
                    {
                        var (source, replacement) = GetFluentContractsReplacements(root, contractResolver, operation, contractMethod);
                        //sources.Add(source);
                        //replacements.Add(replacement);
                        map[source] = replacement;
                    }
                }
            }

            root = root.ReplaceNodes(map.Keys, (source, temp) => map[source]);
            root = SyntaxTreeUtilities.ReplaceNamespaceUsings(root,
                originalNamespace: FluentContractNames.OldRuntimeContractsNamespace,
                newNamespace: FluentContractNames.FluentContractsNamespace);

            return document.WithSyntaxRoot(root);
        }

        private static async Task<Document> UseFluentContractsOrRemovePostconditions(Document document, InvocationExpressionSyntax invocationExpression, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var operation = (IInvocationOperation)semanticModel.GetOperation(invocationExpression);
            var contractResolver = new ContractResolver(semanticModel);

            //var sources = new List<SyntaxNode>();
            //var replacements = new List<SyntaxNode>();
            Dictionary<SyntaxNode, SyntaxNode> map = new Dictionary<SyntaxNode, SyntaxNode>();

            if (contractResolver.GetContractInvocation(operation.TargetMethod, out var contractMethod))
            {
                var root = await document.GetSyntaxRootAsync(cancellationToken);
                if (contractMethod.IsPostcondition())
                {
                    root = RemovePostcondition(root, operation);
                }
                else
                {
                    var (source, replacement) = GetFluentContractsReplacements(root, contractResolver, operation, contractMethod);
                    //sources.Add(source);
                    //replacements.Add(replacement);
                    map[source] = replacement;
                }

                root = root.ReplaceNodes(map.Keys, (source, temp) => map[source]);
                root = SyntaxTreeUtilities.ReplaceNamespaceUsings(root,
                    originalNamespace: FluentContractNames.OldRuntimeContractsNamespace,
                    newNamespace: FluentContractNames.FluentContractsNamespace);

                //root = SyntaxTreeUtilities.AddNamespaceUsingsIfNeeded(root, FluentContractNames.FluentContractsNamespace);

                return document.WithSyntaxRoot(root);
            }

            return document;
        }

        private static SyntaxNode RemovePostcondition(
            SyntaxNode root,
            IInvocationOperation operation)
        {
            var invocationExpression = operation.Syntax;
            root = root.RemoveNode(invocationExpression.Parent, SyntaxRemoveOptions.KeepNoTrivia);

            return root;
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
    }

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
}
