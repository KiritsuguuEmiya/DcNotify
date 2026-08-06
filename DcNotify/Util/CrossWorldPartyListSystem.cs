using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace Dnc.Util;

public static class CrossWorldPartyListSystem
{
    public struct CrossWorldMember
    {
        public string Name;
        public int PartyCount;
        public uint Level;
        public uint JobId;
        public ulong ContentId;
        public byte MemberIndex;
    }

    public delegate void CrossWorldJoinDelegate(CrossWorldMember m);
    public delegate void CrossWorldLeaveDelegate(CrossWorldMember m);

    public static event CrossWorldJoinDelegate? OnJoin;
    public static event CrossWorldLeaveDelegate? OnLeave;

    private static readonly List<CrossWorldMember> members = new();
    private static List<CrossWorldMember> oldMembers = new();
    private static bool hasBaseline;

    public static void Start()
    {
        Service.Framework.Update += Update;
    }

    public static void Stop()
    {
        Service.Framework.Update -= Update;
        ResetState();
    }

    private static void ResetState()
    {
        members.Clear();
        oldMembers.Clear();
        hasBaseline = false;
        PfRoleTracker.Clear();
        PfRecruitmentSnapshot.Clear();
    }

    private static unsafe void Update(IFramework framework)
    {
        if (!Service.ClientState.IsLoggedIn)
        {
            ResetState();
            return;
        }

        if (!InfoProxyCrossRealm.IsCrossRealmParty())
        {
            ResetState();
            return;
        }

        members.Clear();
        var partyCount = InfoProxyCrossRealm.GetPartyMemberCount();
        for (var i = 0u; i < partyCount; i++)
        {
            var addr = InfoProxyCrossRealm.GetGroupMember(i);
            var mObj = new CrossWorldMember
            {
                Name = addr->NameString,
                PartyCount = partyCount,
                Level = addr->Level,
                JobId = addr->ClassJobId,
                ContentId = addr->ContentId,
                MemberIndex = addr->MemberIndex,
            };
            members.Add(mObj);
        }

        if (!hasBaseline)
        {
            PfRecruitmentSnapshot.Capture();

            foreach (var member in members)
            {
                var slotRole = PfRoleResolver.ResolveSlotRole(member.ContentId, member.MemberIndex);
                PfRoleTracker.RecordJoin(member.ContentId, member.Name, member.JobId, slotRole);
                RecordPfMemberSlot(member.ContentId, member.MemberIndex);
            }

            oldMembers = members.ToList();
            hasBaseline = true;
            return;
        }

        foreach (var member in PartyMemberChangeDetector.DetectJoins(members, oldMembers))
        {
            var slotRole = PfRoleResolver.ResolveSlotRole(member.ContentId, member.MemberIndex);
            PfRoleTracker.RecordJoin(member.ContentId, member.Name, member.JobId, slotRole);
            RecordPfMemberSlot(member.ContentId, member.MemberIndex);
            OnJoin?.Invoke(member);
        }

        foreach (var member in PartyMemberChangeDetector.DetectLeaves(members, oldMembers))
        {
            OnLeave?.Invoke(member);
            PfRoleTracker.Remove(member.ContentId, member.Name);
        }

        oldMembers = members.ToList();
    }

    private static void RecordPfMemberSlot(ulong contentId, byte memberIndex)
    {
        var slotIndex = PfRoleResolver.FindSlotIndex(contentId, memberIndex);
        if (slotIndex >= 0)
            PfRecruitmentSnapshot.RecordMember(slotIndex, contentId);
    }
}
