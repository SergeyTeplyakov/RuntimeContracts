using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RuntimeContracts.Analyzer.Core;

namespace RuntimeContracts.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseStandardContractAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RA001";

        private static readonly string Title = "Do not use System.Diagnostics.Contract class.";
        private static readonly string MessageFormat = "Do not use System.Diagnostics.Contract class.";
        private static readonly string Description = "System.Diagnostics.Contract class can be used only with ccrewrite enabled.";
        private const string Category = "Correctness";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var resolver = new ContractResolver(context.SemanticModel);

            if (resolver.IsStandardContractInvocation(invocation))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
