//using System.Collections.Immutable;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Diagnostics;
//using RuntimeContracts.Analyzer.Core;
//#nullable enable

//namespace RuntimeContracts.Analyzer
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    public class RemovePostconditionAnalyzer : DiagnosticAnalyzer
//    {
//        public const string DiagnosticId = DiagnosticIds.UseFluentContractsId;

//        private static readonly string Title = "Remove postcondition because it is not supported at runtime.";
//        private static readonly string MessageFormat = "Remove postcondition because it is not supported at runtime.";
//        private static readonly string Description = "Postconditions require some form of code rewriting and they are not supported by the RuntimeContracts library.";
//        private const string Category = "Correctness";

//        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

//        public override void Initialize(AnalysisContext context)
//        {
//            context.EnableConcurrentExecution();
//            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

//            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.InvocationExpression);
//        }

//        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
//        {
//            var invocation = (InvocationExpressionSyntax)context.Node;

//            var resolver = new ContractResolver(context.SemanticModel);

//            if (resolver.IsContractInvocation(invocation, ContractMethodNames.Ensures))
//            {
//                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());

//                context.ReportDiagnostic(diagnostic);
//            }
//        }
//    }
//}
