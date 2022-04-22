using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RuntimeContracts.Analyzer.Core;
using static RuntimeContracts.Analyzer.Core.ContractMethodNames;

#nullable enable

namespace RuntimeContracts.Analyzer;

/// <summary>
/// Fluent API is obsolete with new interpolated string improvements in C# 10.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotUseFluentContractsAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule => DiagnosticIds.RA009;
    /// <nodoc />
    public static readonly string DiagnosticId = Rule.Id;

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(context =>
        {
            var resolver = new ContractResolver(context.Compilation);

            context.RegisterOperationAction(context => AnalyzeOperation(context, resolver), OperationKind.Invocation);
        });
    }

    private void AnalyzeOperation(OperationAnalysisContext context, ContractResolver resolver)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (resolver.IsContractInvocation(invocation.TargetMethod, AllFluentContracts))
        {
            // Emitting diagnostics for the statement not for just an invocation.
            var diagnostic = Diagnostic.Create(Rule, invocation.Parent?.Syntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}