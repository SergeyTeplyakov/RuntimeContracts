#define CONTRACTS_LIGHT_ASSERTS
#define CONTRACTS_LIGHT_PRECONDITIONS
using System;
using System.Diagnostics.ContractsLight;
using Xunit;

namespace RuntimeContracts.Test;

public static class AssertionExtensions
{
    public static string ShouldThrow(this Action action) => Assert.Throws<ContractException>(action).Message;

    public static string ShouldThrow<TException>(this Action action) where TException : Exception
        => Assert.Throws<TException>(action).Message;
}