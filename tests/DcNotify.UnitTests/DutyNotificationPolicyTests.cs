using Dnc;
using Dnc.Notifications;
using Xunit;

namespace DcNotify.UnitTests;

public class DutyNotificationPolicyTests
{
    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, false, false)]
    public void ShouldNotify_RequiresAllConditions(bool enabled, bool dutyPops, bool isAfk, bool expected)
    {
        var config = new Configuration
        {
            Enabled = enabled,
            EnableForDutyPops = dutyPops,
        };

        Assert.Equal(expected, DutyNotificationPolicy.ShouldNotify(config, isAfk));
    }
}
