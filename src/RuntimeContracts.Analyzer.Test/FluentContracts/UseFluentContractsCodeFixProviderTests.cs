using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuntimeContracts.Analyzer.Test;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.UseFluentContractsAnalyzer,
    RuntimeContracts.Analyzer.UseFluentContractsCodeFixProvider>;

namespace RuntimeContracts.Analyzer.FluentContracts.Test
{
    [TestClass]
    public class UseFluentContractsCodeFixProviderTests
    {
        [TestMethod]
        public async Task WarnWhenResultOfRequiresIsNotEnforcedByIsTrueCallBulkOld()
        {
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
            Contract.Requires(s != null, ""s"");
            Contract.Requires(s != null, s);
            [|Contract.Requires(s != null, s + 1)|];
            [|Contract.Requires(s != null, s + 1)|];
        }
    }
}";

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
            Contract.Requires(s != null, ""s"");
            Contract.Requires(s != null, s);
            Contract.Check(s != null)?.Requires(s + 1);
            Contract.Check(s != null)?.Requires(s + 1);
        }
    }
}";

            await VerifyCS.RunBatchWithFixer(test, fixedTest, fixedTest);
        }

        [TestMethod]
        public async Task Convert_If_Then_Assert_False_Case()
        {
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
            if (s == null || s.Length > s.Length + 1)
            {
                [|Contract.Assert(false, s + 1)|];
            }

            if (s == null)
            {
                [|Contract.Assert(false, s + 1)|];
            }
        }
    }
}";

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
            Contract.Check(s != null && s.Length <= s.Length + 1)?.Assert(s + 1);
            Contract.Check(s != null)?.Assert(s + 1);
        }
    }
}";

            await VerifyCS.RunBatchWithFixer(test, fixedTest, fixedTest);
        }

        [TestMethod]
        public async Task Do_Not_Convert_If_Then_Assert_False_Case()
        {
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
            if (s == null || s.Length > s.Length + 1)
            {
                [|Contract.Assert(false, s + 1)|];
                s += string.Empty;
            }
        }
    }
}";

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
            if (s == null || s.Length > s.Length + 1)
            {
                Contract.Check(false)?.Assert(s + 1);
                s += string.Empty;
            }
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
        public async Task WarnOnRequiresNotNullWithMessage() 
            => await TestFixer("Contract.RequiresNotNull(s, s + 1)", "Contract.Check(s != null)?.Requires(s + 1)");

        [TestMethod]
        public async Task WarnOnRequiresNotNullOrEmptyWithMessage()
            => await TestFixer("Contract.RequiresNotNullOrEmpty(s, s + 1)", "Contract.Check(!string.IsNullOrEmpty(s))?.Requires(s + 1)");

        [TestMethod]
        public async Task WarnOnRequiresNotNullOrWhiteSpaceWithMessage()
            => await TestFixer("Contract.RequiresNotNullOrWhiteSpace(s, s + 1)", "Contract.Check(!string.IsNullOrWhiteSpace(s))?.Requires(s + 1)");

        //-----------------------------------------------Assert-----------------------------------------------//
        [TestMethod]
        public async Task WarnOnAssertWithMessage()
            => await TestFixer("Contract.Assert(s != null, s + 1)", "Contract.Check(s != null)?.Assert(s + 1)");

        [TestMethod]
        public async Task WarnOnAssertNotNullWithMessage()
            => await TestFixer("Contract.AssertNotNull(s, s + 1)", "Contract.Check(s != null)?.Assert(s + 1)");

        [TestMethod]
        public async Task WarnOnAssertNotNullOrEmptyWithMessage()
            => await TestFixer("Contract.AssertNotNullOrEmpty(s, s + 1)", "Contract.Check(!string.IsNullOrEmpty(s))?.Assert(s + 1)");

        [TestMethod]
        public async Task WarnOnAssertNotNullOrWhiteSpaceWithMessage()
            => await TestFixer("Contract.AssertNotNullOrWhiteSpace(s, s + 1)", "Contract.Check(!string.IsNullOrWhiteSpace(s))?.Assert(s + 1)");

        [TestMethod]
        public async Task WarnOnAssertNotNull()
            => await TestFixer("Contract.AssertNotNull(s, s + 1)", "Contract.Check(s != null)?.Assert(s + 1)");

        [TestMethod]
        public async Task WarnOnAssertNotNullOrEmpty()
            => await TestFixer("Contract.AssertNotNullOrEmpty(s, s + 1)", "Contract.Check(!string.IsNullOrEmpty(s))?.Assert(s + 1)");

        [TestMethod]
        public async Task WarnOnAssertNotNullOrWhiteSpace()
            => await TestFixer("Contract.AssertNotNullOrWhiteSpace(s, s + 1)", "Contract.Check(!string.IsNullOrWhiteSpace(s))?.Assert(s + 1)");

        //-----------------------------------------------Assert-----------------------------------------------//
        [TestMethod]
        public async Task WarnOnAssume()
            => await TestFixer("Contract.Assume(s != null, s + 1)", "Contract.Check(s != null)?.Assert(s + 1)");

        [TestMethod]
        public async Task WarnOnAssumeWithMessage()
            => await TestFixer("Contract.Assume(s != null, s + 1)", "Contract.Check(s != null)?.Assert(s + 1)");

        private async Task TestFixer(string originalContract, string fixedContract)
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
}
