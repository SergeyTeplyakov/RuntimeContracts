using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RuntimeContracts.Analyzer.Core;
#nullable enable

namespace RuntimeContracts.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseComputedStringAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RA002";

        private static readonly string Title = "Do not compute message programmatically for performance reasons.";
        private static readonly string Description = "String computation may be expensive and can cause perf issues when assertion is used on hot paths.";
        private const string Category = "Performance";
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, Title, Category, Severity, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var resolver = new ContractResolver(context.SemanticModel);
            if (resolver.IsContractInvocation(invocation) && invocation.ArgumentList.Arguments.Count == 2)
            {
                var firstArgument = invocation.ArgumentList.Arguments[0];
                if (firstArgument.Expression is LiteralExpressionSyntax les && les.Kind() == SyntaxKind.FalseLiteralExpression)
                {
                    // This is Assert(false, ....). The second argument may be in any form in this case.
                    return;
                }

                var messageExpression = invocation.ArgumentList.Arguments[1];
                if (messageExpression.Expression is InterpolatedStringExpressionSyntax ise &&
                    ise.DescendantNodes().Any(n => n.Kind() == SyntaxKind.IdentifierName))
                {
                    // Found interpolated string with expressions inside
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
                    return;
                }

                if (messageExpression.Expression is BinaryExpressionSyntax bes &&
                    bes.OperatorToken.Kind() == SyntaxKind.PlusToken &&
                    (bes.Left is LiteralExpressionSyntax || bes.Right is LiteralExpressionSyntax) &&
                    bes.DescendantNodes().Any(n => n.Kind() == SyntaxKind.IdentifierName))
                {
                    // Found "literal" + expression
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
                    return;
                }
            }
        }
    }
}
