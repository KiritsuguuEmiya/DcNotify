using Dnc.Util;
using Xunit;

namespace DcNotify.UnitTests;

public class PartyCompositionBuilderTests
{
    [Fact]
    public void CountFilled_CountsFilledSlots()
    {
        var slots = new[]
        {
            new PartySlot(PartySlotKind.Filled, PfRoleGroup.Tank, 62019),
            new PartySlot(PartySlotKind.Empty, PfRoleGroup.Healer, 62024),
            new PartySlot(PartySlotKind.Omitted, null, 0),
            new PartySlot(PartySlotKind.Filled, PfRoleGroup.MeleeDps, 62020),
        };

        Assert.Equal(2, PartyCompositionBuilder.CountFilled(slots));
    }

    [Fact]
    public void BuildRandomSample_ReturnsEightSlots()
    {
        var slots = PartyCompositionBuilder.BuildRandomSample();

        Assert.Equal(8, slots.Length);
    }

    [Fact]
    public void BuildRandomSample_ContainsAtLeastOneFilledSlot()
    {
        var slots = PartyCompositionBuilder.BuildRandomSample();

        Assert.InRange(PartyCompositionBuilder.CountFilled(slots), 1, 8);
    }

    [Fact]
    public void BuildRandomSample_OnlyUsesFilledOrEmptyKinds()
    {
        var slots = PartyCompositionBuilder.BuildRandomSample();

        Assert.All(slots, slot => Assert.NotEqual(PartySlotKind.Omitted, slot.Kind));
    }
}
