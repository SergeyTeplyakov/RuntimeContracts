using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.DoNotUseComputedStringAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace RuntimeContracts.Analyzer.Test
{
    [TestClass]
    public class DoNotUseComputedStringAnalyzerTest
    {
        [TestMethod]
        public async Task FailsWhenAssertWithInterpolatedMessage()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        [|Contract.Assert(s != null, $""String {s} is not null."")|];
                    }
                }
            }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task FailsWhenAssertWithStringConcat()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        [|Contract.Assert(s != null, ""String "" + s + ""is not null."")|];
                    }
                }
            }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task NoWarnsWhenAssertWithStringConcatWithoutExpressions()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(s != null, ""String is not null."" + ""Another"");
                    }
                }
            }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task NoWarnWhenAssertWithLiteral()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(s != null, $""String s is not null."");
                    }
                }
            }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task NoWarnWhenAssertWhenTheFirstArgumentIsFalse()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(false, $""String {s} is not null."");
                    }
                }
            }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task NoWarnWhenAssertWithInterpolatedStringWithoutCaptures()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(s != null, $""String s is not null."");
                    }
                }
            }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
