using Dnc;
using Xunit;

namespace DcNotify.UnitTests;

public class ConfigurationTests
{
    [Fact]
    public void ShouldNotifyForClassJob_AllMode_ReturnsTrue()
    {
        var config = new Configuration { ClassFilterMode = ClassFilterMode.All };

        Assert.True(config.ShouldNotifyForClassJob(19));
        Assert.True(config.ShouldNotifyForClassJob(999));
    }

    [Fact]
    public void ShouldNotifyForClassJob_NoneMode_ReturnsFalse()
    {
        var config = new Configuration { ClassFilterMode = ClassFilterMode.None };

        Assert.False(config.ShouldNotifyForClassJob(19));
    }

    [Fact]
    public void ShouldNotifyForClassJob_SelectedMode_OnlyMatchesSelected()
    {
        var config = new Configuration
        {
            ClassFilterMode = ClassFilterMode.Selected,
            SelectedClassJobIds = [19, 24],
        };

        Assert.True(config.ShouldNotifyForClassJob(19));
        Assert.True(config.ShouldNotifyForClassJob(24));
        Assert.False(config.ShouldNotifyForClassJob(20));
    }

    [Fact]
    public void ShouldNotifyForLeave_NoneMode_ReturnsFalse()
    {
        var config = new Configuration { ClassFilterMode = ClassFilterMode.None };

        Assert.False(config.ShouldNotifyForLeave());
    }

    [Theory]
    [InlineData(ClassFilterMode.All)]
    [InlineData(ClassFilterMode.Selected)]
    public void ShouldNotifyForLeave_NonNoneMode_ReturnsTrue(ClassFilterMode mode)
    {
        var config = new Configuration { ClassFilterMode = mode };

        Assert.True(config.ShouldNotifyForLeave());
    }

    [Fact]
    public void ToggleClassJob_SwitchesSelectionAndSetsSelectedMode()
    {
        var config = new Configuration { ClassFilterMode = ClassFilterMode.All };

        config.ToggleClassJob(19);
        Assert.Equal(ClassFilterMode.Selected, config.ClassFilterMode);
        Assert.True(config.IsClassJobSelected(19));

        config.ToggleClassJob(19);
        Assert.False(config.IsClassJobSelected(19));
    }

    [Fact]
    public void SetRoleGroup_AddsAndRemovesJobs()
    {
        var config = new Configuration();

        config.SetRoleGroup([19, 21], selected: true);
        Assert.Equal(ClassFilterMode.Selected, config.ClassFilterMode);
        Assert.True(config.IsRoleGroupFullySelected([19, 21]));
        Assert.False(config.IsRoleGroupFullySelected([19, 21, 32]));

        config.SetRoleGroup([21], selected: false);
        Assert.True(config.IsClassJobSelected(19));
        Assert.False(config.IsClassJobSelected(21));
    }

    [Fact]
    public void ClearClassJobSelection_SetsAllMode()
    {
        var config = new Configuration
        {
            ClassFilterMode = ClassFilterMode.Selected,
            SelectedClassJobIds = [19],
        };

        config.ClearClassJobSelection();

        Assert.Equal(ClassFilterMode.All, config.ClassFilterMode);
        Assert.Empty(config.SelectedClassJobIds);
    }

    [Fact]
    public void SetClassFilterNone_ClearsSelection()
    {
        var config = new Configuration
        {
            ClassFilterMode = ClassFilterMode.Selected,
            SelectedClassJobIds = [19],
        };

        config.SetClassFilterNone();

        Assert.Equal(ClassFilterMode.None, config.ClassFilterMode);
        Assert.Empty(config.SelectedClassJobIds);
    }
}
