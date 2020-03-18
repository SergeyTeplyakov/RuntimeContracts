using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RuntimeContracts.Analyzer.Core;

namespace RuntimeContracts.Analyzer
{
    /// <summary>
    /// Analyzer warns when the result of a contract check made using fluent API is not observerd.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FluentAssertionResultIsNotObservedAnalyzer : DiagnosticAnalyzer
    {
        /// <nodoc />
        public const string DiagnosticId = DiagnosticIds.FluentAssertionResultIsNotObserved;

        private static readonly string Title = $"The result of a contract call is not checked by calling '?.{FluentContractNames.CheckMethodName}()'.";
        private static readonly string Message = $"The result of {{0}} is not checked by calling '?.{{FluentContractNames.CheckMethodName}}()' extension method.";

        private static readonly string Description = $"Contract checks are performed in two stages and is no-op if the assertion is not followed by '?.{FluentContractNames.CheckMethodName}' call.";
        private const string Category = "CodeSmell";

        private const DiagnosticSeverity Severity = DiagnosticSeverity.Error;

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, Severity, isEnabledByDefault: true, description: Description);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <nodoc />
        public FluentAssertionResultIsNotObservedAnalyzer()
        {
        }

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

            // Looking for contract methods based  on 'RuntimeContracts' package.
            if (resolver.IsFluentContractInvocation(invocation.TargetMethod))
            {
                var assertionKind = ContractResolver.ParseContractMethodName(invocation.TargetMethod.Name);
                string preconditionOrAssertion =
                    assertionKind == ContractMethodNames.Requires
                    ? "a precondition"
                    : "an assertion";

                // The parent of this invocation is a statement, i.e. we're seeing
                // a standalone Contract.Requires(x > 0); case.
                if (invocation.Parent is IExpressionStatementOperation)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), preconditionOrAssertion));
                }
                else if (invocation.Parent is IInvocationOperation parentInvocation)
                {
                    // It can be Contract.Requires(x > 0).ToString() or even 
                    // Contract.Requires(x > 0)?.MyOwnExtensionMethod()
                    // and we should warn for all the other cases but '?.IsTrue'
                    reportDiagnosticForWrongInvocation(parentInvocation);
                }
                else if (invocation.Parent is IConditionalAccessOperation conditionalAccess
                    && conditionalAccess.WhenNotNull is IInvocationOperation accessInvocation)
                {
                    reportDiagnosticForWrongInvocation(accessInvocation);
                }

                void reportDiagnosticForWrongInvocation(IInvocationOperation operation)
                {
                    // It can be Contract.Requires(x > 0).ToString() or even 
                    // Contract.Requires(x > 0)?.MyOwnExtensionMethod()
                    // and we should warn for all the other cases but '?.IsTrue'
                    if (!resolver.IsFluentContractCheck(operation.TargetMethod))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), preconditionOrAssertion));
                    }
                }
            }
        }
    }
}
