using Dnc.Delivery;
using Xunit;

namespace DcNotify.UnitTests;

public class GamePartyChatSenderTests
{
    [Fact]
    public void Sanitize_TrimsAndTruncates()
    {
        var longMessage = new string('a', 250);

        var sanitized = GamePartyChatSender.Sanitize($"  {longMessage}  ");

        Assert.Equal(GamePartyChatSender.MaxMessageLength, sanitized.Length);
        Assert.False(sanitized.StartsWith(' '));
    }

    [Fact]
    public void Sanitize_ReplacesNewlines()
    {
        Assert.Equal("hello world", GamePartyChatSender.Sanitize("hello\r\nworld"));
    }
}
