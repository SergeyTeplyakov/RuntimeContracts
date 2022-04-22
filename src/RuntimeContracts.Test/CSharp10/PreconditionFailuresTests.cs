#define CONTRACTS_LIGHT_PRECONDITIONS_DEBUG
#define CONTRACTS_LIGHT_PRECONDITIONS
#define CONTRACTS_LIGHT_PRECONDITIONS_FOR_ALL

using System;
using System.Diagnostics.ContractsLight;
using FluentAssertions;
using Xunit;

namespace RuntimeContracts.Test.CSharp10;

public class PreconditionFailuresTests
{
    [Fact]
    public void RequiresFailure()
    {
        Action a = () => WillFail(null);
        string message = a.ShouldThrow();
        message.Should().Contain("Precondition failed (s != null): custom message .");

        a = () => WillFailWithNoMessage(null);
        message = a.ShouldThrow();
        message.Should().Contain("Precondition failed (s != null)");

        a = () => WillFailConstString(null);
        message = a.ShouldThrow();
        message.Should().Contain("Precondition failed (s != null): custom message.");

        static void WillFail(string s)
        {
            Contract.Requires(s != null, $"custom message {s}.");
        }

        static void WillFailConstString(string s)
        {
            Contract.Requires(s != null, "custom message.");
        }

        static void WillFailWithNoMessage(string s)
        {
            Contract.Requires(s != null);
        }
    }

    [Fact]
    public void RequiresGenericFailure()
    {
        Action a = () => WillFail(null);
        string message = a.ShouldThrow<ArgumentNullException>();
        message.Should().Contain("Precondition failed (s != null): custom message");

        a = () => WillFailWithNoMessage(null);
        message = a.ShouldThrow<ArgumentNullException>();
        message.Should().Contain("Precondition failed (s != null)");

        a = () => WillFailConstString(null);
        message = a.ShouldThrow<ArgumentNullException>();
        message.Should().Contain("Precondition failed (s != null): custom message.");

        static void WillFail(string s)
        {
            Contract.Requires<ArgumentNullException>(s != null, $"custom message {s}.");
        }

        static void WillFailConstString(string s)
        {
            Contract.Requires<ArgumentNullException>(s != null, "custom message.");
        }

        static void WillFailWithNoMessage(string s)
        {
            Contract.Requires<ArgumentNullException>(s != null);
        }
    }

    [Fact]
    public void RequiresDebugFailure()
    {
        Action a = () => WillFail(null);
        string message = a.ShouldThrow();
        message.Should().Contain("Precondition failed (s != null): custom message .");

        a = () => WillFailWithNoMessage(null);
        message = a.ShouldThrow();
        message.Should().Contain("Precondition failed (s != null)");

        a = () => WillFailConstString(null);
        message = a.ShouldThrow();
        message.Should().Contain("Precondition failed (s != null): custom message.");

        static void WillFail(string s)
        {
            Contract.Requires(s != null, $"custom message {s}.");
        }

        static void WillFailConstString(string s)
        {
            Contract.Requires(s != null, "custom message.");
        }

        static void WillFailWithNoMessage(string s)
        {
            Contract.Requires(s != null);
        }
    }
}