using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RuntimeContracts.Analyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void FailsOnContractRequires()
        {
            var test = @"using System.Diagnostics.Contracts;
    
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public TypeName(string s)
            {
                Contract.Requires(s != null);
            }
        }
    }";

            var diagnostic = GetFirstDiagnosticFor(test);
            Assert.AreEqual(diagnostic.Id, DoNotUseStandardContractAnalyzer.DiagnosticId);
        }

        [TestMethod]
        public void FixUsingOnTopLevel()
        {
            var test = @"using System;
using System.Diagnostics.Contracts;
    
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public TypeName(string s)
            {
                Contract.Requires(s != null);
            }
        }
    }";

            var diagnostic = GetFirstDiagnosticFor(test);
            Assert.AreEqual(diagnostic.Id, DoNotUseStandardContractAnalyzer.DiagnosticId);

            VerifyCSharpFix(test, test.Replace("System.Diagnostics.Contracts", "System.Diagnostics.ContractsLight"), codeFixIndex: null, allowNewCompilerDiagnostics: true);
        }

        [TestMethod]
        public void FixUsingInsideNamespace()
        {
            var test = @"using System;
    
    namespace ConsoleApplication1
    {
using System.Diagnostics.Contracts;
        class TypeName
        {
            public TypeName(string s)
            {
                Contract.Requires(s != null);
            }
        }
    }";

            var diagnostic = GetFirstDiagnosticFor(test);
            Assert.AreEqual(diagnostic.Id, DoNotUseStandardContractAnalyzer.DiagnosticId);

            VerifyCSharpFix(test, test.Replace("System.Diagnostics.Contracts", "System.Diagnostics.ContractsLight"), codeFixIndex: null, allowNewCompilerDiagnostics: true);
        }

        [TestMethod]
        public void FixUsingInsideSecondNamespace()
        {
            var test = @"using System;

namespace NsFake {
using System.Diagnostics.Contracts;
        class TypeName
        {
            public TypeName(string s)
            {
                Contract.Requires(s != null);
            }
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
                Contract.Requires(s != null);
            }
        }
    }";

            var diagnostic = GetFirstDiagnosticFor(test);
            Assert.AreEqual(diagnostic.Id, DoNotUseStandardContractAnalyzer.DiagnosticId);

            VerifyCSharpFix(test, test.Replace("System.Diagnostics.Contracts", "System.Diagnostics.ContractsLight"), codeFixIndex: null, allowNewCompilerDiagnostics: true);
        }

        protected override string GetAdditionalSources()
        {
            return @"
namespace System.Diagnostics.ContractsLight
{
    public static class Contract
    {
        public static void Requires(bool predicate, string message = null) {}
        public static void Assert(bool predicate, string message = null) {}
    }
}";
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RuntimeContractsAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseStandardContractAnalyzer();
        }
    }
}
