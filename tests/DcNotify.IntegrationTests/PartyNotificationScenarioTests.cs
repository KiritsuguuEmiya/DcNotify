using System;
using Dnc;
using Dnc.Notifications;
using Dnc.Util;
using DcNotify.TestSupport;
using Xunit;

namespace DcNotify.IntegrationTests;

public sealed class PartyNotificationScenarioTests : IDisposable
{
    private readonly FakeNotificationSink sink = new();
    private readonly PartyNotificationHandler handler;
    private readonly Configuration config = new();

    public PartyNotificationScenarioTests()
    {
        PfRoleTracker.Clear();
        handler = new PartyNotificationHandler(sink, jobId => jobId switch
        {
            19 => "PLD",
            24 => "WHM",
            _ => "???",
        });
    }

    public void Dispose()
    {
        PfRoleTracker.Clear();
        sink.Clear();
    }

    private static CrossWorldPartyListSystem.CrossWorldMember Member(
        uint jobId = 19,
        int partyCount = 3,
        string name = "Test Player",
        ulong contentId = 100)
        => new()
        {
            Name = name,
            PartyCount = partyCount,
            Level = 100,
            JobId = jobId,
            ContentId = contentId,
        };

    [Fact]
    public void Join_FilterMatchesJob_DeliversNotification()
    {
        config.ClassFilterMode = ClassFilterMode.Selected;
        config.SelectedClassJobIds = [19];

        handler.HandleJoin(Member(jobId: 19), config, isClientAfk: true, includeComposition: false);

        Assert.Single(sink.Deliveries);
        Assert.Equal("3/8: Party join", sink.Deliveries[0].Title);
        Assert.Contains("joins the party.", sink.Deliveries[0].Text);
    }

    [Fact]
    public void Join_FilterExcludesJob_DoesNotDeliver()
    {
        config.ClassFilterMode = ClassFilterMode.Selected;
        config.SelectedClassJobIds = [24];

        handler.HandleJoin(Member(jobId: 19), config, isClientAfk: true, includeComposition: false);

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void Join_PartyFull_BypassesFilter()
    {
        config.ClassFilterMode = ClassFilterMode.None;

        handler.HandleJoin(Member(jobId: 19, partyCount: 8), config, isClientAfk: true, includeComposition: false);

        Assert.Single(sink.Deliveries);
        Assert.Equal("Party full", sink.Deliveries[0].Title);
        Assert.Contains("Party recruitment ended.", sink.Deliveries[0].Text);
    }

    [Fact]
    public void Leave_DeliversWithoutClassFilter()
    {
        config.ClassFilterMode = ClassFilterMode.Selected;
        config.SelectedClassJobIds = [24];

        handler.HandleLeave(Member(jobId: 19, partyCount: 4), config, isClientAfk: true, includeComposition: false);

        Assert.Single(sink.Deliveries);
        Assert.Equal("3/8: Party leave", sink.Deliveries[0].Title);
    }

    [Fact]
    public void Leave_FilterNone_DoesNotDeliver()
    {
        config.ClassFilterMode = ClassFilterMode.None;

        handler.HandleLeave(Member(partyCount: 4), config, isClientAfk: true, includeComposition: false);

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void Disabled_DoesNotDeliver()
    {
        config.Enabled = false;
        config.ClassFilterMode = ClassFilterMode.All;

        handler.HandleJoin(Member(), config, isClientAfk: true, includeComposition: false);
        handler.HandleLeave(Member(partyCount: 4), config, isClientAfk: true, includeComposition: false);

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void NotAfk_DoesNotDeliver()
    {
        config.ClassFilterMode = ClassFilterMode.All;

        handler.HandleJoin(Member(), config, isClientAfk: false, includeComposition: false);

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void Join_WithPfRole_IncludesRoleInTitle()
    {
        config.ClassFilterMode = ClassFilterMode.All;
        PfRoleTracker.RecordJoin(100, "Test Player", 19, PfRoleGroup.Tank);

        handler.HandleJoin(Member(jobId: 19), config, isClientAfk: true, includeComposition: false);

        Assert.Single(sink.Deliveries);
        Assert.Equal("3/8: Tank join", sink.Deliveries[0].Title);
        Assert.Contains("**Tank** — **Test Player** (Lv100 PLD)", sink.Deliveries[0].Text);
    }
}
