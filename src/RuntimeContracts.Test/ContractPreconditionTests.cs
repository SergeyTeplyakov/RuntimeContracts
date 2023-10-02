#define CONTRACTS_LIGHT_PRECONDITIONS
using System;
using System.Diagnostics.ContractsLight;
using Xunit;

namespace RuntimeContracts.FluentContracts.Test;

public class ContractPreconditionTests
{
    [Fact]
    public void CorrectExceptionTypeShouldBeGenerated()
    {
        Assert.Throws<ArgumentNullException>(() => WillFail(null));

        static void WillFail(string s)
        {
            Contract.Requires<ArgumentNullException>(s != null, "custom message");
        }
    }

    [Fact]
    public void PreconditionsShouldFail()
    {
        try
        {
            someMethod(-1);
            Assert.True(false, "someMethod(-1) should throw contract exception");
        }
        catch(Exception e) when (e.GetType().Name.Contains("ContractException"))
        {

        }

        static void someMethod(int arg)
        {
            Contract.Requires(arg > 0);
        }
    }
}