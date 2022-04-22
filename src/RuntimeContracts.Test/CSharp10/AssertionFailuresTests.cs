#define CONTRACTS_LIGHT_ASSERTS_DEBUG

#define CONTRACTS_LIGHT_ASSERTS
#define CONTRACTS_LIGHT_ASSERTS_FOR_ALL

using System;
using System.Diagnostics.ContractsLight;
using FluentAssertions;
using Xunit;

namespace RuntimeContracts.Test.CSharp10;

public class AssertionFailuresTests
{
    [Fact]
    public void AssertionFailure()
    {
        Action a = () => WillFail(null);
        string message = a.ShouldThrow();
        message.Should().Contain("Assertion failed (s != null): custom message .");

        a = () => WillFailWithNoMessage(null);
        message = a.ShouldThrow();
        message.Should().Contain("Assertion failed (s != null)");

        a = () => WillFailConstString(null);
        message = a.ShouldThrow();
        message.Should().Contain("Assertion failed (s != null): custom message.");

        static void WillFail(string s)
        {
            Contract.Assert(s != null, $"custom message {s}.");
        }

        static void WillFailConstString(string s)
        {
            Contract.Assert(s != null, "custom message.");
        }

        static void WillFailWithNoMessage(string s)
        {
            Contract.Assert(s != null);
        }
    }

    [Fact]
    public void AssertionDebugFailure()
    {
        Action a = () => WillFail(null);
        string message = a.ShouldThrow();
        message.Should().Contain("Assertion failed (s != null): custom message .");

        a = () => WillFailWithNoMessage(null);
        message = a.ShouldThrow();
        message.Should().Contain("Assertion failed (s != null)");

        a = () => WillFailConstString(null);
        message = a.ShouldThrow();
        message.Should().Contain("Assertion failed (s != null): custom message.");

        static void WillFail(string s)
        {
            Contract.AssertDebug(s != null, $"custom message {s}.");
        }

        static void WillFailConstString(string s)
        {
            Contract.AssertDebug(s != null, "custom message.");
        }

        static void WillFailWithNoMessage(string s)
        {
            Contract.AssertDebug(s != null);
        }
    }
}