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
    }

    private static bool ListContainsMember(List<CrossWorldMember> l, CrossWorldMember m)
        => l.Any(a => a.Name == m.Name);

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
            };
            members.Add(mObj);
        }

        if (!hasBaseline)
        {
            oldMembers = members.ToList();
            hasBaseline = true;
            return;
        }

        foreach (var i in members)
        {
            if (!ListContainsMember(oldMembers, i))
                OnJoin?.Invoke(i);
        }

        foreach (var i in oldMembers)
        {
            if (!ListContainsMember(members, i))
                OnLeave?.Invoke(i);
        }

        oldMembers = members.ToList();
    }
}
