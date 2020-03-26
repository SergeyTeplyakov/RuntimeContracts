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
        public async Task Warn_On_String_Format()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        [|Contract.Requires(s != null, string.Format(""{0}"", s))|];
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
        public async Task No_Warn_On_Local_String_Or_Argument()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        Contract.Requires(s != null, s);
                        Contract.Assert(s != null, s);

                        var s2 = s;
                        Contract.Assert(s2 != null, s2);
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
        public async Task No_Warn_On_Ensures()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        Contract.Ensures(s != null, s + 42);
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
        public async Task Warn_On_Interpolated_String()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        [|Contract.Requires(s != null, ""Value"" + s)|];
                        [|Contract.Requires(s != null, $""{s}"")|];
                        Contract.Assert(s != null);
                        [|Contract.Assert(s != null, $""{s}"")|];
                        
                        Contract.RequiresDebug(s != null);
                        [|Contract.RequiresDebug(s != null, $""{s}"")|];
                        Contract.AssertDebug(s != null);
                        [|Contract.AssertDebug(s != null, $""{s}"")|];

                        // No warning for for-all now. it is not a common check.
                        Contract.RequiresForAll(c, e => e != null, $""{s}"");

                        Contract.Assume(s != null);
                        [|Contract.Assume(s != null, $""{s}"")|];
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
        public async Task Warn_On_Method_Call()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, int n, string[] c)
                    {
                        [|Contract.Requires(s != null, n.ToString())|];
                        [|Contract.Requires(s != null, Msg())|];
                        [|Contract.Requires(s != null, MsgProp)|];
                        [|Contract.Requires(s != null, MsgProp)|];
                    }

                    private static string Msg() { return string.Empty; }
                    private static string MsgProp => string.Empty;
                }
            }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task Warn_On_Concatenated_Message()
        {
            var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        Contract.Requires(s != null, ""Value"");
                        Contract.Requires(s != null, ""Value"" + ""Another string value"");
                        [|Contract.Requires(s != null, ""Value"" + 5)|];
                        [|Contract.Requires(s != null, ""Value"" + MsgProp)|];
                    }

                    private static string MsgProp => string.Empty;
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
