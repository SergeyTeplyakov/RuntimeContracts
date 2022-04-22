using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.DoNotComputeMessageProgrammaticallyAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace RuntimeContracts.Analyzer.Test;

[TestClass]
public class DoNotComputeMessageProgrammaticallyAnalyzerTests
{
    [TestMethod]
    public async Task Warn_For_CSharp_10()
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
                        [|Contract.Assert(s != null, s + 1)|];
                    }
                }
            }";

        await new VerifyCS.Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10
        }.WithoutGeneratedCodeVerification().RunAsync();
    }
    
    [TestMethod]
    public async Task Do_Not_Warn_On_Missing_Message()
    {
        var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        Contract.Requires(s != null);
                    }
                }
            }";

        await new VerifyCS.Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10
        }.WithoutGeneratedCodeVerification().RunAsync();
    }
    
    [TestMethod]
    public async Task Do_Not_Warn_On_Interpolated_String()
    {
        var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        Contract.Requires(s != null, $""{s}"");
                    }
                }
            }";

        await new VerifyCS.Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10
        }.WithoutGeneratedCodeVerification().RunAsync();
    }
    
    [TestMethod]
    public async Task Do_Not_Warn_On_Assert_With_False()
    {
        // Technically it should be inside 'if' block,
        // but we know that 'Assert(false)' will fail anyways, so no point for emitting a warning anyways.
        var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        Contract.Requires(false, s + 1);
                        Contract.Assert(false, s + 1);
                    }
                }
            }";

        await new VerifyCS.Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10
        }.WithoutGeneratedCodeVerification().RunAsync();
    }
    
    [TestMethod]
    public async Task Do_Not_Warn_On_Requires_All()
    {
        var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        Contract.RequiresForAll(c, id => false);
                    }
                }
            }";

        await new VerifyCS.Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10
        }.WithoutGeneratedCodeVerification().RunAsync();
    }

    [TestMethod]
    public async Task Do_Not_Warn_On_String_Message()
    {
        var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        Contract.Requires(s != null, ""foo bar!"");
                    }
                }
            }";

        await new VerifyCS.Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10
        }.WithoutGeneratedCodeVerification().RunAsync();
    }

    [TestMethod]
    public async Task DoNot_Warn_For_CSharp_9()
    {
        var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        Contract.Requires(s != null, string.Format(""{0}"", s));
                        Contract.Assert(s != null, s + 1);
                    }
                }
            }";

        await new VerifyCS.Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9
        }.WithoutGeneratedCodeVerification().RunAsync();
    }
}