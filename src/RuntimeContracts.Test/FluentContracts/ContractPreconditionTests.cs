#define CONTRACTS_LIGHT_PRECONDITIONS
using System;
using System.Diagnostics.ContractsLight;

using Xunit;

namespace RuntimeContracts.Test
{
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
            new ContractAssertions().PreconditionFailures(true);
        }
    }
}
