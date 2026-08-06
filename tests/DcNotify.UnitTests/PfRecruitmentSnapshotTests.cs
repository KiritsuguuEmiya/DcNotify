using Dnc.Util;
using Xunit;

namespace DcNotify.UnitTests;

public class PfRecruitmentSnapshotTests
{
    public PfRecruitmentSnapshotTests()
    {
        PfRecruitmentSnapshot.Clear();
    }

    [Fact]
    public void Clear_ResetsActiveState()
    {
        PfRecruitmentSnapshot.Clear();

        Assert.False(PfRecruitmentSnapshot.IsActive);
    }
}
