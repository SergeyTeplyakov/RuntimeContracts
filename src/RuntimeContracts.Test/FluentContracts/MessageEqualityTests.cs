#define CONTRACTS_LIGHT_ASSERTS
#define CONTRACTS_LIGHT_PRECONDITIONS

using System;
using System.Diagnostics.ContractsLight;
using Xunit;
using Xunit.Abstractions;

namespace RuntimeContracts.Test.FluentContracts;

/// <summary>
/// Contains a set of tests to make sure the old and the new APIs provide the same messages.
/// </summary>
public class MessageEqualityTests
{
    private readonly ITestOutputHelper _helper;

    public MessageEqualityTests(ITestOutputHelper helper)
    {
        _helper = helper;
    }

    [Fact]
    public void RequiresPredicateViolations()
    {
        var oldContractViolationCount = 0;
        var newContractViolationCount = 0;

        // With such a small number of tests, we don't care that the test instance will stay in memory
        // for the entire process lifetime.
        System.Diagnostics.ContractsLight.Contract.ContractFailed += (o, e) => oldContractViolationCount++;
        Contract.ContractFailed += (o, e) => newContractViolationCount++;

        var oldContractMessage = GetContractMessage(() => oldContract(0));
        var newContractMessage = GetContractMessage(() => newContract(0));
        Assert.Equal(oldContractMessage, newContractMessage);

        var oldContractMessage2 = GetContractMessage(() => oldContractWithMessage(0));
        var newContractMessage2 = GetContractMessage(() => newContractWithMessage(0));
        Assert.Equal(oldContractMessage2, newContractMessage2);

        // Both handlers are called for all the violations.
        // Can be greater than 4 because test runner runs other tests in parallel.
        Assert.True(oldContractViolationCount >= 4, $"oldContractViolationCount== {oldContractViolationCount}");
        Assert.True(newContractViolationCount >= 4, $"newContractViolationCount == {newContractViolationCount}");

        static void oldContract(int n)
        {
            System.Diagnostics.ContractsLight.Contract.Requires(n == 42);
        }

        static void oldContractWithMessage(int n)
        {
            System.Diagnostics.ContractsLight.Contract.Requires(n == 42, "n == 42");
        }

        static void newContract(int n)
        {
            Contract.Check(n == 42)?.Requires(string.Empty);
        }

        static void newContractWithMessage(int n)
        {
            Contract.Check(n == 42)?.Requires("n == 42");
        }
    }

    [Fact]
    public void AssertPredicateViolations()
    {
        var oldContractMessage = GetContractMessage(() => oldContract(0));
        var newContractMessage = GetContractMessage(() => newContract(0));
        Assert.Equal(oldContractMessage, newContractMessage);

        var oldContractMessage2 = GetContractMessage(() => oldContractWithMessage(0));
        var newContractMessage2 = GetContractMessage(() => newContractWithMessage(0));
        Assert.Equal(oldContractMessage2, newContractMessage2);

        static void oldContract(int n)
        {
            System.Diagnostics.ContractsLight.Contract.Assert(n == 42);
        }

        static void oldContractWithMessage(int n)
        {
            System.Diagnostics.ContractsLight.Contract.Assert(n == 42, "n == 42");
        }

        static void newContract(int n)
        {
            Contract.Check(n == 42)?.Assert(string.Empty);
        }

        static void newContractWithMessage(int n)
        {
            Contract.Check(n == 42)?.Assert("n == 42");
        }
    }

    private string GetContractMessage(Action a)
    {
        try
        {
            a();
        }
        catch (Exception e) when (e.GetType().Name == "ContractException")
        {
            _helper.WriteLine(e.ToString());
            var message = e.Message;
            // Need to remove the line number from the message
            var index = message.LastIndexOf(".cs");
            return message.Substring(0, index);
        }

        throw new InvalidOperationException("The method should generate a contract exception, but it didn't.");
    }
}