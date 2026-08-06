using System;
using System.Linq;
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

    private static readonly PfRoleGroup[] SampleLayout =
    [
        PfRoleGroup.Tank,
        PfRoleGroup.Tank,
        PfRoleGroup.Healer,
        PfRoleGroup.Healer,
        PfRoleGroup.MeleeDps,
        PfRoleGroup.PhysicalRangedDps,
        PfRoleGroup.MagicalRangedDps,
        PfRoleGroup.MeleeDps,
    ];

    public static PartySlot[] Build()
        => PfRecruitmentSnapshot.HasPfLayout() ? BuildFromPf() : BuildFromPartyList();

    public static PartySlot[] BuildRandomSample()
    {
        var slots = new PartySlot[PartySlotCount];
        var filledCount = Random.Shared.Next(1, PartySlotCount + 1);
        var filledIndices = Enumerable.Range(0, PartySlotCount)
            .OrderBy(_ => Random.Shared.Next())
            .Take(filledCount)
            .ToHashSet();

        for (var i = 0; i < PartySlotCount; i++)
        {
            var role = SampleLayout[i];
            if (filledIndices.Contains(i))
            {
                slots[i] = new PartySlot(PartySlotKind.Filled, role, ClassJobRegistry.PickRandomJobIconForRole(role));
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

    public static int CountFilled(PartySlot[] slots)
        => slots.Count(s => s.Kind == PartySlotKind.Filled);

    private static PartySlot[] BuildFromPf()
    {
        var slots = new PartySlot[PartySlotCount];

        for (var i = 0; i < PartySlotCount; i++)
        {
            var slotFlags = PfRecruitmentSnapshot.GetSlotFlags(i);
            if (slotFlags == 0)
            {
                slots[i] = new PartySlot(PartySlotKind.Omitted, null, 0);
                continue;
            }

            var role = ClassJobRegistry.GetRoleFromSlotFlags(slotFlags);
            var contentId = PfRoleResolver.GetMemberContentId(i);

            if (contentId != 0)
            {
                var iconId = ResolveMemberIconId(contentId);
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

    private static unsafe PartySlot[] BuildFromPartyList()
    {
        var slots = new PartySlot[PartySlotCount];
        var partyCount = InfoProxyCrossRealm.GetPartyMemberCount();

        for (var i = 0; i < PartySlotCount; i++)
        {
            if (i >= partyCount)
            {
                slots[i] = new PartySlot(PartySlotKind.Omitted, null, 0);
                continue;
            }

            var member = InfoProxyCrossRealm.GetGroupMember((uint)i);
            if (member == null)
            {
                slots[i] = new PartySlot(PartySlotKind.Omitted, null, 0);
                continue;
            }

            var contentId = member->ContentId;
            var jobId = member->ClassJobId;
            var iconId = ResolveMemberIconId(contentId, jobId);
            var role = ClassJobRegistry.GetRoleGroup(jobId);

            slots[i] = new PartySlot(PartySlotKind.Filled, role, iconId);
        }

        return slots;
    }

    private static uint ResolveMemberIconId(ulong contentId, uint fallbackJobId = 0)
    {
        var snapshot = PfRoleTracker.Get(contentId, string.Empty);
        if (snapshot?.JoinJobIconId is > 0)
            return snapshot.Value.JoinJobIconId;

        if (fallbackJobId != 0)
            return ClassJobRegistry.GetClassJobIconId(fallbackJobId);

        return ResolveLiveJobIcon(contentId);
    }

    private static unsafe uint ResolveLiveJobIcon(ulong contentId)
    {
        var member = InfoProxyCrossRealm.GetMemberByContentId(contentId);
        if (member == null)
            return 0;

        return ClassJobRegistry.GetClassJobIconId(member->ClassJobId);
    }
}
