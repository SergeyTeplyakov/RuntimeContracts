//using Microsoft.CodeAnalysis.Testing;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Threading;
//using System.Threading.Tasks;
//using VerifyCS = RuntimeContracts.Analyzer.Test.CSharpCodeFixVerifier<
//    RuntimeContracts.Analyzer.UseSimplifiedNullCheckAnalyzer,
//    RuntimeContracts.Analyzer.UseSimplifiedNullCheckCodeFixProvider>;

//namespace RuntimeContracts.Analyzer.Test
//{
//    [TestClass]
//    public class UseSimplifiedNullCheckCodeFixProvider
//    {
//        [TestMethod]
//        public async Task FixNullChecks()
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
//                        [|Contract.Requires(s != null, ""Message"")|];

//                        string s2 = s;
//                        [|Contract.Assert(s2 != null, userMessage: string.Empty)|];

//                        [|Contract.Assert(_f != null)|];
//                        [|Contract.Assert(Prop != null)|];
//                        [|Contract.Assert(MyFunc() != null)|];
//                    }
//                }
//            }";

//            var fixedTest = @"using System.Diagnostics.ContractsLight;
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
//                        Contract.RequiresNotNull(s, ""Message"");

//                        string s2 = s;
//                        Contract.AssertNotNull(s2, userMessage: string.Empty);

//                        Contract.AssertNotNull(_f);
//                        Contract.AssertNotNull(Prop);
//                        Contract.AssertNotNull(MyFunc());
//                    }
//                }
//            }";

//            await new VerifyCS.Test
//            {
//                TestState = { Sources = { test } },
//                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8,
//                FixedState = { Sources = { fixedTest } },
//            }.WithoutGeneratedCodeVerification().RunAsync();
//        }

//        [TestMethod]
//        public async Task FixNullOrEmptyChecks()
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
//                        [|Contract.Requires(!string.IsNullOrEmpty(s), userMessage: ""Message"")|];
//                        [|Contract.Requires(!string.IsNullOrWhiteSpace(s), ""Message"")|];
//                        Contract.Assert(!string.IsNullOrEmpty(s) && s != null); // The check is 'too complicated' for the analysis.

//                        string s2 = s;
//                        Contract.AssertNotNull(s2);
//                        Contract.AssertNotNullOrEmpty(s2);
//                        Contract.AssertNotNullOrWhiteSpace(s2);
//                        [|Contract.Assert(!System.String.IsNullOrEmpty(s2))|];

//                        Contract.Assert(string.IsNullOrEmpty(_f));
//                        [|Contract.Assert(!string.IsNullOrEmpty(_f))|];
//                        [|Contract.Assert(!string.IsNullOrWhiteSpace(Prop))|];
//                        [|Contract.Assert(!string.IsNullOrEmpty(MyFunc()))|];
//                    }
//                }
//            }";

//            var fixedTest = @"using System.Diagnostics.ContractsLight;
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
//                        Contract.RequiresNotNullOrEmpty(s, userMessage: ""Message"");
//                        Contract.RequiresNotNullOrWhiteSpace(s, ""Message"");
//                        Contract.Assert(!string.IsNullOrEmpty(s) && s != null); // The check is 'too complicated' for the analysis.

//                        string s2 = s;
//                        Contract.AssertNotNull(s2);
//                        Contract.AssertNotNullOrEmpty(s2);
//                        Contract.AssertNotNullOrWhiteSpace(s2);
//                        Contract.AssertNotNullOrEmpty(s2);

//                        Contract.Assert(string.IsNullOrEmpty(_f));
//                        Contract.AssertNotNullOrEmpty(_f);
//                        Contract.AssertNotNullOrWhiteSpace(Prop);
//                        Contract.AssertNotNullOrEmpty(MyFunc());
//                    }
//                }
//            }";

//            await new VerifyCS.Test
//            {
//                TestState = { Sources = { test } },
//                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8,
//                FixedState = { Sources = { fixedTest } },
//            }.WithoutGeneratedCodeVerification().RunAsync();
//        }
        

//        public async Task Main()
//        {


//            var run = Task.Run(async () =>
//            {
//                // CPU-bound operation (blocking)
                
//                // Thread 1
//                Thread.Sleep(1000);
//                // console.WriteLine
//                // compute A
//                // Compute B

//                var task1 = Task.Run(() => Thread.Sleep(1000)); //
//                var task2 = Task.Run(() => Thread.Sleep(1000)); //
//                await Task.WhenAll(task1, task2);
                
//                // IO-bound operation (non-blocking)
//                await Task.Delay(1000);

//                // Thread 2
//                Thread.Sleep(1000);

//                // 
//                await Task.Delay(1000);
//            });
//            await run;


//            var run2 = Task.Run(() =>
//            {
//                Thread.Sleep(1000);
//            });

//            var delayContinuation = run2.ContinueWith(_ =>
//            {
//                Thread.Sleep(1000);
//            });

//            var delay2Continuation = delayContinuation.ContinueWith(_ =>
//            {
//                Thread.Sleep(1000);
//            });

//            await delay2Continuation;



//            // 

//        }


//        [TestMethod]
//        public async Task AssumeShouldBeTranslatedToAssert()
//        {
//            var test = @"using System.Diagnostics.ContractsLight;
//            #nullable enable
//            namespace ConsoleApplication1
//            {
//                class TypeName
//                {
//                    public string FooBar(string s)
//                    {
//                        [|Contract.Assume(s != null)|];
//                        [|Contract.Assume(!string.IsNullOrEmpty(s))|];
//                        [|Contract.Assume(!string.IsNullOrWhiteSpace(s))|];
//                        Contract.Ensures(Contract.Result<string>() != null);
//                        return s;
//                    }
//                }
//            }";

//            var fixedTest = @"using System.Diagnostics.ContractsLight;
//            #nullable enable
//            namespace ConsoleApplication1
//            {
//                class TypeName
//                {
//                    public string FooBar(string s)
//                    {
//                        Contract.AssertNotNull(s);
//                        Contract.AssertNotNullOrEmpty(s);
//                        Contract.AssertNotNullOrWhiteSpace(s);
//                        Contract.Ensures(Contract.Result<string>() != null);
//                        return s;
//                    }
//                }
//            }";

//            await new VerifyCS.Test
//            {
//                TestState = { Sources = { test } },
//                LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8,
//                FixedState = { Sources = { fixedTest } },
//            }.WithoutGeneratedCodeVerification().RunAsync();
//        }
//    }
//}
