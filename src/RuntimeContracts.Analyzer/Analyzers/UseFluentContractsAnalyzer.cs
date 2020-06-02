using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RuntimeContracts.Analyzer.Core;
using static RuntimeContracts.Analyzer.Core.ContractMethodNames;
#nullable enable

namespace RuntimeContracts.Analyzer
{
    /// <summary>
    /// Analyzer emits a diagnostic that allows using new fluent API.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseFluentContractsAnalyzer : DiagnosticAnalyzer
    {
        /// <nodoc />
        public const string DiagnosticId = DiagnosticIds.UseFluentContractsId;

        private static readonly string Title = "Use fluent API for preconditions/assertions to avoid runtime overhead.";
        private static readonly string MessageFormat = Title;
        private static readonly string Description = "Fluent API allows constructing custom assertion messages with no allocations if contract is not violated.";
        private const string Category = "Correctness";
        
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Info;
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, Severity, isEnabledByDefault: true, description: Description);

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

            var contracts = AllAsserts | AllRequires | Assume | Ensures | EnsuresOnThrow;
            
            // Excluding ForAll and postconditions.
            contracts &= ~(RequiresForAll | Postconditions);
            
            if (resolver.IsContractInvocation(invocation.TargetMethod, contracts))
            {
                if (AssertionMessageConstructedProgrammatically(invocation))
                {
                    var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool AssertionMessageConstructedProgrammatically(IInvocationOperation invocation)
        {
            var messageExpression = invocation.Arguments[1];
            if (messageExpression.Value is ILiteralOperation)
            {
                return false;
            }

            // Its ok to reference a string parameter
            if (messageExpression.Value is IParameterReferenceOperation parameterReference &&
                parameterReference.Parameter.Type.SpecialType == SpecialType.System_String)
            {
                return false;
            }
            
            // Or a local variable of type string
            if (messageExpression.Value is ILocalReferenceOperation localReference &&
                localReference.Type.SpecialType == SpecialType.System_String)
            {
                return false;
            }

            // This is something like: "Msg1" + "Msg2" etc
            var valueChildren = messageExpression.Value.Children;

            if (valueChildren.All(e => e is ILiteralOperation) && valueChildren.Any())
            {
                return false;
            }

            return true;
        }
    }
}
