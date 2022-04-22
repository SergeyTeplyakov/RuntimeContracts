using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuntimeContracts.Analyzer.Test;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.DoNotUseFluentContractsAnalyzer,
    RuntimeContracts.Analyzer.DoNotUseFluentContractsCodeFixProvider>;

namespace RuntimeContracts.Analyzer.FluentContracts.Test;

[TestClass]
public class DoNotUseFluentContractsCodeFixProviderTests
{
    [TestMethod]
    public async Task WarnWhenResultOfRequiresIsNotEnforcedByIsTrueCallBulkOld()
    {
        var fixedTest =
            @"using System;
using System.Diagnostics.ContractsLight;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            Contract.Requires(s != null, s + 1);
            Contract.Requires(s != null, s + 1);
        }
    }
}";

        var test =
            @"using System;
using System.Diagnostics.ContractsLight;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            [|Contract.Check(s != null)?.Requires(s + 1)|];
            [|Contract.Check(s != null)?.Requires(s + 1)|];
        }
    }
}";

        await VerifyCS.RunBatchWithFixer(test, fixedTest, fixedTest);
    }

    [TestMethod]
    public async Task Convert_If_Then_Assert_False_Case()
    {
        var fixedTest =
            @"using System;
using System.Diagnostics.ContractsLight;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            Contract.Assert(s != null && s.Length <= s.Length + 1, s + 1);
            Contract.Assert(s != null, s + 1);
        }
    }
}";

        var test =
            @"using System;
using System.Diagnostics.ContractsLight;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            [|Contract.Check(s != null && s.Length <= s.Length + 1)?.Assert(s + 1)|];
            [|Contract.Check(s != null)?.Assert(s + 1)|];
        }
    }
}";

        await VerifyCS.RunBatchWithFixer(test, fixedTest, fixedTest);
    }

    //-----------------------------------------------Requires-----------------------------------------------//
    [TestMethod]
    public async Task WarnOnRequires()
        => await TestFixer("Contract.Requires(s != null, s + 1)", "Contract.Check(s != null)?.Requires(s + 1)");

    [TestMethod]
    public async Task WarnOnRequiresDebug()
        => await TestFixer("Contract.RequiresDebug(s != null, s + 1)", "Contract.CheckDebug(s != null)?.Requires(s + 1)");

    [TestMethod]
    public async Task WarnOnRequiresNotNullOrEmptyWithMessage()
        => await TestFixer("Contract.Requires(!string.IsNullOrEmpty(s), s + 1)", "Contract.Check(!string.IsNullOrEmpty(s))?.Requires(s + 1)");

    [TestMethod]
    public async Task WarnOnRequiresNotNullOrWhiteSpaceWithMessage()
        => await TestFixer("Contract.Requires(!string.IsNullOrWhiteSpace(s), s + 1)", "Contract.Check(!string.IsNullOrWhiteSpace(s))?.Requires(s + 1)");

    //-----------------------------------------------Assert-----------------------------------------------//
    [TestMethod]
    public async Task WarnOnAssertWithMessage()
        => await TestFixer("Contract.Assert(s != null, s + 1)", "Contract.Check(s != null)?.Assert(s + 1)");

    [TestMethod]
    public async Task WarnOnAssertNotNullOrEmptyWithMessage()
        => await TestFixer("Contract.Assert(!string.IsNullOrEmpty(s), s + 1)", "Contract.Check(!string.IsNullOrEmpty(s))?.Assert(s + 1)");

    [TestMethod]
    public async Task WarnOnAssertNotNullOrWhiteSpaceWithMessage()
        => await TestFixer("Contract.Assert(!string.IsNullOrWhiteSpace(s), s + 1)", "Contract.Check(!string.IsNullOrWhiteSpace(s))?.Assert(s + 1)");

    [TestMethod]
    public async Task WarnOnAssertNotNull()
        => await TestFixer("Contract.Assert(s != null, s + 1)", "Contract.Check(s != null)?.Assert(s + 1)");

    [TestMethod]
    public async Task WarnOnAssertNotNullOrEmpty()
        => await TestFixer("Contract.Assert(!string.IsNullOrEmpty(s), s + 1)", "Contract.Check(!string.IsNullOrEmpty(s))?.Assert(s + 1)");

    [TestMethod]
    public async Task WarnOnAssertNotNullOrWhiteSpace()
        => await TestFixer("Contract.Assert(!string.IsNullOrWhiteSpace(s), s + 1)", "Contract.Check(!string.IsNullOrWhiteSpace(s))?.Assert(s + 1)");

    //-----------------------------------------------Assert-----------------------------------------------//
    private async Task TestFixer(string fixedContract, string originalContract)
    {
        var test = 
            @"using System.Diagnostics.ContractsLight;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            [|REPLACE_ME|];
        }
    }
}".Replace("REPLACE_ME", originalContract);

        var fixedTest =
            @"using System.Diagnostics.ContractsLight;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            REPLACE_ME;
        }
    }
}".Replace("REPLACE_ME", fixedContract);

        await VerifyCS.RunWithFixer(test, fixedTest);
    }
}