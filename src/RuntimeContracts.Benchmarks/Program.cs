#define CONTRACTS_LIGHT_PRECONDITIONS
//#define CONTRACTS_LIGHT_ASSERTS

using System.Diagnostics.ContractsLight;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace RuntimeContracts.Benchmarks;

[MemoryDiagnoser]
public class ContractOverheadPredicatesBenchamrk
{
    public int Iterations = 100_000;

    [Benchmark]
    public void ContractsStaticMessage()
    {
        long count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            ContractStaticMessage(i);
            count += i;
        }
    }

    [Benchmark]
    public void FluentContractsStaticMessage()
    {
        long count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            FluentContractStaticMessage(i);
            count += i;
        }
    }

    [Benchmark]
    public void ContractsStringFormatMessage()
    {
        long count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            ContractStringFormatMessage(i);
            count += i;
        }
    }

    [Benchmark]
    public void ContractsInterpolatedStringMessage()
    {
        long count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            ContractInterpolatedMessage(i);
            count += i;
        }
    }

    [Benchmark]
    public void FluentContractsDynamicMessage()
    {
        long count = 0;
        for (int i = 0; i < Iterations; i++)
        {
            FluentContractMessage(i);
            count += i;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ContractStringFormatMessage(int i)
    {
        Contract.Requires(i >= 0, string.Format("i >= 0, i={0}", i));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ContractInterpolatedMessage(int i)
    {
        Contract.Requires(i >= 0, $"i >= 0, i={i}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void FluentContractMessage(int i)
    {
        Contract.Check(i >= 0)?.Requires($"i >= 0, i={i}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ContractStaticMessage(int i)
    {
        Contract.Requires(i >= 0, "i >= 0");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void FluentContractStaticMessage(int i)
    {
        Contract.Check(i >= 0)?.Requires($"i >= 0");
    }
}

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<ContractOverheadPredicatesBenchamrk>();
    }
}