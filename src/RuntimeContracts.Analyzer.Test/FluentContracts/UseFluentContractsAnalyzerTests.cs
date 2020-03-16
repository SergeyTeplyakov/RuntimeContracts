using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuntimeContracts.Analyzer.Test;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.UseFluentContractsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace RuntimeContracts.Analyzer.FluentContracts.Test
{
    [TestClass]
    public class UseFluentContractsAnalyzerTests
    {
        [TestMethod]
        public async Task WarnWhenResultOfRequiresIsNotEnforcedByIsTrueCall()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        [|Contract.Requires(s != null)|];
                        [|Contract.Assert(s != null)|];
                        
                        [|Contract.RequiresDebug(s != null)|];
                        [|Contract.AssertDebug(s != null)|];

                        [|Contract.RequiresForAll(c, e => e != null)|];

                        [|Contract.Assume(s != null)|];
                        [|Contract.Ensures(s != null)|];
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
