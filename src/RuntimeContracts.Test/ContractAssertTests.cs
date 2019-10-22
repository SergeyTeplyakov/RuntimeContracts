using System;
using System.Diagnostics.ContractsLight;

using Xunit;

namespace RuntimeContracts.Test
{
    public class ContractAssertTests
    {
        [Fact]
        public void PreconditionsShouldFail()
        {
            new ContractAssertions().AssertionFailures(true);
        }
    }
}
