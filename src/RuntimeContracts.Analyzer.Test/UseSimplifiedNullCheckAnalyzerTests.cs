//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Threading.Tasks;
//using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
//    RuntimeContracts.Analyzer.UseSimplifiedNullCheckAnalyzer,
//    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

//namespace RuntimeContracts.Analyzer.Test
//{
//    [TestClass]
//    public class DoNotUseExplicitNullChecksTest
//    {
//        [TestMethod]
//        public async Task WarnOnNullChecks()
//        {
//            var test = @"using System.Diagnostics.ContractsLight;
//            #nullable enable
//            namespace ConsoleApplication1
//            {
//                class TypeName
//                {
//                    private string? _f;
//                    private string? Prop => _f;
//                    private string? MyFunc() => null;
//                    public TypeName(string s)
//                    {
//                        System.Diagnostics.Contracts.Contract.Requires(s != null);
//                        [|Contract.Assert(s != null, ""Message"")|];
//                        Contract.Assert(s != null && true); // The check is 'too complicated' for the analysis.

//                        string s2 = s;
//                        Contract.AssertNotNull(s2);
//                        [|Contract.Assert(s2 != null)|];

//                        [|Contract.Assert(_f != null)|];
//                        [|Contract.Assert(Prop != null)|];
//                        [|Contract.Assert(MyFunc() != null)|];
//                    }
//                }
//            }";

//            await new VerifyCS.Test
//            {
//                TestState = { Sources = { test } },
//                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8
//            }.WithoutGeneratedCodeVerification().RunAsync();
//        }

//        [TestMethod]
//        public async Task WarnOnNullOrEmptyChecks()
//        {
//            var test = @"using System.Diagnostics.ContractsLight;
//            #nullable enable
//            namespace ConsoleApplication1
//            {
//                class TypeName
//                {
//                    private string? _f;
//                    private string? Prop => _f;
//                    private string? MyFunc() => null;
//                    public TypeName(string s)
//                    {
//                        System.Diagnostics.Contracts.Contract.Requires(s != null);
//                        [|Contract.Assert(!string.IsNullOrEmpty(s), ""Message"")|];
//                        Contract.Assert(!string.IsNullOrEmpty(s) && s != null); // The check is 'too complicated' for the analysis.

//                        string s2 = s;
//                        Contract.AssertNotNull(s2);
//                        [|Contract.Assert(!System.String.IsNullOrEmpty(s2))|];

//                        Contract.Assert(string.IsNullOrEmpty(_f));
//                        [|Contract.Assert(!string.IsNullOrEmpty(_f))|];
//                        [|Contract.Assert(!string.IsNullOrWhiteSpace(Prop))|];
//                        [|Contract.Assert(!string.IsNullOrEmpty(MyFunc()))|];
//                    }
//                }
//            }";

//            await new VerifyCS.Test
//            {
//                TestState = { Sources = { test } },
//                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8
//            }.WithoutGeneratedCodeVerification().RunAsync();
//        }
//    }
//}
