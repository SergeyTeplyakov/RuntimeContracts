using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RuntimeContracts.Analyzer.Core;
using RuntimeContracts.Core;
using static RuntimeContracts.Analyzer.Core.ContractMethodNames;

#nullable enable

namespace RuntimeContracts.Analyzer;

/// <summary>
/// Fluent API is obsolete with new interpolated string improvements in C# 10.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnsupportedCSharpLanguageVersionAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule => DiagnosticIds.RA008;


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

        var contracts = AllAsserts | AllRequires;


        if (resolver.IsContractInvocation(invocation.TargetMethod, contracts))
        {
            if (InterpolationStringUsedForMessageConstruction(invocation) &&
                LanguageVersionHelper.GetLanguageVersion(context.Operation) < LanguageVersion.CSharp10)
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool InterpolationStringUsedForMessageConstruction(IInvocationOperation invocation)
    {
        var messageExpression = invocation.Arguments[1];
        return messageExpression.Value is IInterpolatedStringOperation;
    }
}