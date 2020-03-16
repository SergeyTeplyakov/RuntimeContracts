//using System.Collections.Immutable;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Diagnostics;
//using Microsoft.CodeAnalysis.Operations;
//using RuntimeContracts.Analyzer.Core;
//#nullable enable

//namespace RuntimeContracts.Analyzer
//{
//    /// <summary>
//    /// The analyzers warns on null checks like <code>Contract.Requires(arg != null)</code> and "suggest" a fix to switch to <code>Contract.RequiresNotNull(arg)</code>
//    /// recognizable by the C# compiler nullability analyzers.
//    /// </summary>
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    public class UseSimplifiedNullCheckAnalyzer : DiagnosticAnalyzer
//    {
//        public static readonly string DiagnosticId = DiagnosticIds.UseSimplifiedNullCheckId;

//        private static readonly string Title = "Use simplified null check.";
//        private static readonly string Description = "Use simplified null check that the C# compiler is aware of.";
//        private const string Category = "Correctness";
//        private const DiagnosticSeverity Severity = DiagnosticSeverity.Info;

//        private static readonly DiagnosticDescriptor Rule = 
//            new DiagnosticDescriptor(DiagnosticId, Title, Title, Category, Severity, isEnabledByDefault: true, description: Description);

//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

//        public override void Initialize(AnalysisContext context)
//        {
//            context.EnableConcurrentExecution();
            
//            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            
//            context.RegisterOperationAction(AnalyzeMethodInvocation, OperationKind.Invocation);
//        }

//        private void AnalyzeMethodInvocation(OperationAnalysisContext context)
//        {
//            var invocation = (IInvocationOperation)context.Operation;
//            var resolver = new ContractResolver(invocation.SemanticModel);
//            var contractsForSimplification =
//                ContractMethodNames.Assert | ContractMethodNames.Assume | ContractMethodNames.Requires;
//            // Looking for contract methods based  on 'RuntimeContracts' package.
//            if (resolver.IsContractInvocation(invocation.TargetMethod, contractsForSimplification)
//                && invocation.Arguments.Length > 0)
//            {
//                // Then looking for null checks.
//                var condition = invocation.Arguments[0]; // the first argument is a predicate.
//                if (condition.Value is IBinaryOperation binary && IsNullCheck(binary, out _))
//                {
//                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
//                }
//                else if (condition.Value is IUnaryOperation unary && IsNullOrEmptyCheck(unary, out _))
//                {
//                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
//                }
//            }

//            //var contractMethods2 = ContractMethodNames.Assume | ContractMethodNames.AllAsserts | ContractMethodNames.AllRequires;
//            //// Looking for contract methods based  on 'RuntimeContracts' package.
//            //if (resolver.IsContractInvocation(invocation.TargetMethod, contractMethods2) 
//            //    && invocation.Arguments.Length > 2)
//            //{
//            //    // Checking that the message is not provided.
//            //    if (invocation.Arguments[1].IsImplicit)
//            //    {
//            //        context.ReportDiagnostic(Diagnostic.Create(ProvideMessageAnalyzer.Rule, invocation.Syntax.GetLocation()));
//            //    }
//            //}
            

//        }

//        public static bool IsNullCheck(IBinaryOperation binary, out IOperation? nullableOperand)
//        {
//            nullableOperand = null;

//            if (binary.OperatorKind == BinaryOperatorKind.NotEquals && 
//                (IsNullCheck(binary.LeftOperand, binary.RightOperand, out nullableOperand, out _) ||
//                 IsNullCheck(binary.RightOperand, binary.LeftOperand, out nullableOperand, out _)))
//            {
//                return true;
//            }

//            return false;
//        }
//        public static bool IsNullOrEmptyCheck(IUnaryOperation unary, out IOperation? nullableOperand)
//        {
//            nullableOperand = null;

//            if (unary.OperatorKind == UnaryOperatorKind.Not &&
//                unary.Operand is IInvocationOperation operation)
//            {
//                if (operation.TargetMethod.ContainingType.SpecialType == SpecialType.System_String &&
//                    (operation.TargetMethod.Name == nameof(string.IsNullOrEmpty) ||
//                     operation.TargetMethod.Name == nameof(string.IsNullOrWhiteSpace)))
//                {
//                    nullableOperand = operation.Arguments[0];
//                    return true;
//                }
//            }
            
//            return false;
//        }

//        private static bool IsNullCheck(IOperation lhs, IOperation rhs, out IOperation? nullableOperand, out ISymbol? nullCompareSymbol)
//        {
//            if (IsNullLiteral(UnwrapImplicitConversion(rhs)))
//            {
//                nullableOperand = lhs;
//                // this is an expression like 'foobar' != null
//                nullCompareSymbol = UnwrapImplicitConversion(lhs) switch
//                    {
//                        IParameterReferenceOperation pr => (ISymbol)pr.Parameter,
//                        ILocalReferenceOperation lr => lr.Local,
//                        IFieldReferenceOperation fr => fr.Field,
//                        IPropertyReferenceOperation property => property.Property,
//                        IInvocationOperation invocation => invocation.TargetMethod,
//                        _ => null,
//                    };

//                return nullCompareSymbol != null;
//            }

//            nullableOperand = null;
//            nullCompareSymbol = null;
//            return false;
//        }

//        protected static IOperation UnwrapImplicitConversion(IOperation operation)
//            => operation is IConversionOperation conversion && conversion.IsImplicit
//                ? conversion.Operand
//                : operation;

//        private static bool IsNullLiteral(IOperation operand)
//            => operand is ILiteralOperation literal &&
//               literal.ConstantValue.HasValue &&
//               literal.ConstantValue.Value == null;
//    }
//}
