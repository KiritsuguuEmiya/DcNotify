using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace Dnc.Util;

public static class PartyUtil
{
    public static unsafe bool IsInParty()
    {
        if (!Service.ClientState.IsLoggedIn)
            return false;

        if (InfoProxyCrossRealm.IsCrossRealmParty())
            return InfoProxyCrossRealm.GetPartyMemberCount() > 0;

        var groupManager = GroupManager.Instance();
        if (groupManager == null)
            return false;

        var group = groupManager->GetGroup();
        return group != null && group->MemberCount > 0;
    }
}
