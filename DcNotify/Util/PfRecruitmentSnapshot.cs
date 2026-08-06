namespace Dnc.Util;

public static class PfRecruitmentSnapshot
{
    private const int PartySlotCount = 8;

    private static readonly ulong[] SlotFlags = new ulong[PartySlotCount];
    private static bool active;

    public static bool IsActive => active;

    public static void Capture()
    {
        active = false;
        for (var i = 0; i < PartySlotCount; i++)
        {
            SlotFlags[i] = PfRoleResolver.GetSlotFlags(i);
            if (SlotFlags[i] != 0)
                active = true;
        }
    }

    public static void Clear()
    {
        active = false;
        System.Array.Clear(SlotFlags);
    }

    public static ulong GetSlotFlags(int slotIndex)
    {
        if (active && slotIndex is >= 0 and < PartySlotCount)
            return SlotFlags[slotIndex];

        return PfRoleResolver.GetSlotFlags(slotIndex);
    }

    public static bool HasPfLayout()
    {
        if (active)
            return true;

        for (var i = 0; i < PartySlotCount; i++)
        {
            if (PfRoleResolver.GetSlotFlags(i) != 0)
                return true;
        }

        return false;
    }
}
