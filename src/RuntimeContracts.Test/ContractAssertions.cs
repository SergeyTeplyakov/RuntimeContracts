#define CONTRACTS_LIGHT_ASSERTS
#define CONTRACTS_LIGHT_PRECONDITIONS
using System;
using System.Diagnostics.ContractsLight;

using Xunit;

namespace RuntimeContracts.Test;

public class ContractAssertions
{
    public void PreconditionFailures(bool shouldFail)
    {
        AssertThrowsContractException(shouldFail, () => failsWithRequires(null));
        AssertThrowsContractException(shouldFail, () => failsWithRequiresNotNull(null));
        AssertThrowsContractException(shouldFail, () => failsWithRequiresNotNullForInt(new int?()));
        AssertThrowsContractException(shouldFail, () => failsWithRequiresNotNullOrEmpty(null));
        AssertThrowsContractException(shouldFail, () => failsWithRequiresNotNullOrWhitespace(null));

        static void failsWithRequires(string s)
        {
            Contract.Requires(s != null);
        }

        static void failsWithRequiresNotNull(string s)
        {
            Contract.RequiresNotNull(s);
        }

        static void failsWithRequiresNotNullForInt(int? n)
        {
            Contract.RequiresNotNull(n);
        }

        static void failsWithRequiresNotNullOrEmpty(string s)
        {
            Contract.RequiresNotNullOrEmpty(s);
        }

        static void failsWithRequiresNotNullOrWhitespace(string s)
        {
            Contract.RequiresNotNullOrWhiteSpace(s);
        }
    }
        
    public void AssertionFailures(bool shouldFail)
    {
        AssertThrowsContractException(shouldFail, () => failsWithOldNotNull(null));
        AssertThrowsContractException(shouldFail, () => failsWithNotNull(null));
        AssertThrowsContractException(shouldFail, () => failsWithNotNullForNullableInt(new int?()));
        AssertThrowsContractException(shouldFail, () => failsWithNotNullOrEmpty(null));
        AssertThrowsContractException(shouldFail, () => failsWithNotNullOrWhitespace(null));

        static void failsWithOldNotNull(string s)
        {
            Contract.Assert(s != null);
        }

        static void failsWithNotNull(string s)
        {
            Contract.AssertNotNull(s);
        }

        static void failsWithNotNullForNullableInt(int? n)
        {
            Contract.AssertNotNull(n);
        }

        static void failsWithNotNullOrEmpty(string s)
        {
            Contract.AssertNotNullOrEmpty(s);
        }

        static void failsWithNotNullOrWhitespace(string s)
        {
            Contract.AssertNotNullOrWhiteSpace(s);
        }
    }
        
    public static void AssertThrowsContractException(bool shouldFail, Action a)
    {
        bool failWithContractException = false;
        try
        {
            a();
        }
        catch (Exception e) when (e.GetType().Name == "ContractException")
        {
            failWithContractException = true;
        }

        Assert.Equal(shouldFail, failWithContractException);
    }
}