using Dnc.Util;
using Xunit;

namespace DcNotify.UnitTests;

public class PfRoleTrackerTests
{
    public PfRoleTrackerTests()
    {
        PfRoleTracker.Clear();
    }

    [Fact]
    public void RecordJoin_GetByContentId_ReturnsSnapshot()
    {
        PfRoleTracker.RecordJoin(123, "Player", 19, PfRoleGroup.Tank);

        var snapshot = PfRoleTracker.Get(123, "Other Name");

        Assert.NotNull(snapshot);
        Assert.Equal(PfRoleGroup.Tank, snapshot.Value.SlotRole);
        Assert.Equal(19u, snapshot.Value.JoinJobId);
        Assert.Equal(62019u, snapshot.Value.JoinJobIconId);
    }

    [Fact]
    public void RecordJoin_GetByName_WhenContentIdMissing()
    {
        PfRoleTracker.RecordJoin(0, "Player", 24, PfRoleGroup.Healer);

        var snapshot = PfRoleTracker.Get(0, "Player");

        Assert.NotNull(snapshot);
        Assert.Equal(PfRoleGroup.Healer, snapshot.Value.SlotRole);
    }

    [Fact]
    public void Get_PrefersContentIdOverName()
    {
        PfRoleTracker.RecordJoin(1, "Alpha", 19, PfRoleGroup.Tank);
        PfRoleTracker.RecordJoin(2, "Beta", 24, PfRoleGroup.Healer);

        var snapshot = PfRoleTracker.Get(1, "Beta");

        Assert.Equal(PfRoleGroup.Tank, snapshot?.SlotRole);
    }

    [Fact]
    public void Remove_ClearsBothKeys()
    {
        PfRoleTracker.RecordJoin(42, "Player", 19, PfRoleGroup.Tank);

        PfRoleTracker.Remove(42, "Player");

        Assert.Null(PfRoleTracker.Get(42, "Player"));
    }

    [Fact]
    public void Clear_RemovesAllSnapshots()
    {
        PfRoleTracker.RecordJoin(1, "A", 19, PfRoleGroup.Tank);
        PfRoleTracker.RecordJoin(2, "B", 24, PfRoleGroup.Healer);

        PfRoleTracker.Clear();

        Assert.Null(PfRoleTracker.Get(1, "A"));
        Assert.Null(PfRoleTracker.Get(2, "B"));
    }
}
