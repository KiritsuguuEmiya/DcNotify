using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace Dnc.Util;

public enum PartySlotKind
{
    Omitted,
    Empty,
    Filled,
}

public readonly record struct PartySlot(PartySlotKind Kind, PfRoleGroup? Role, uint IconId);

public static class PartyCompositionBuilder
{
    private const int PartySlotCount = 8;

    public static PartySlot[] Build()
    {
        var slots = new PartySlot[PartySlotCount];

        for (var i = 0; i < PartySlotCount; i++)
        {
            var slotFlags = PfRoleResolver.GetSlotFlags(i);
            if (slotFlags == 0)
            {
                slots[i] = new PartySlot(PartySlotKind.Omitted, null, 0);
                continue;
            }

            var role = ClassJobRegistry.GetRoleFromSlotFlags(slotFlags);
            var contentId = PfRoleResolver.GetMemberContentId(i);

            if (contentId != 0)
            {
                var snapshot = PfRoleTracker.Get(contentId, string.Empty);
                var iconId = snapshot?.JoinJobIconId ?? 0;
                if (iconId == 0)
                    iconId = ResolveLiveJobIcon(contentId);

                slots[i] = new PartySlot(PartySlotKind.Filled, role, iconId);
            }
            else
            {
                slots[i] = new PartySlot(
                    PartySlotKind.Empty,
                    role,
                    ClassJobRegistry.GetRolePlaceholderIconId(role));
            }
        }

        return slots;
    }

    private static unsafe uint ResolveLiveJobIcon(ulong contentId)
    {
        var member = InfoProxyCrossRealm.GetMemberByContentId(contentId);
        if (member == null)
            return 0;

        return ClassJobRegistry.GetClassJobIconId(member->ClassJobId);
    }
}
