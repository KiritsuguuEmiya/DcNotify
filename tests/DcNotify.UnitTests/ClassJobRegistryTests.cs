using Dnc.Util;
using Xunit;

namespace DcNotify.UnitTests;

public class ClassJobRegistryTests
{
    [Theory]
    [InlineData(PfRoleGroup.Tank, "Tank")]
    [InlineData(PfRoleGroup.Healer, "Healer")]
    [InlineData(PfRoleGroup.MeleeDps, "Melee DPS")]
    [InlineData(PfRoleGroup.PhysicalRangedDps, "Physical Ranged DPS")]
    [InlineData(PfRoleGroup.MagicalRangedDps, "Magical Ranged DPS")]
    [InlineData(PfRoleGroup.Free, "Free")]
    public void GetRoleLabel_ReturnsExpectedLabel(PfRoleGroup role, string expected)
        => Assert.Equal(expected, ClassJobRegistry.GetRoleLabel(role));

    [Theory]
    [InlineData(19u, 62019u)]
    [InlineData(42u, 62042u)]
    [InlineData(0u, 0u)]
    [InlineData(43u, 0u)]
    public void GetClassJobIconId_ReturnsExpectedIcon(uint rowId, uint expected)
        => Assert.Equal(expected, ClassJobRegistry.GetClassJobIconId(rowId));

    [Fact]
    public void GetRoleFromSlotFlags_SingleTankJob_ReturnsTank()
    {
        var flags = 1ul << 19;

        Assert.Equal(PfRoleGroup.Tank, ClassJobRegistry.GetRoleFromSlotFlags(flags));
    }

    [Fact]
    public void GetRoleFromSlotFlags_MultipleRoleGroups_ReturnsFree()
    {
        var flags = (1ul << 19) | (1ul << 24);

        Assert.Equal(PfRoleGroup.Free, ClassJobRegistry.GetRoleFromSlotFlags(flags));
    }

    [Fact]
    public void GetRoleFromSlotFlags_EmptyFlags_ReturnsNull()
        => Assert.Null(ClassJobRegistry.GetRoleFromSlotFlags(0));

    [Fact]
    public void GetRoleFromSlotFlags_ManyJobs_ReturnsFree()
    {
        ulong flags = 0;
        for (uint jobId = 1; jobId <= 20; jobId++)
            flags |= 1ul << (int)jobId;

        Assert.Equal(PfRoleGroup.Free, ClassJobRegistry.GetRoleFromSlotFlags(flags));
    }

    [Theory]
    [InlineData(PfRoleGroup.Tank, 62019u)]
    [InlineData(PfRoleGroup.Healer, 62024u)]
    [InlineData(PfRoleGroup.MeleeDps, 62020u)]
    public void GetRolePlaceholderIconId_ReturnsRoleIcon(PfRoleGroup role, uint expected)
        => Assert.Equal(expected, ClassJobRegistry.GetRolePlaceholderIconId(role));
}
