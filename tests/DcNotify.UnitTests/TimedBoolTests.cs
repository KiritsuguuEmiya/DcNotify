using System.Threading;
using Dnc.Util;
using Xunit;

namespace DcNotify.UnitTests;

public class TimedBoolTests
{
    [Fact]
    public void Value_BeforeExpiry_ReturnsTrue()
    {
        var timed = new TimedBool(10f).Start();

        Assert.True(timed.Value);
    }

    [Fact]
    public void Value_AfterStop_ReturnsFalse()
    {
        var timed = new TimedBool(10f).Start().Stop();

        Assert.False(timed.Value);
    }

    [Fact]
    public void Value_AfterExpiry_ReturnsFalse()
    {
        var timed = new TimedBool(0.001f).Start();
        Thread.Sleep(20);

        Assert.False(timed.Value);
    }
}
