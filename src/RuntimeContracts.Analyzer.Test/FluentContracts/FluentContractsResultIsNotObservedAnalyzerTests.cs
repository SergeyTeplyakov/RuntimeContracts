using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuntimeContracts.Analyzer.Test;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.FluentAssertionResultIsNotObservedAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace RuntimeContracts.Analyzer.FluentContracts.Test
{
    [TestClass]
    public class FluentContractsResultIsNotObservedAnalyzerTests
    {
        [TestMethod]
        public async Task WarnWhenResultOfRequiresIsNotEnforcedByRequiresOrAssert()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Check(s != null)?.Requires(s);

                        [|Contract.Check(s != null)|].ToString();
                        [|Contract.CheckDebug(s != null, ""Message"")|];
                    }
                }
            }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task WarnWhenRequiresIsNotCheckedButAnotherExtensionMethodIsCalled()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                public static class MyExtensions
                {
                    public static void MyTrue(this in AssertionFailure result) {}
                }

                class TypeName
                {
                    public TypeName(string s)
                    {
                        [|Contract.Check(s != null, ""Message"")|]?.MyTrue();
                    }
                }
            }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8
            }.WithoutGeneratedCodeVerification().RunAsync();
        }
    }
}
