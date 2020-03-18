using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.ProvideMessageAnalyzer,
    RuntimeContracts.Analyzer.GenerateMessageCodeFixProvider>;

namespace RuntimeContracts.Analyzer.Test
{
    [TestClass]
    public class GenerateMessageForFluentAPICodeFixProvider
    {
        [TestMethod]
        public async Task FixForRequires()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        [|Contract.Requires(s?.Length > 1)|];
                        [|Contract.RequiresNotNull(s)|];
                        [|Contract.RequiresNotNullOrEmpty(s)|];
                        [|Contract.RequiresNotNullOrWhiteSpace(s)|];
                    }
                }
            }";

            var fixedTest = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Requires(s?.Length > 1, ""s?.Length > 1"");
                        Contract.RequiresNotNull(s, ""s is not null"");
                        Contract.RequiresNotNullOrEmpty(s, ""s is not null or empty"");
                        Contract.RequiresNotNullOrWhiteSpace(s, ""s is not null or whitespace"");
                    }
                }
            }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8,
                FixedState = { Sources = { fixedTest } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task FixForAsserts()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    private static string Foo(string s) => null;
                    public TypeName(string s)
                    {
                        [|Contract.Assert(s?.Length > 1)|];
                        [|Contract.AssertNotNull(Foo(s))|];
                        [|Contract.AssertNotNullOrEmpty(Foo(s))|];
                        [|Contract.AssertNotNullOrWhiteSpace(s)|];
                    }
                }
            }";

            var fixedTest = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    private static string Foo(string s) => null;
                    public TypeName(string s)
                    {
                        Contract.Assert(s?.Length > 1, ""s?.Length > 1"");
                        Contract.AssertNotNull(Foo(s), ""Foo(s) is not null"");
                        Contract.AssertNotNullOrEmpty(Foo(s), ""Foo(s) is not null or empty"");
                        Contract.AssertNotNullOrWhiteSpace(s, ""s is not null or whitespace"");
                    }
                }
            }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8,
                FixedState = { Sources = { fixedTest } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task FixForAssumes()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        [|Contract.Assume(s?.Length > 1)|];
                    }
                }
            }";

            var fixedTest = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assume(s?.Length > 1, ""s?.Length > 1"");
                    }
                }
            }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8,
                FixedState = { Sources = { fixedTest } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }
    }
}
