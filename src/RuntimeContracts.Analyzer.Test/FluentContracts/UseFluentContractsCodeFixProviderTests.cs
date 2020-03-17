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
            [|Contract.Requires(s != null)|];
            [|Contract.Requires(s != null)|];
            [|Contract.Requires(s != null)|];
        }
    }
}";

            var fixedTest =
@"using System;
using System.Diagnostics.FluentContracts;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            Contract.Requires(s != null)?.IsTrue(""s != null"");
            Contract.Requires(s != null);
            Contract.Requires(s != null);
        }
    }
}";

            var batchFixedTest =
@"using System;
using System.Diagnostics.FluentContracts;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            Contract.Requires(s != null)?.IsTrue(""s != null"");
            Contract.Requires(s != null)?.IsTrue(""s != null"");
            Contract.Requires(s != null)?.IsTrue(""s != null"");
        }
    }
}";

            await VerifyCS.RunBatchWithFixer(test, fixedTest, batchFixedTest);
        }

        [TestMethod]
        public async Task WarnWhenResultOfRequiresIsNotEnforcedByIsTrueCall()
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
            [|Contract.Requires(s != null)|];
        }
    }
}";

            var fixedTest = 
@"using System;
using System.Diagnostics.FluentContracts;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            Contract.Requires(s != null)?.IsTrue(""s != null"");
        }
    }
}";

            await VerifyCS.RunWithFixer(test, fixedTest);
        }

        //-----------------------------------------------Requires-----------------------------------------------//
        [TestMethod]
        public async Task WarnOnRequires()
            => await TestFixer("Contract.Requires(s != null)", "Contract.Requires(s != null)?.IsTrue(\"s != null\")");

        [TestMethod]
        public async Task WarnOnRequiresDebug()
            => await TestFixer("Contract.RequiresDebug(s != null)", "Contract.RequiresDebug(s != null)?.IsTrue(\"s != null\")");

        [TestMethod]
        public async Task WarnOnRequiresForAll()
            => await TestFixer(
                "Contract.RequiresForAll(System.Linq.Enumerable.Empty<string>(), s => s != null)",
                "Contract.RequiresForAll(System.Linq.Enumerable.Empty<string>(), s => s != null)?.IsTrue(\"s => s != null\")");

        [TestMethod]
        public async Task WarnOnRequiresForAllWithMessage()
            => await TestFixer(
                "Contract.RequiresForAll(System.Linq.Enumerable.Empty<string>(), s => s != null, \"message\")",
                "Contract.RequiresForAll(System.Linq.Enumerable.Empty<string>(), s => s != null)?.IsTrue(\"message\")");

        [TestMethod]
        public async Task WarnOnRequiresWithMessage()
            => await TestFixer("Contract.Requires(s != null, \"message\")", "Contract.Requires(s != null)?.IsTrue(\"message\")");

        [TestMethod]
        public async Task WarnOnRequiresNotNullWithMessage() 
            => await TestFixer("Contract.RequiresNotNull(s, \"message\")", "Contract.Requires(s != null)?.IsTrue(\"message\")");

        [TestMethod]
        public async Task WarnOnRequiresNotNullOrEmptyWithMessage()
            => await TestFixer("Contract.RequiresNotNullOrEmpty(s, \"message\")", "Contract.Requires(!string.IsNullOrEmpty(s))?.IsTrue(\"message\")");

        [TestMethod]
        public async Task WarnOnRequiresNotNullOrWhiteSpaceWithMessage()
            => await TestFixer("Contract.RequiresNotNullOrWhiteSpace(s, \"message\")", "Contract.Requires(!string.IsNullOrWhiteSpace(s))?.IsTrue(\"message\")");

        [TestMethod]
        public async Task WarnOnRequiresNotNull()
            => await TestFixer("Contract.RequiresNotNull(s)", "Contract.Requires(s != null)?.IsTrue(\"s != null\")");

        [TestMethod]
        public async Task WarnOnRequiresNotNullOrEmpty()
            => await TestFixer("Contract.RequiresNotNullOrEmpty(s)", "Contract.Requires(!string.IsNullOrEmpty(s))?.IsTrue(\"!string.IsNullOrEmpty(s)\")");

        [TestMethod]
        public async Task WarnOnRequiresNotNullOrWhiteSpace()
            => await TestFixer("Contract.RequiresNotNullOrWhiteSpace(s)", "Contract.Requires(!string.IsNullOrWhiteSpace(s))?.IsTrue(\"!string.IsNullOrWhiteSpace(s)\")");

        //-----------------------------------------------Assert-----------------------------------------------//
        [TestMethod]
        public async Task WarnOnAssert()
            => await TestFixer("Contract.Assert(s != null)", "Contract.Assert(s != null)?.IsTrue(\"s != null\")");

        [TestMethod]
        public async Task WarnOnAssertDebug()
            => await TestFixer("Contract.AssertDebug(s != null)", "Contract.AssertDebug(s != null)?.IsTrue(\"s != null\")");

        [TestMethod]
        public async Task WarnOnAssertWithMessage()
            => await TestFixer("Contract.Assert(s != null, \"message\")", "Contract.Assert(s != null)?.IsTrue(\"message\")");

        [TestMethod]
        public async Task WarnOnAssertNotNullWithMessage()
            => await TestFixer("Contract.AssertNotNull(s, \"message\")", "Contract.Assert(s != null)?.IsTrue(\"message\")");

        [TestMethod]
        public async Task WarnOnAssertNotNullOrEmptyWithMessage()
            => await TestFixer("Contract.AssertNotNullOrEmpty(s, \"message\")", "Contract.Assert(!string.IsNullOrEmpty(s))?.IsTrue(\"message\")");

        [TestMethod]
        public async Task WarnOnAssertNotNullOrWhiteSpaceWithMessage()
            => await TestFixer("Contract.AssertNotNullOrWhiteSpace(s, \"message\")", "Contract.Assert(!string.IsNullOrWhiteSpace(s))?.IsTrue(\"message\")");

        [TestMethod]
        public async Task WarnOnAssertNotNull()
            => await TestFixer("Contract.AssertNotNull(s)", "Contract.Assert(s != null)?.IsTrue(\"s != null\")");

        [TestMethod]
        public async Task WarnOnAssertNotNullOrEmpty()
            => await TestFixer("Contract.AssertNotNullOrEmpty(s)", "Contract.Assert(!string.IsNullOrEmpty(s))?.IsTrue(\"!string.IsNullOrEmpty(s)\")");

        [TestMethod]
        public async Task WarnOnAssertNotNullOrWhiteSpace()
            => await TestFixer("Contract.AssertNotNullOrWhiteSpace(s)", "Contract.Assert(!string.IsNullOrWhiteSpace(s))?.IsTrue(\"!string.IsNullOrWhiteSpace(s)\")");

        //-----------------------------------------------Assert-----------------------------------------------//
        [TestMethod]
        public async Task WarnOnAssume()
            => await TestFixer("Contract.Assume(s != null)", "Contract.Assert(s != null)?.IsTrue(\"s != null\")");

        [TestMethod]
        public async Task WarnOnAssumeWithMessage()
            => await TestFixer("Contract.Assume(s != null, \"message\")", "Contract.Assert(s != null)?.IsTrue(\"message\")");

        //-----------------------------------------------Ensures-----------------------------------------------//
        [TestMethod]
        public async Task EnsuresAreRemoved()
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
            [|Contract.Ensures(s != null)|];
            int x = s.Length;
        }
    }
}";

            var fixedTest =
@"using System.Diagnostics.FluentContracts;
#nullable enable
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            int x = s.Length;
        }
    }
}";

            await VerifyCS.RunWithFixer(test, fixedTest);
        }

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
@"using System.Diagnostics.FluentContracts;
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
