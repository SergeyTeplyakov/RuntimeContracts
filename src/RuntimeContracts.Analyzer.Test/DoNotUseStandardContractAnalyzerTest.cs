using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
    RuntimeContracts.Analyzer.DoNotUseStandardContractAnalyzer,
    RuntimeContracts.Analyzer.RuntimeContractsAnalyzerCodeFixProvider>;

namespace RuntimeContracts.Analyzer.Test
{
    [TestClass]
    public class DoNotUseStandardContractAnalyzerTest
    {
        [TestMethod]
        public async Task FailsOnContractRequires()
        {
            var test = @"using System.Diagnostics.Contracts;
    
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

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task FixUsingOnTopLevel()
        {
            var test = @"using System;
using System.Diagnostics.Contracts;
    
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

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                FixedState = { Sources = { test.Replace("System.Diagnostics.Contracts", "System.Diagnostics.ContractsLight") } },
                CodeFixValidationMode = CodeFixValidationMode.None,
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task FixUsingInsideNamespace()
        {
            var test = @"using System;
    
    namespace ConsoleApplication1
    {
using System.Diagnostics.Contracts;
        class TypeName
        {
            public TypeName(string s)
            {
                [|Contract.Requires(s != null)|];
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                FixedState = { Sources = { test.Replace("System.Diagnostics.Contracts", "System.Diagnostics.ContractsLight") } },
                CodeFixValidationMode = CodeFixValidationMode.None,
            }.WithoutGeneratedCodeVerification().RunAsync();
        }

        [TestMethod]
        public async Task FixUsingInsideSecondNamespace()
        {
            var test = @"using System;

namespace NsFake {
using System.Diagnostics.Contracts;
        class TypeName
        {
            public TypeName(string s)
            {
                [|Contract.Requires(s != null)|];
            }
        }
}    

    namespace ConsoleApplication1
    {
using System.Diagnostics.Contracts;
        class TypeName
        {
            public TypeName(string s)
            {
                [|Contract.Requires(s != null)|];
            }
        }
    }";

            await new VerifyCS.Test
            {
                TestState = { Sources = { test } },
                FixedState = { Sources = { test.Replace("System.Diagnostics.Contracts", "System.Diagnostics.ContractsLight") } },
                CodeFixValidationMode = CodeFixValidationMode.None,
            }.WithoutGeneratedCodeVerification().RunAsync();
        }
    }
}
