#define CONTRACTS_LIGHT_ASSERTS
#define CONTRACTS_LIGHT_PRECONDITIONS

using System;
using System.Diagnostics.ContractsLight;

using Xunit;

namespace RuntimeContracts.Test;

public class ContractAssertTests
{
    [Fact]
    public void PreconditionsShouldFail()
    {
        new ContractAssertions().AssertionFailures(true);
    }

    [Fact]
    public void MessageCanBeNull()
    {
        Assert.NotNull(shouldFail());

        static Exception shouldFail()
        {
            try
            {
                Contract.Assert(false, null);
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}