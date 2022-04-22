using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RuntimeContracts.Analyzer.Core;

#nullable enable

namespace RuntimeContracts.Analyzer;

/// <summary>
/// An analyzer that warns against using special contracts like <code>Contract.RequiresNotNull(x)</code> and <code>Contract.AssertNotNull</code> etc.
/// </summary>
/// <remarks>
/// The analyzer doesn't warn against <code>Contract.RequiresNotNullOrEmpty(s)</code> because the full framework still doesn't have nullable annotations
/// for <code>string.IsNullOrEmpty</code> and switching to <code>Contract.Requires(!string.IsNullOrEmpty(s))</code> potentially may cause the nullability warnings.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotUseSimplifiedNullCheckAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => DiagnosticIds.RA007;
    public static string DiagnosticId => Rule.Id;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(context =>
        {
            var resolver = new ContractResolver(context.Compilation);

            context.RegisterOperationAction(context => AnalyzeMethodInvocation(context, resolver), OperationKind.Invocation);
        });
    }

    private void AnalyzeMethodInvocation(OperationAnalysisContext context, ContractResolver resolver)
    {
        var invocation = (IInvocationOperation)context.Operation;

        // We do care only about the following methods (and not about AssertNotNull, Invariants and Ensures)
        var contractMethods = ContractMethodNames.AssertNotNull | ContractMethodNames.RequiresNotNull;
            
        // Looking for contract methods based  on 'RuntimeContracts' package.
        if (resolver.IsContractInvocation(invocation.TargetMethod, contractMethods))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
        }
    }
}