using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
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

                context.RegisterOperationAction(context => AnalyzeInvocation(context, resolver), OperationKind.Invocation);
            });
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context, ContractResolver resolver)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (resolver.IsStandardContractInvocation(invocation.TargetMethod))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
