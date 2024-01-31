using Xunit;
using WalletConnectSharp.Sign.Models;

namespace WalletConnectSharp.Sign.Test;

public class NamespaceTests
{
    [Fact, Trait("Category", "unit")]
    public void WithMethod_AppendsNewMethod()
    {
        var ns = new Namespace();
        ns.WithMethod("newMethod");

        Assert.Contains("newMethod", ns.Methods);
    }

    [Fact, Trait("Category", "unit"), Trait("Category", "unit")]
    public void WithChain_AppendsNewChain_WhenChainsIsNull()
    {
        var ns = new Namespace();
        ns.WithChain("newChain");

        Assert.Contains("newChain", ns.Chains);
    }

    [Fact, Trait("Category", "unit"), Trait("Category", "unit")]
    public void WithChain_AppendsNewChain_WhenChainsIsNotNull()
    {
        var ns = new Namespace();
        ns.WithChain("existingChain");
        ns.WithChain("newChain");

        Assert.Contains("newChain", ns.Chains);
    }

    [Fact, Trait("Category", "unit")]
    public void WithEvent_AppendsNewEvent_WhenEventsIsNull()
    {
        var ns = new Namespace();
        ns.WithEvent("newEvent");

        Assert.Contains("newEvent", ns.Events);
    }

    [Fact, Trait("Category", "unit")]
    public void WithEvent_AppendsNewEvent_WhenEventsIsNotNull()
    {
        var ns = new Namespace();
        ns.WithEvent("existingEvent");
        ns.WithEvent("newEvent");

        Assert.Contains("newEvent", ns.Events);
    }

    [Fact, Trait("Category", "unit")]
    public void WithAccount_AppendsNewAccount_WhenAccountsIsNull()
    {
        var ns = new Namespace();
        ns.WithAccount("newAccount");

        Assert.Contains("newAccount", ns.Accounts);
    }

    [Fact, Trait("Category", "unit")]
    public void WithAccount_AppendsNewAccount_WhenAccountsIsNotNull()
    {
        var ns = new Namespace();
        ns.WithAccount("existingAccount");
        ns.WithAccount("newAccount");

        Assert.Contains("newAccount", ns.Accounts);
    }

    [Fact, Trait("Category", "unit")]
    public void Equals_ReturnsTrue_WhenNamespacesAreIdentical()
    {
        var ns1 = new Namespace();
        ns1.WithMethod("method").WithChain("chain").WithEvent("event").WithAccount("account");

        var ns2 = new Namespace();
        ns2.WithMethod("method").WithChain("chain").WithEvent("event").WithAccount("account");

        Assert.True(ns1.Equals(ns2));
    }

    [Fact, Trait("Category", "unit")]
    public void Equals_ReturnsFalse_WhenNamespacesAreDifferent()
    {
        var ns1 = new Namespace();
        ns1.WithMethod("method1").WithChain("chain1").WithEvent("event1").WithAccount("account1");

        var ns2 = new Namespace();
        ns2.WithMethod("method2").WithChain("chain2").WithEvent("event2").WithAccount("account2");

        Assert.False(ns1.Equals(ns2));
    }
}
