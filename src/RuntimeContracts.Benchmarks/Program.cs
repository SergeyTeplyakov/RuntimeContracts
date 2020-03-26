#define CONTRACTS_LIGHT_PRECONDITIONS
//#define CONTRACTS_LIGHT_ASSERTS

using System;
using System.Collections.Generic;
using System.Diagnostics.ContractsLight;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace RuntimeContracts.Benchmarks
{
    public class ContractOverheadPredicatesBenchamrk
    {
        public int Iterations = 100_000;

        [Benchmark]
        public void OldContractsStaticMessage()
        {
            long count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                OldContractStaticMessage(i);
                count += i;
            }
        }

        [Benchmark]
        public void NewContractsStaticMessage()
        {
            long count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                NewContractStaticMessage(i);
                count += i;
            }
        }

        [Benchmark]
        public void OldContractsDynamicMessage()
        {
            long count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                OldContractMessage(i);
                count += i;
            }
        }

        [Benchmark]
        public void NewContractsDynamicMessage()
        {
            long count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                NewContractMessage(i);
                count += i;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void OldContractMessage(int i)
        {
            System.Diagnostics.ContractsLight.Contract.Requires(i >= 0, $"i >= 0, i={i}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void NewContractMessage(int i)
        {
            Contract.Check(i >= 0)?.Requires($"i >= 0, i={i}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void OldContractStaticMessage(int i)
        {
            System.Diagnostics.ContractsLight.Contract.Requires(i >= 0, "i >= 0");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void NewContractStaticMessage(int i)
        {
            Contract.Check(i >= 0)?.Requires($"i >= 0");
        }
    }

    public class ContractOverheadNullCheckBenchmark
    {
        public int Iterations = 100_000;

        [Benchmark]
        public void OldContractsStaticMessage()
        {
            long count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                OldContractStaticMessage(i, this);
                count += i;
            }
        }

        [Benchmark]
        public void NewContractsStaticMessage()
        {
            long count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                NewContractStaticMessage(i, this);
                count += i;
            }
        }

        [Benchmark]
        public void OldContractsDynamicMessage()
        {
            long count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                OldContractMessage(i, this);
                count += i;
            }
        }

        [Benchmark]
        public void NewContractsDynamicMessage()
        {
            long count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                NewContractMessage(i, this);
                count += i;
            }
        }

        [Benchmark]
        public void NewContractsIsCheck()
        {
            long count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                NewContractIsCheckMessage(i, this);
                count += i;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void OldContractMessage(int i, ContractOverheadNullCheckBenchmark @this)
        {
            System.Diagnostics.ContractsLight.Contract.RequiresNotNull(@this, $"i >= 0, i={i}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void NewContractMessage(int i, ContractOverheadNullCheckBenchmark @this)
        {
            Contract.Check(@this != null)?.Requires($"i >= 0, i={i}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void NewContractIsCheckMessage(int i, ContractOverheadNullCheckBenchmark @this)
        {
            Contract.Check(@this is object)?.Requires($"i >= 0, i={i}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void OldContractStaticMessage(int i, ContractOverheadNullCheckBenchmark @this)
        {
            System.Diagnostics.ContractsLight.Contract.RequiresNotNull(@this, "i >= 0");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void NewContractStaticMessage(int i, ContractOverheadNullCheckBenchmark @this)
        {
            Contract.Check(@this != null)?.Requires($"i >= 0");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<ContractOverheadPredicatesBenchamrk>();
            BenchmarkRunner.Run<ContractOverheadNullCheckBenchmark>();
        }
    }
}
