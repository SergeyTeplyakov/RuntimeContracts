using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RuntimeContracts.Analyzer.Core;
#nullable enable

namespace RuntimeContracts.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseStandardContractAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = DiagnosticIds.DoNotUseStandardContractId;

        private static readonly string Title = "Do not use System.Diagnostics.Contract class.";
        private static readonly string MessageFormat = "Do not use System.Diagnostics.Contract class.";
        private static readonly string Description = "System.Diagnostics.Contract class can be used only with ccrewrite enabled.";
        private const string Category = "Correctness";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(context =>
            {
                var resolver = new ContractResolver(context.Compilation);

                context.RegisterSyntaxNodeAction(context => AnalyzeSyntax(context, resolver), SyntaxKind.InvocationExpression);
            });
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context, ContractResolver resolver)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (resolver.IsStandardContractInvocation(invocation, context.SemanticModel))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
