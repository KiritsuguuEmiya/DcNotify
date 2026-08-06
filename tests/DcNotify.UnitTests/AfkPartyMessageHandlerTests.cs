using System;
using System.Threading;
using System.Threading.Tasks;
using Dnc;
using Dnc.Notifications;
using Dnc.Util;
using DcNotify.TestSupport;
using Xunit;

namespace DcNotify.UnitTests;

public class AfkPartyMessageHandlerTests
{
    private readonly FakePartyChatSender sender = new();
    private bool isClientAfk = true;

    private AfkPartyMessageHandler CreateHandler()
        => new(
            sender,
            (_, _) => Task.CompletedTask,
            () => isClientAfk);

    private static CrossWorldPartyListSystem.CrossWorldMember FullPartyMember(int partyCount = 8)
        => new() { PartyCount = partyCount };

    [Fact]
    public void PartyFull_SendsOnceAfterDelay()
    {
        var handler = CreateHandler();
        var config = EnabledConfig();

        handler.HandleJoin(FullPartyMember(), config, isClientAfk: true);

        Assert.Single(sender.Messages);
        Assert.Equal("brb in 5 minutes", sender.Messages[0]);
    }

    [Fact]
    public void PartyFull_DoesNotSendWhenDisabled()
    {
        var handler = CreateHandler();
        var config = EnabledConfig();
        config.AfkPartyMessageEnabled = false;

        handler.HandleJoin(FullPartyMember(), config, isClientAfk: true);

        Assert.Empty(sender.Messages);
    }

    [Fact]
    public void PartyFull_DoesNotSendWhenNotAfk()
    {
        var handler = CreateHandler();
        var config = EnabledConfig();

        handler.HandleJoin(FullPartyMember(), config, isClientAfk: false);

        Assert.Empty(sender.Messages);
    }

    [Fact]
    public void PartyFull_SendsOnlyOnceUntilPartyDropsBelowFull()
    {
        var handler = CreateHandler();
        var config = EnabledConfig();

        handler.HandleJoin(FullPartyMember(), config, isClientAfk: true);
        handler.HandleJoin(FullPartyMember(), config, isClientAfk: true);

        Assert.Single(sender.Messages);

        handler.HandleLeave(FullPartyMember());
        handler.HandleJoin(FullPartyMember(), config, isClientAfk: true);

        Assert.Equal(2, sender.Messages.Count);
    }

    [Fact]
    public void CancelPendingSend_WhenUserReturnsFromAfk()
    {
        var tcs = new TaskCompletionSource();
        var handler = new AfkPartyMessageHandler(
            sender,
            (_, token) =>
            {
                token.Register(() => tcs.TrySetResult());
                return tcs.Task;
            },
            () => isClientAfk);

        var config = EnabledConfig();
        config.AfkPartyMessageDelaySeconds = 30;

        handler.HandleJoin(FullPartyMember(), config, isClientAfk: true);
        isClientAfk = false;
        handler.OnFrameworkUpdate(isClientAfk: false);

        tcs.TrySetResult();
        Assert.Empty(sender.Messages);
    }

    [Fact]
    public void SendTestMessage_SendsImmediatelyWhenPluginEnabled()
    {
        var handler = CreateHandler();
        var config = EnabledConfig();
        config.AfkPartyMessageEnabled = false;
        config.AfkPartyMessageTemplate = "coming soon";

        var result = handler.SendTestMessage(config);

        Assert.True(result.Success);
        Assert.Equal("coming soon", sender.Messages[0]);
    }

    [Fact]
    public void SendTestMessage_FailsWhenPluginDisabled()
    {
        var handler = CreateHandler();
        var config = EnabledConfig();
        config.Enabled = false;

        var result = handler.SendTestMessage(config);

        Assert.False(result.Success);
        Assert.Empty(sender.Messages);
    }

    private static Configuration EnabledConfig()
        => new()
        {
            Enabled = true,
            AfkPartyMessageEnabled = true,
            AfkPartyMessageTemplate = "brb in 5 minutes",
        };
}
