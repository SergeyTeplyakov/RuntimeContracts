using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RuntimeContracts.Analyzer.Core;

namespace RuntimeContracts.Analyzer
{
    /// <summary>
    /// Analyzer warns when <code>Contract.AssertFailure()</code> is called without `throw`.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IncorrectUseOfAssertFailureAnalyzer : DiagnosticAnalyzer
    {
        /// <nodoc />
        public const string DiagnosticId = DiagnosticIds.IncorrectUseOfAssertFailureId;

        private static readonly string Title = "Incorrect usage of `Contract.AssertFailure`. Did you forget to use `throw`?.";
        private static readonly string Message = Title;

        private static readonly string Description = "`Contract.AssertFailure` returns an exception that should be thrown in order for the contract check to have an effect.";
        private const string Category = "CodeSmell";

        private const DiagnosticSeverity Severity = DiagnosticSeverity.Error;

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, Severity, isEnabledByDefault: true, description: Description);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterOperationAction(AnalyzeInvocationOperation, OperationKind.Invocation);
        }

        private void AnalyzeInvocationOperation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;
            var resolver = new ContractResolver(invocation.SemanticModel);

            if (resolver.IsFluentContractInvocation(invocation.TargetMethod, ContractMethodNames.AssertFailure) || 
                resolver.IsContractInvocation(invocation.TargetMethod, ContractMethodNames.AssertFailure))
            {
                if (invocation.Parent is IExpressionStatementOperation)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
                }
            }
        }
    }
}
