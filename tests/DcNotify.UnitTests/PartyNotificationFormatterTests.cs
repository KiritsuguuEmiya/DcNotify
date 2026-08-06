using Dnc.Notifications;
using Dnc.Util;
using Xunit;

namespace DcNotify.UnitTests;

public class PartyNotificationFormatterTests
{
    private static CrossWorldPartyListSystem.CrossWorldMember Member(int partyCount = 3)
        => new()
        {
            Name = "Test Player",
            PartyCount = partyCount,
            Level = 100,
            JobId = 19,
            ContentId = 1,
        };

    [Fact]
    public void FormatJoinTitle_PartyFull_ReturnsPartyFull()
        => Assert.Equal("Party full", PartyNotificationFormatter.FormatJoinTitle(Member(8), null));

    [Fact]
    public void FormatJoinTitle_WithRole_IncludesRoleLabel()
    {
        var snapshot = new JoinSnapshot(PfRoleGroup.Tank, 19, 62019);

        var title = PartyNotificationFormatter.FormatJoinTitle(Member(3), snapshot);

        Assert.Equal("3/8: Tank join", title);
    }

    [Fact]
    public void FormatJoinTitle_WithoutRole_UsesGenericLabel()
        => Assert.Equal("3/8: Party join", PartyNotificationFormatter.FormatJoinTitle(Member(3), null));

    [Fact]
    public void FormatLeaveTitle_WithRole_IncludesRoleLabel()
    {
        var snapshot = new JoinSnapshot(PfRoleGroup.Healer, 24, 62024);

        var title = PartyNotificationFormatter.FormatLeaveTitle(Member(4), remaining: 3, snapshot);

        Assert.Equal("3/8: Healer leave", title);
    }

    [Fact]
    public void FormatJoinDescription_PartyFull_IncludesRecruitmentEnded()
    {
        var description = PartyNotificationFormatter.FormatJoinDescription(Member(8), null, "PLD");

        Assert.Contains("joins the party.", description);
        Assert.Contains("Party recruitment ended.", description);
        Assert.Contains("**8/8 filled · 0 players remaining**", description);
    }

    [Fact]
    public void FormatJoinDescription_WithRole_IncludesRoleInMemberLine()
    {
        var snapshot = new JoinSnapshot(PfRoleGroup.Tank, 19, 62019);

        var description = PartyNotificationFormatter.FormatJoinDescription(Member(3), snapshot, "PLD");

        Assert.Contains("**Tank** — **Test Player** (Lv100 PLD)", description);
        Assert.Contains("**3/8 filled · 5 players remaining**", description);
    }

    [Fact]
    public void FormatLeaveDescription_IncludesMemberAndStatus()
    {
        var description = PartyNotificationFormatter.FormatLeaveDescription(Member(4), remaining: 3, null, "WHM");

        Assert.Contains("**Test Player** (Lv100 WHM) has left the party.", description);
        Assert.Contains("**3/8 filled · 5 players remaining**", description);
    }
}
