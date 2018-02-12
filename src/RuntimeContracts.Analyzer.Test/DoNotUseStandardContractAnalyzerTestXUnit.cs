using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace RuntimeContracts.Analyzer.Test
{
    public class DoNotUseStandardContractAnalyzerTestXUnit : CodeFixVerifier
    {
        [Fact]
        public void FailsOnContractRequires2()
        {
            var test = @"
    using System.Diagnostics.Contracts;
    
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public TypeName(string s)
            {
                Contract.Requires(s != null);
            }
        }
    }";

            var diagnostic = GetFirstDiagnosticFor(test);
            Assert.Equal(diagnostic.Id, DoNotUseStandardContractAnalyzer.DiagnosticId);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RuntimeContractsAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseStandardContractAnalyzer();
        }
    }
}
