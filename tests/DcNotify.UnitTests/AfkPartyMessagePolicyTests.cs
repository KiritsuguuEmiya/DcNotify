using Dnc;
using Dnc.Notifications;
using Dnc.Util;
using Xunit;

namespace DcNotify.UnitTests;

public class AfkPartyMessagePolicyTests
{
    [Fact]
    public void ShouldTrigger_RequiresEnabledFeatureAndAfk()
    {
        var config = new Configuration
        {
            Enabled = true,
            AfkPartyMessageEnabled = true,
        };

        Assert.True(AfkPartyMessagePolicy.ShouldTrigger(config, isClientAfk: true));
        Assert.False(AfkPartyMessagePolicy.ShouldTrigger(config, isClientAfk: false));
    }

    [Fact]
    public void ShouldTrigger_RequiresPluginEnabled()
    {
        var config = new Configuration
        {
            Enabled = false,
            AfkPartyMessageEnabled = true,
        };

        Assert.False(AfkPartyMessagePolicy.ShouldTrigger(config, isClientAfk: true));
    }

    [Fact]
    public void ShouldTrigger_RequiresFeatureToggle()
    {
        var config = new Configuration
        {
            Enabled = true,
            AfkPartyMessageEnabled = false,
        };

        Assert.False(AfkPartyMessagePolicy.ShouldTrigger(config, isClientAfk: true));
    }

    [Theory]
    [InlineData(8, true)]
    [InlineData(7, false)]
    public void IsPartyFull_MatchesSlotCount(int partyCount, bool expected)
    {
        var member = new CrossWorldPartyListSystem.CrossWorldMember { PartyCount = partyCount };

        Assert.Equal(expected, AfkPartyMessagePolicy.IsPartyFull(member));
    }

    [Theory]
    [InlineData(8, true)]
    [InlineData(7, false)]
    public void IsPartyNoLongerFullAfterLeave_OnlyWhenLeaveFromFullParty(int partyCount, bool expected)
    {
        var member = new CrossWorldPartyListSystem.CrossWorldMember { PartyCount = partyCount };

        Assert.Equal(expected, AfkPartyMessagePolicy.IsPartyNoLongerFullAfterLeave(member));
    }
}
