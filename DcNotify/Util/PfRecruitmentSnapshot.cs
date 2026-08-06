namespace Dnc.Util;

public static class PfRecruitmentSnapshot
{
    private static readonly ulong[] SlotFlags = new ulong[PartyConstants.SlotCount];
    private static readonly ulong[] MemberContentIds = new ulong[PartyConstants.SlotCount];
    private static bool active;

    public static bool IsActive => active;

    public static void Capture()
    {
        active = false;
        for (var i = 0; i < PartyConstants.SlotCount; i++)
        {
            SlotFlags[i] = PfRoleResolver.GetSlotFlags(i);
            MemberContentIds[i] = PfRoleResolver.GetMemberContentId(i);
            if (SlotFlags[i] != 0)
                active = true;
        }
    }

    public static void RecordMember(int slotIndex, ulong contentId)
    {
        if (!active || slotIndex is < 0 or >= PartyConstants.SlotCount || contentId == 0)
            return;

        MemberContentIds[slotIndex] = contentId;
    }

    public static void Clear()
    {
        active = false;
        System.Array.Clear(SlotFlags);
        System.Array.Clear(MemberContentIds);
    }

    public static ulong GetSlotFlags(int slotIndex)
    {
        if (active && slotIndex is >= 0 and < PartyConstants.SlotCount)
            return SlotFlags[slotIndex];

        return PfRoleResolver.GetSlotFlags(slotIndex);
    }

    public static ulong GetMemberContentId(int slotIndex)
    {
        if (slotIndex is < 0 or >= PartyConstants.SlotCount)
            return 0;

        var live = PfRoleResolver.GetMemberContentId(slotIndex);
        if (live != 0)
            return live;

        if (active)
            return MemberContentIds[slotIndex];

        return 0;
    }

    public static bool HasPfLayout()
    {
        if (active)
            return true;

        for (var i = 0; i < PartyConstants.SlotCount; i++)
        {
            if (PfRoleResolver.GetSlotFlags(i) != 0)
                return true;
        }

        return false;
    }
}
