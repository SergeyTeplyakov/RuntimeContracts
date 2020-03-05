using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RuntimeContracts.Analyzer.Core;
using static RuntimeContracts.Analyzer.Core.ContractMethodNames;
#nullable enable

namespace RuntimeContracts.Analyzer
{
    /// <summary>
    /// An analyzer that emit a diagnostic when an assertion doesn't have a message.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ProvideMessageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = Diagnostics.NoAssertionMessageId;

        private static readonly string Title = "Assertion is missing a custom message.";
        private static readonly string Description = "Lack of user-defined message may complicate a post-mortem analysis when the code is in flux.";
        private const string Category = "Usablity";
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Info;

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, Title, Category, Severity, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            
            context.RegisterOperationAction(AnalyzeMethodInvocation, OperationKind.Invocation);
        }

        private void AnalyzeMethodInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;
            var resolver = new ContractResolver(invocation.SemanticModel);

            // We do care only about the following methods (and not about AssertNotNull, Invariants and Ensures)
            var contractMethods = Assume | AllAsserts | AllRequires;
            // Looking for contract methods based  on 'RuntimeContracts' package.
            if (resolver.IsContractInvocation(invocation.TargetMethod, contractMethods))
            {
                // Checking that the message is not provided.
                if (invocation.Arguments.Length > 2 && 
                    invocation.Arguments[1].IsImplicit)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
                }
            }
        }
    }
}
