using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.ProvideMessageAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace RuntimeContracts.Analyzer.Test
{
    [TestClass]
    public class ProvideMessageAnalyzerTest
    {
        [TestMethod]
        public async Task WarnWhenNoMessageIsProvided()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    private string? _f;
                    private string? Prop => _f;
                    private string? MyFunc() => null;
                    public TypeName(string s)
                    {
                        [|Contract.RequiresNotNull(s)|];
                        [|Contract.RequiresNotNullOrEmpty(s)|];
                        [|Contract.RequiresNotNullOrWhiteSpace(s)|];

                        [|Contract.Assert(s != null && true)|];
                        System.Diagnostics.Contracts.Contract.Requires(s != null);
                        Contract.Assert(s != null, ""Message"");
                        string msg = string.Empty;
                        Contract.Assert(s != null, msg);
                        Contract.Assert(s != null, $""{msg}"");
                        
                        string s2 = s;
                        [|Contract.AssertNotNull(s2)|];
                        [|Contract.AssertNotNullOrEmpty(s2)|];
                        [|Contract.AssertNotNullOrWhiteSpace(s2)|];
                        [|Contract.AssertNotNull(MyFunc())|];
                        [|Contract.Assert(s2 != null)|];

                        [|Contract.Assert(_f != null)|];
                        [|Contract.Assume(_f != null)|];
                        [|Contract.Assert(Prop != null)|];
                        [|Contract.Assert(MyFunc() != null)|];
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
        public async Task WarnWhenNoMessageIsProvidedForFluentApi()
        {
            var test = @"using System.Diagnostics.FluentContracts;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    private string? _f;
                    private string? Prop => _f;
                    private string? MyFunc() => null;
                    public TypeName(string s)
                    {
                        [|Contract.Requires(s != null)?.IsTrue()|];
                        [|Contract.Requires(!string.IsNullOrEmpty(s))?.IsTrue()|];

                        [|Contract.Assert(s != null && true)?.IsTrue()|];
                        System.Diagnostics.Contracts.Contract.Requires(s != null);
                        Contract.Assert(s != null)?.IsTrue(""Message"");
                        string msg = string.Empty;
                        Contract.Assert(s != null)?.IsTrue(msg);
                        Contract.Assert(s != null)?.IsTrue($""{msg}"");
                        
                        string s2 = s;
                        [|Contract.Assert(!string.IsNullOrWhiteSpace(s2))?.IsTrue()|];
                        [|Contract.Assert(MyFunc() != null)?.IsTrue()|];
                        [|Contract.Assert(s2 != null)?.IsTrue()|];
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
