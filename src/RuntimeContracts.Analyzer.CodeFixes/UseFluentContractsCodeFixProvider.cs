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
using RuntimeContracts.Analyzer.Core;
using Microsoft.CodeAnalysis.CSharp;

using System;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static RuntimeContracts.Analyzer.Core.ContractMethodNames;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Shared.Extensions;

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
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc />
        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c =>
                        FixDocumentAsync(context.Document, context.Diagnostics, c),
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

            foreach (var invocationExpression in invocationExpressions)
            {
                var operation = (IInvocationOperation)semanticModel.GetOperation(invocationExpression);
                var contractResolver = new ContractResolver(semanticModel.Compilation);

                if (contractResolver.GetContractInvocation(operation.TargetMethod, out var contractMethod))
                {
                    var (source, replacement) = GetFluentContractsReplacements(operation, contractMethod, semanticModel, cancellationToken);
                    nodeTranslationMap[source] = replacement;
                }
            }

            root = root.ReplaceNodes(nodeTranslationMap.Keys, (source, temp) => nodeTranslationMap[source]);

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

        private static (SyntaxNode source, SyntaxNode destination) GetFluentContractsReplacements(
            IInvocationOperation operation,
            ContractMethodNames contractMethod,
            SemanticModel semanticModel,
            CancellationToken token)
        {
            var invocationExpression = operation.Syntax;

            // Getting the original predicate.
            var predicateArgumentOperation = operation.Arguments[0];
            var predicateArgument = (ArgumentSyntax)predicateArgumentOperation.Syntax;
                
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

            // Detecting the following case:
            // if (predicate is false) {Contract.Assert(false, complicatedMessage);}
            var sourceNode = invocationExpression.Parent;
            if (predicateArgumentOperation.Value is ILiteralOperation lo
                && lo.ConstantValue.HasValue && lo.ConstantValue.Value.Equals(false))
            {
                // this is Assert(false) case.
                if (operation.Parent?.Parent?.Parent is IConditionalOperation conditional
                    && conditional.WhenFalse == null
                    && conditional.WhenTrue.Children.Count() == 1)
                {
                    // The contract is inside the if block with a single statement.
                    sourceNode = conditional.Syntax;

                    var negatedCondition = (ExpressionSyntax?)SyntaxGeneratorExtensions.Negate(conditional.Condition.Syntax, semanticModel,  token);
                    if (negatedCondition != null)
                    {
                        predicateArgument = Argument(negatedCondition);
                    }
                }
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

            // Generating Contract.Check(predicate)?.Requires/Assert(message)

            var targetMethodName = GetTargetMethod(contractMethod);
            var checkMethodName = GetCheckMethod(contractMethod);
            var contractCall =
                ExpressionStatement(
                    ConditionalAccessExpression(
                        // Contract.Requires(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(FluentContractNames.ContractClassName),
                                IdentifierName(checkMethodName)))
                        // (predicate)
                        .WithArgumentList(
                            ArgumentList(arguments)),
                        // ?.IsTrue(message)
                        InvocationExpression(
                                    MemberBindingExpression(
                                        IdentifierName(targetMethodName)))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(messageArgument)))
                                )
                    );

            var trivia = sourceNode.GetLeadingTrivia();
            var finalNode = contractCall.WithLeadingTrivia(trivia);
            return (sourceNode, finalNode);
        }

        private static string GetTargetMethod(ContractMethodNames contractMethod)
        {
            if (contractMethod.IsPrecondition())
            {
                return FluentContractNames.Requires;
            }

            return FluentContractNames.Assert;
        }
        
        private static string GetCheckMethod(ContractMethodNames contractMethod)
        {
            return contractMethod switch
            {
                RequiresDebug => FluentContractNames.CheckDebugMethodName,
                AssertDebug => FluentContractNames.CheckDebugMethodName,
                _ => FluentContractNames.CheckMethodName,
            };
        }
    }
}
