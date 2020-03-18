#define CONTRACTS_LIGHT_ASSERTS
#define CONTRACTS_LIGHT_PRECONDITIONS
using System;

using Xunit;

namespace RuntimeContracts.Test
{
    public static class AssertionExtensions
    {
        public static void ShouldThrow(this Action action)
        {
            bool failWithContractException = false;
            try
            {
                action();
            }
            catch (Exception e) when (e.GetType().Name == "ContractException")
            {
                failWithContractException = true;
            }

            Assert.True(failWithContractException);
        }
    }
}