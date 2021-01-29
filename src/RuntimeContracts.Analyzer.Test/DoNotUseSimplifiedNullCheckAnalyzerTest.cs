using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.DoNotUseSimplifiedNullCheckAnalyzer,
    RuntimeContracts.Analyzer.DoNotUseSimplifiedNullCheckCodeFixProvider>;

namespace RuntimeContracts.Analyzer.Test
{
    [TestClass]
    public class DoNotUseSimplifiedNullCheckAnalyzerTest
    {
        [TestMethod]
        public async Task RequiresNotNull()
        {
            var test = @"using System.Diagnostics.ContractsLight;
    
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            [|Contract.RequiresNotNull(s, ""s != null"")|];
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                FixedState = { Sources = { test.Replace("RequiresNotNull(s", "Requires(s != null") } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        //[TestMethod] // Not applicable to RequiresNotNullOrEmpty
        public async Task RequiresNotNullOrEmpty()
        {
            var test = @"using System.Diagnostics.ContractsLight;
    
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            [|Contract.RequiresNotNullOrEmpty(s)|];
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                FixedState = { Sources = { test.Replace("RequiresNotNullOrEmpty(s)", "Requires(!string.IsNullOrEmpty(s))") } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }


        //[TestMethod] // Not applicable to RequiresNotNullOrWhiteSpace
        public async Task RequiresNotNullOrWhitespace()
        {
            var test = @"using System.Diagnostics.ContractsLight;
    
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            [|Contract.RequiresNotNullOrWhiteSpace(s)|];
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                FixedState = { Sources = { test.Replace("RequiresNotNullOrWhiteSpace(s)", "Requires(!string.IsNullOrWhiteSpace(s))") } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task AssertNotNull()
        {
            var test = @"using System.Diagnostics.ContractsLight;
    
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            [|Contract.AssertNotNull(s)|];
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                FixedState = { Sources = { test.Replace("AssertNotNull(s)", "Assert(s != null)") } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        // [TestMethod] // not applicable to AssertNotNullOrEmpty
        public async Task AssertNotNullOrEmpty()
        {
            var test = @"using System.Diagnostics.ContractsLight;
    
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            [|Contract.AssertNotNullOrEmpty(s)|];
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                FixedState = { Sources = { test.Replace("AssertNotNullOrEmpty(s)", "Assert(!string.IsNullOrEmpty(s))") } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        // [TestMethod] // not applicable to AssertNotNullOrWhiteSpace
        public async Task AssertNotNullOrWhitespace()
        {
            var test = @"using System.Diagnostics.ContractsLight;
    
namespace ConsoleApplication1
{
    class TypeName
    {
        public TypeName(string s)
        {
            [|Contract.AssertNotNullOrWhiteSpace(s)|];
        }
    }
}";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                FixedState = { Sources = { test.Replace("AssertNotNullOrWhiteSpace(s)", "Assert(!string.IsNullOrWhiteSpace(s))") } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }
    }
}
