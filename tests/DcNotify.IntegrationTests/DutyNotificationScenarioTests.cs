using System;
using Dnc;
using Dnc.Notifications;
using DcNotify.TestSupport;
using Xunit;

namespace DcNotify.IntegrationTests;

public sealed class DutyNotificationScenarioTests : IDisposable
{
    private readonly FakeNotificationSink sink = new();
    private readonly DutyNotificationHandler handler;
    private readonly Configuration config = new();

    public DutyNotificationScenarioTests()
        => handler = new DutyNotificationHandler(sink);

    public void Dispose() => sink.Clear();

    [Fact]
    public void DutyPop_EnabledAndAfk_Delivers()
    {
        config.Enabled = true;
        config.EnableForDutyPops = true;

        handler.Handle(default, config, isClientAfk: true);

        Assert.Single(sink.Deliveries);
        Assert.Equal("Duty pop", sink.Deliveries[0].Title);
        Assert.Contains("Duty registered:", sink.Deliveries[0].Text);
    }

    [Fact]
    public void DutyPop_Disabled_DoesNotDeliver()
    {
        config.Enabled = false;
        config.EnableForDutyPops = true;

        handler.Handle(default, config, isClientAfk: true);

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void DutyPop_NotAfk_DoesNotDeliver()
    {
        config.Enabled = true;
        config.EnableForDutyPops = true;

        handler.Handle(default, config, isClientAfk: false);

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void DutyPop_DutyPopsDisabled_DoesNotDeliver()
    {
        config.Enabled = true;
        config.EnableForDutyPops = false;

        handler.Handle(default, config, isClientAfk: true);

        Assert.Empty(sink.Deliveries);
    }
}
