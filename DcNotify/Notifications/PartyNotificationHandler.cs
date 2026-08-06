using System;
using System.Threading.Tasks;
using Dnc.Delivery;
using Dnc.Util;

namespace Dnc.Notifications;

public sealed class PartyNotificationHandler
{
    public static PartyNotificationHandler Default { get; } = new(DiscordNotificationSink.Instance);

    private readonly INotificationSink sink;
    private readonly Func<uint, string>? jobAbbreviationResolver;

    public PartyNotificationHandler(
        INotificationSink sink,
        Func<uint, string>? jobAbbreviationResolver = null)
    {
        this.sink = sink;
        this.jobAbbreviationResolver = jobAbbreviationResolver;
    }

    public void HandleJoin(
        CrossWorldPartyListSystem.CrossWorldMember member,
        Configuration config,
        bool isClientAfk,
        bool includeComposition = true)
    {
        if (!PartyNotificationPolicy.ShouldNotify(config.Enabled, isClientAfk))
            return;

        if (!PartyNotificationPolicy.ShouldNotifyJoin(config, member))
            return;

        var snapshot = PfRoleTracker.Get(member.ContentId, member.Name);
        var jobAbbr = ResolveJobAbbreviation(snapshot?.JoinJobId ?? member.JobId);
        var title = PartyNotificationFormatter.FormatJoinTitle(member, snapshot);
        var description = PartyNotificationFormatter.FormatJoinDescription(member, snapshot, jobAbbr);

        DeliverAsync(title, description, includeComposition);
    }

    public void HandleLeave(
        CrossWorldPartyListSystem.CrossWorldMember member,
        Configuration config,
        bool isClientAfk,
        bool includeComposition = true)
    {
        if (!PartyNotificationPolicy.ShouldNotify(config.Enabled, isClientAfk))
            return;

        if (!PartyNotificationPolicy.ShouldNotifyLeave(config, member))
            return;

        var remaining = Math.Max(0, member.PartyCount - 1);
        var snapshot = PfRoleTracker.Get(member.ContentId, member.Name);
        var jobAbbr = ResolveJobAbbreviation(snapshot?.JoinJobId ?? member.JobId);
        var title = PartyNotificationFormatter.FormatLeaveTitle(member, remaining, snapshot);
        var description = PartyNotificationFormatter.FormatLeaveDescription(member, remaining, snapshot, jobAbbr);

        DeliverAsync(title, description, includeComposition);
    }

    private string ResolveJobAbbreviation(uint jobId)
        => jobAbbreviationResolver?.Invoke(jobId) ?? LuminaDataUtil.GetJobAbbreviation(jobId);

    private void DeliverAsync(string title, string description, bool includeComposition)
    {
        _ = DeliverInternalAsync(title, description, includeComposition);
    }

    private async Task DeliverInternalAsync(string title, string description, bool includeComposition)
    {
        byte[]? composition = null;

        if (includeComposition)
        {
            try
            {
                var slots = PartyCompositionBuilder.Build();
                composition = await PartyCompositionRenderer.RenderAsync(slots);
            }
            catch (Exception ex)
            {
                Service.PluginLog.Warning(ex, "Failed to build party composition for webhook.");
            }
        }

        sink.Deliver(title, description, composition);
    }
}
