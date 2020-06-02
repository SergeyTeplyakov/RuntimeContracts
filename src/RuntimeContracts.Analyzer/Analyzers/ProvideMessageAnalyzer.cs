using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RuntimeContracts.Analyzer.Core;
#nullable enable

namespace RuntimeContracts.Analyzer
{
    /// <summary>
    /// An analyzer that emit a diagnostic when an assertion doesn't have a message.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ProvideMessageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = DiagnosticIds.ProvideMessageId;

        private static readonly string Title = "User-defined message is missing in a contract assertion.";
        private static readonly string Description = "Lack of user-defined message may complicate a post-mortem analysis when the code is in flux.";
        private const string Category = "Usability";
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Info;

        public static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, Title, Category, Severity, isEnabledByDefault: true, description: Description);

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
            var contractMethods = ContractMethodNames.Assume | ContractMethodNames.AllAsserts | ContractMethodNames.AllRequires;
            // Looking for contract methods based  on 'RuntimeContracts' package.
            if (resolver.IsContractInvocation(invocation.TargetMethod, contractMethods)
                && invocation.Arguments.Length > 2)
            {
                // Checking that the message is not provided.
                if (invocation.Arguments[1].IsImplicit)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
                }
            }
            else if (resolver.IsFluentContractInvocation(invocation.TargetMethod)
                && invocation.Parent is IConditionalAccessOperation conditionalAccess
                && conditionalAccess.WhenNotNull is IInvocationOperation checkInvocation
                && checkInvocation.Arguments.Length > 0)
            {
                // First argument for IsTrue is a struct. And the second one is the message
                if (checkInvocation.Arguments[1].IsImplicit)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Parent.Syntax.GetLocation()));
                }
            }
        }
    }
}
