using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RuntimeContracts.Analyzer.Core;
#nullable enable

namespace RuntimeContracts.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotUseStandardContractAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule => DiagnosticIds.RA001;
    
    public static string DiagnosticId => Rule.Id;

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