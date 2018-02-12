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

            void WillFail(string s)
            {
                Contract.Requires<ArgumentNullException>(s != null, "custom message");
            }
        }
    }
}
