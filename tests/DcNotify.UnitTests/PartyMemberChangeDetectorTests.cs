using System.Linq;
using Dnc.Util;
using Xunit;

namespace DcNotify.UnitTests;

public class PartyMemberChangeDetectorTests
{
    private static CrossWorldPartyListSystem.CrossWorldMember Member(string name, ulong contentId = 0)
        => new()
        {
            Name = name,
            PartyCount = 2,
            Level = 100,
            JobId = 19,
            ContentId = contentId,
        };

    [Fact]
    public void MembersEqual_PrefersContentId()
    {
        var a = Member("Alpha", contentId: 10);
        var b = Member("Beta", contentId: 10);

        Assert.True(PartyMemberChangeDetector.MembersEqual(a, b));
    }

    [Fact]
    public void MembersEqual_FallsBackToNameWhenContentIdMissing()
    {
        var a = Member("Alpha");
        var b = Member("Alpha");

        Assert.True(PartyMemberChangeDetector.MembersEqual(a, b));
    }

    [Fact]
    public void DetectJoins_ReturnsMembersNotInPrevious()
    {
        var previous = new[] { Member("Alpha", 1), Member("Beta", 2) };
        var current = new[] { Member("Alpha", 1), Member("Beta", 2), Member("Gamma", 3) };

        var joins = PartyMemberChangeDetector.DetectJoins(current, previous).ToList();

        Assert.Single(joins);
        Assert.Equal("Gamma", joins[0].Name);
    }

    [Fact]
    public void DetectLeaves_ReturnsMembersMissingFromCurrent()
    {
        var previous = new[] { Member("Alpha", 1), Member("Beta", 2) };
        var current = new[] { Member("Alpha", 1) };

        var leaves = PartyMemberChangeDetector.DetectLeaves(current, previous).ToList();

        Assert.Single(leaves);
        Assert.Equal("Beta", leaves[0].Name);
    }
}
