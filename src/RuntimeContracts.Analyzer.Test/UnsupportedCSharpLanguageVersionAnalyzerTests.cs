using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.UnsupportedCSharpLanguageVersionAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace RuntimeContracts.Analyzer.Test;

[TestClass]
public class UnsupportedCSharpLanguageVersionAnalyzerTests
{
    [TestMethod]
    public async Task Warn_For_CSharp_9()
    {
        var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        [|Contract.Requires(s != null, $""{s}"")|];
                        [|Contract.Assert(s != null, $""{s}"")|];
                    }
                }
            }";

        await new VerifyCS.Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9
        }.WithoutGeneratedCodeVerification().RunAsync();
    }
        
    [TestMethod]
    public async Task No_Warn_For_CSharp_10()
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
                        Contract.Assert(s != null, $""{s}"");
                    }
                }
            }";

        await new VerifyCS.Test
        {
            TestState = { Sources = { test } },
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10
        }.WithoutGeneratedCodeVerification().RunAsync();
    }
}