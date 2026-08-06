using Dnc;
using Dnc.Notifications;
using Dnc.Util;
using Xunit;

namespace DcNotify.UnitTests;

public class PartyNotificationPolicyTests
{
    private static CrossWorldPartyListSystem.CrossWorldMember Member(int partyCount = 3, uint jobId = 19)
        => new()
        {
            Name = "Player",
            PartyCount = partyCount,
            Level = 100,
            JobId = jobId,
            ContentId = 1,
        };

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    public void ShouldNotify_RequiresEnabledAndAfk(bool enabled, bool isAfk, bool expected)
        => Assert.Equal(expected, PartyNotificationPolicy.ShouldNotify(enabled, isAfk));

    [Fact]
    public void ShouldNotifyJoin_PartyFull_BypassesFilter()
    {
        var config = new Configuration { ClassFilterMode = ClassFilterMode.None };

        Assert.True(PartyNotificationPolicy.ShouldNotifyJoin(config, Member(partyCount: 8)));
    }

    [Fact]
    public void ShouldNotifyJoin_UsesClassFilterWhenNotFull()
    {
        var config = new Configuration
        {
            ClassFilterMode = ClassFilterMode.Selected,
            SelectedClassJobIds = [19],
        };

        Assert.True(PartyNotificationPolicy.ShouldNotifyJoin(config, Member(jobId: 19)));
        Assert.False(PartyNotificationPolicy.ShouldNotifyJoin(config, Member(jobId: 24)));
    }

    [Fact]
    public void ShouldNotifyLeave_FalseWhenFilterNone()
    {
        var config = new Configuration { ClassFilterMode = ClassFilterMode.None };

        Assert.False(PartyNotificationPolicy.ShouldNotifyLeave(config));
    }

    [Fact]
    public void ShouldNotifyLeave_TrueWhenFilterNotNone()
    {
        var config = new Configuration { ClassFilterMode = ClassFilterMode.All };

        Assert.True(PartyNotificationPolicy.ShouldNotifyLeave(config));
    }
}
