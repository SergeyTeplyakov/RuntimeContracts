using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RuntimeContracts.Analyzer.Core;
using static RuntimeContracts.Analyzer.Core.ContractMethodNames;
#nullable enable

namespace RuntimeContracts.Analyzer
{
    /// <summary>
    /// Analyzer emits a diagnostic that allows using new fluent API.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseFluentContractsAnalyzer : DiagnosticAnalyzer
    {
        /// <nodoc />
        public const string DiagnosticId = DiagnosticIds.UseFluentContractsId;

        private static readonly string Title = "Use fluent API for preconditions/assertions.";
        private static readonly string MessageFormat = "Use fluent API for preconditions/assertions.";
        private static readonly string Description = "Fluent API allows constructing custom assertion messages with 0 cost at runtime.";
        private const string Category = "Correctness";
        
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Info;
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, Severity, isEnabledByDefault: true, description: Description);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc />
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

            if (resolver.IsContractInvocation(invocation,
                AllAsserts | AllRequires | Assume | Ensures | EnsuresOnThrow))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
