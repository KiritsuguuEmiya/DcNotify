using Dnc;
using Dnc.Notifications;
using DcNotify.TestSupport;
using Dalamud.Game.Text;
using Xunit;

namespace DcNotify.UnitTests;

public class PartyChatNotificationHandlerTests
{
    private readonly FakeNotificationSink sink = new();
    private readonly PartyChatNotificationHandler handler;
    private readonly Configuration config = new();

    public PartyChatNotificationHandlerTests()
    {
        handler = new PartyChatNotificationHandler(sink);
    }

    [Fact]
    public void Disabled_DoesNotDeliver()
    {
        config.Enabled = false;
        config.NotifyOnPartyChatMessages = true;

        handler.Handle("Player", "hello", config, isClientAfk: true, localPlayerName: "Me");

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void ToggleOff_DoesNotDeliver()
    {
        config.Enabled = true;
        config.NotifyOnPartyChatMessages = false;

        handler.Handle("Player", "hello", config, isClientAfk: true, localPlayerName: "Me");

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void NotAfk_DoesNotDeliver()
    {
        config.Enabled = true;
        config.NotifyOnPartyChatMessages = true;

        handler.Handle("Player", "hello", config, isClientAfk: false, localPlayerName: "Me");

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void OwnMessage_DoesNotDeliver()
    {
        config.Enabled = true;
        config.NotifyOnPartyChatMessages = true;

        handler.Handle("Me", "hello", config, isClientAfk: true, localPlayerName: "Me");

        Assert.Empty(sink.Deliveries);
    }

    [Fact]
    public void PartyMessage_DeliversFormattedNotification()
    {
        config.Enabled = true;
        config.NotifyOnPartyChatMessages = true;

        handler.Handle("Player", "brb 5 min", config, isClientAfk: true, localPlayerName: "Me");

        Assert.Single(sink.Deliveries);
        Assert.Equal("Party chat", sink.Deliveries[0].Title);
        Assert.Equal("**Player**: brb 5 min", sink.Deliveries[0].Text);
    }

    [Theory]
    [InlineData(XivChatType.Party, true)]
    [InlineData(XivChatType.CrossParty, true)]
    [InlineData(XivChatType.Say, false)]
    public void IsPartyChatType_OnlyAcceptsPartyChannels(XivChatType type, bool expected)
        => Assert.Equal(expected, PartyChatNotificationPolicy.IsPartyChatType(type));
}
