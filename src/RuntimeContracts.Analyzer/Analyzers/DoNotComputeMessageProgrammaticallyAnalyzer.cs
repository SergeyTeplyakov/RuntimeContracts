using System.Collections;
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
/// Warns when a contract message is computed programmatically and not using interpolated strings.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotComputeMessageProgrammaticallyAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule => DiagnosticIds.RA002;
        
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

        // Intentionally ignoring all the NotNull checks in this analyzer
        var contracts = Assert | AssertDebug | AssertFailure | Requires | RequiresDebug;

        if (resolver.IsContractInvocation(invocation.TargetMethod, contracts))
        {
            if (invocation.Arguments[0].Value is ILiteralOperation {ConstantValue.Value: false})
            {
                // Ignoring the cases like 'Contract.Assert(false, constructed message)'
                return;
            }

            // A bit tricky way to define if we call the version that takes an interpolated string or not
            var messageExpression = invocation.Arguments[1];
            if (messageExpression.Parameter?.Type.Name.Contains("InterpolatedString") == true)
            {
                // this is fine for sure, we call with an interpolated string argument.
                return;
            }
            
            // Ok to have message: "Msg" + "msg"
            if (messageExpression.Value is IBinaryOperation binaryOp && ContainsOnlyLiterals(binaryOp))
            {
                return;
            }

            // Ok to reference locals
            if (messageExpression.Value is ILocalReferenceOperation or IParameterReferenceOperation)
            {
                return;
            }

            if (messageExpression.Value.ConstantValue.HasValue)
            {
                // This is $"foo bar" or $"foo {nameof(x)}" cases.
                return;
            }

            if (messageExpression.Value is not IDefaultValueOperation &&
                messageExpression.Value is not ILiteralOperation &&
                // Warn only for C#10+
                LanguageVersionHelper.GetLanguageVersion(context.Operation) >= LanguageVersion.CSharp10)
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool ContainsOnlyLiterals(IBinaryOperation? binaryOperation)
    {
        if (binaryOperation is null)
        {
            return true;
        }

        if (binaryOperation.LeftOperand is not (ILiteralOperation or IBinaryOperation))
        {
            return false;
        }

        if (binaryOperation.RightOperand is not (ILiteralOperation or IBinaryOperation))
        {
            return false;
        }

        return (binaryOperation.LeftOperand is ILiteralOperation ||
                ContainsOnlyLiterals((binaryOperation.LeftOperand as IBinaryOperation)))
               &&
               (binaryOperation.RightOperand is ILiteralOperation ||
                ContainsOnlyLiterals((binaryOperation.RightOperand as IBinaryOperation)));
    }
}