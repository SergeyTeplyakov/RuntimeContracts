using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.IncorrectUseOfAssertFailureAnalyzer,
    RuntimeContracts.Analyzer.UseRuntimeContractsCodeFixProvider>;

namespace RuntimeContracts.Analyzer.Test
{
    [TestClass]
    public class IncorrectUseOfAssertFailureAnalyzerTests
    {
        [TestMethod]
        public async Task FailForOldContracts()
        {
            var test = @"using System.Diagnostics.ContractsLight;
    
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public TypeName(string s)
            {
                [|Contract.AssertFailure()|];
                [|Contract.AssertFailure(""Message"")|];
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task FailForFluentContracts()
        {
            var test = @"using System.Diagnostics.FluentContracts;
    
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public TypeName(string s)
            {
                [|Contract.AssertFailure()|];
                [|Contract.AssertFailure(""Message"")|];
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }
        
        [TestMethod]
        public async Task NoWarnForFluentContractsWhenObserved()
        {
            var test = @"using System.Diagnostics.FluentContracts;
    
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public TypeName(string s)
            {
                throw Contract.AssertFailure();
            }

            public void Foo()
            {
                TakesException(Contract.AssertFailure());
            }

            private static void TakesException(System.Exception e) {}
        }
    }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }
    }
}
