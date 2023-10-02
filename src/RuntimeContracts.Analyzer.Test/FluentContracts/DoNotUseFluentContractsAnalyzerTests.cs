using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuntimeContracts.Analyzer.Test;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.DoNotUseFluentContractsAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace RuntimeContracts.Analyzer.FluentContracts.Test;

[TestClass]
public class DoNotUseFluentContractsAnalyzerTests
{
    [TestMethod]
    public async Task Warn_On_Check_Requires()
    {
        var test = @"using System.Diagnostics.ContractsLight;
            #nullable enable
            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s, string[] c)
                    {
                        [|Contract.Check(s != null)?.Requires($""{s}"")|];
                        [|Contract.Check(s != null)?.Assert($""{s}"")|];
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