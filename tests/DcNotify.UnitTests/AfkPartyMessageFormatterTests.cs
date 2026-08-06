using Dnc;
using Dnc.Notifications;
using Xunit;

namespace DcNotify.UnitTests;

public class AfkPartyMessageFormatterTests
{
    [Fact]
    public void Format_ReturnsConfiguredMessage()
    {
        var config = new Configuration
        {
            AfkPartyMessageTemplate = "sorry, running late",
        };

        Assert.Equal("sorry, running late", AfkPartyMessageFormatter.Format(config));
    }

    [Fact]
    public void Format_UsesDefaultWhenBlank()
    {
        var config = new Configuration
        {
            AfkPartyMessageTemplate = "   ",
        };

        Assert.Equal(AfkPartyMessageFormatter.DefaultMessage, AfkPartyMessageFormatter.Format(config));
    }

    [Fact]
    public void Format_TrimsWhitespace()
    {
        var config = new Configuration
        {
            AfkPartyMessageTemplate = "  brb  ",
        };

        Assert.Equal("brb", AfkPartyMessageFormatter.Format(config));
    }
}
