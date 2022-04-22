using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RuntimeContracts.Analyzer.Core;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static RuntimeContracts.Analyzer.Core.ContractMethodNames;
using System.Collections.Generic;
using RuntimeContracts.Analyzer.Utilities;

namespace RuntimeContracts.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DoNotUseFluentContractsCodeFixProvider)), Shared]
public class DoNotUseFluentContractsCodeFixProvider : CodeFixProvider
{
    private const string Title = "Use regular contract API.";

    /// <inheritdoc />
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DoNotUseFluentContractsAnalyzer.DiagnosticId);

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

    private static async Task<Document> UseRegularContractsApiAsync(
        Document document, 
        List<ConditionalAccessExpressionSyntax> invocationExpressions, 
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        if (semanticModel is null)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root is null)
        {
            return document;
        }

        var nodeTranslationMap = new Dictionary<SyntaxNode, SyntaxNode>();
        var contractResolver = new ContractResolver(semanticModel.Compilation);

        foreach (var invocationExpression in invocationExpressions)
        {
            var operation = semanticModel.GetOperation(invocationExpression);
            if (contractResolver.TryParseFluentContractInvocation(operation, out var contractMethod, out var condition, out var message))
            {
                var replacementPair =
                    GetFluentContractsReplacements(operation.ThrowIfNull(), contractMethod, condition, message);
                nodeTranslationMap[replacementPair.source] = replacementPair.destination;
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
        if (root is null)
        {
            return document;
        }

        var declarations = filteredDiagnostics.Select(d => (ConditionalAccessExpressionSyntax)root.FindNode(d.Location.SourceSpan)).ToList();
        return await UseRegularContractsApiAsync(document, declarations, token);
    }

    private static (SyntaxNode source, SyntaxNode destination) GetFluentContractsReplacements(
        IOperation operation,
        ContractMethodNames contractMethod,
        ArgumentSyntax predicateSyntax,
        ArgumentSyntax messageSyntax)
    {
        var invocationExpression = operation.Syntax;
        var sourceNode = invocationExpression.Parent.ThrowIfNull();
            
        var arguments =
            new SeparatedSyntaxList<ArgumentSyntax>()
                .Add(predicateSyntax)
                .Add(messageSyntax);

        // Generating Contract.Check(predicate)?.Requires/Assert(message)
        var targetMethodName = GetTargetMethod(contractMethod);
        var contractCall =
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(FluentContractNames.ContractClassName),
                        IdentifierName(targetMethodName)))
                .WithArgumentList(ArgumentList(arguments)
                );

        var trivia = sourceNode.GetLeadingTrivia();
        var finalNode = contractCall.WithLeadingTrivia(trivia);
        return (invocationExpression, finalNode);
    }

    private static string GetTargetMethod(ContractMethodNames contractMethod)
    {
        return contractMethod.ToString();
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