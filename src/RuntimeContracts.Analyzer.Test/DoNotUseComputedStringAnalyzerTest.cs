using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RuntimeContracts.Analyzer.Test
{
    [TestClass]
    public class DoNotUseComputedStringAnalyzerTest : CodeFixVerifier
    {
        [TestMethod]
        public void FailsWhenAssertWithInterpolatedMessage()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(s != null, $""String {s} is not null."");
                    }
                }
            }";

            var diagnostic = GetFirstDiagnostic(test);
            Assert.AreEqual(diagnostic.Id, DoNotUseComputedStringAnalyzer.DiagnosticId);
        }

        [TestMethod]
        public void FailsWhenAssertWithStringConcat()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(s != null, ""String "" + s + ""is not null."");
                    }
                }
            }";

            var diagnostic = GetFirstDiagnostic(test);
            Assert.AreEqual(diagnostic.Id, DoNotUseComputedStringAnalyzer.DiagnosticId);
        }

        [TestMethod]
        public void NoWarnsWhenAssertWithStringConcatWithoutExpressions()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(s != null, ""String is not null."" + ""Another"");
                    }
                }
            }";

            NoDiagnostic(test);
        }

        [TestMethod]
        public void NoWarnWhenAssertWithLiteral()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(s != null, $""String s is not null."");
                    }
                }
            }";

            NoDiagnostic(test);
        }

        [TestMethod]
        public void NoWarnWhenAssertWhenTheFirstArgumentIsFalse()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(false, $""String {s} is not null."");
                    }
                }
            }";

            NoDiagnostic(test);
        }

        [TestMethod]
        public void NoWarnWhenAssertWithInterpolatedStringWithoutCaptures()
        {
            var test = @"using System.Diagnostics.ContractsLight;

            namespace ConsoleApplication1
            {
                class TypeName
                {
                    public TypeName(string s)
                    {
                        Contract.Assert(s != null, $""String s is not null."");
                    }
                }
            }";

            NoDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseComputedStringAnalyzer();
        }

        private Diagnostic GetFirstDiagnostic(string source)
        {
            var diagnostics = GetSortedDiagnostics(source + Environment.NewLine + ContractClass);
            return diagnostics.First();
        }

        private void NoDiagnostic(string source)
        {
            var diagnostics = GetSortedDiagnostics(source + Environment.NewLine + ContractClass);
            if (diagnostics.Any())
            {
                Assert.IsTrue(false, string.Format("\r\n", diagnostics.Select(d => d.ToString())));
            }
        }

        private const string ContractClass = @"
namespace System.Diagnostics.ContractsLight
{
    public static class Contract
    {
        public static void Requires(bool b, string s = null) {}
        public static void Assert(bool b, string s = null) {}
        public static void Assume(bool b, string s = null) {}
    }
}
";
    }
}
