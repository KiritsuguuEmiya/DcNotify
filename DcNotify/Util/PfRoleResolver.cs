using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Dnc.Util;

public static class PfRoleResolver
{
    public static unsafe int FindSlotIndex(ulong contentId, byte memberIndex)
    {
        var lfg = AgentLookingForGroup.Instance();
        if (lfg == null)
            return memberIndex < PartyConstants.SlotCount ? memberIndex : -1;

        var memberIds = lfg->StoredRecruitmentInfo.MemberContentIds;
        if (contentId != 0)
        {
            for (var i = 0; i < PartyConstants.SlotCount; i++)
            {
                if (memberIds[i] == contentId)
                    return i;
            }
        }

        return memberIndex < PartyConstants.SlotCount ? memberIndex : -1;
    }

    public static unsafe PfRoleGroup? ResolveSlotRole(ulong contentId, byte memberIndex)
    {
        var slotIndex = FindSlotIndex(contentId, memberIndex);
        if (slotIndex < 0)
            return null;

        return GetSlotRole(slotIndex);
    }

    public static unsafe PfRoleGroup? GetSlotRole(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PartyConstants.SlotCount)
            return null;

        var lfg = AgentLookingForGroup.Instance();
        if (lfg == null)
            return null;

        return ClassJobRegistry.GetRoleFromSlotFlags(lfg->StoredRecruitmentInfo.SlotFlags[slotIndex]);
    }

    public static unsafe ulong GetSlotFlags(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PartyConstants.SlotCount)
            return 0;

        var lfg = AgentLookingForGroup.Instance();
        if (lfg == null)
            return 0;

        return lfg->StoredRecruitmentInfo.SlotFlags[slotIndex];
    }

    public static unsafe ulong GetMemberContentId(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PartyConstants.SlotCount)
            return 0;

        var lfg = AgentLookingForGroup.Instance();
        if (lfg == null)
            return 0;

        return lfg->StoredRecruitmentInfo.MemberContentIds[slotIndex];
    }
}
