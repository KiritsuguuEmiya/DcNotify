using System;
using System.Threading;
using System.Threading.Tasks;
using Dnc.Delivery;
using Dnc.Util;

namespace Dnc.Notifications;

public sealed class AfkPartyMessageHandler
{
    public static AfkPartyMessageHandler Default { get; private set; } = null!;

    private readonly IPartyChatSender sender;
    private readonly Func<TimeSpan, CancellationToken, Task> delayAsync;
    private readonly Func<bool> isClientAfkProvider;

    private CancellationTokenSource? pendingSendCts;
    private bool armedForCurrentFill;

    public AfkPartyMessageHandler(
        IPartyChatSender sender,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null,
        Func<bool>? isClientAfkProvider = null)
    {
        this.sender = sender;
        this.delayAsync = delayAsync ?? ((duration, token) => Task.Delay(duration, token));
        this.isClientAfkProvider = isClientAfkProvider ?? CharacterUtil.IsClientAfk;
    }

    public static void Initialize(IPartyChatSender sender)
        => Default = new AfkPartyMessageHandler(sender);

    public void HandleJoin(
        CrossWorldPartyListSystem.CrossWorldMember member,
        Configuration config,
        bool isClientAfk)
    {
        if (!AfkPartyMessagePolicy.ShouldTrigger(config, isClientAfk))
            return;

        if (!AfkPartyMessagePolicy.IsPartyFull(member))
            return;

        if (armedForCurrentFill)
            return;

        armedForCurrentFill = true;
        ScheduleSend(config);
    }

    public void HandleLeave(CrossWorldPartyListSystem.CrossWorldMember member)
    {
        if (!AfkPartyMessagePolicy.IsPartyNoLongerFullAfterLeave(member))
            return;

        ResetFillCycle();
    }

    public void OnFrameworkUpdate(bool isClientAfk)
    {
        if (isClientAfk || pendingSendCts == null)
            return;

        CancelPendingSend();
    }

    public PartyChatSendResult SendTestMessage(Configuration config)
    {
        if (!config.Enabled)
            return PartyChatSendResult.Fail("Enable the plugin first.");

        return sender.TrySendPartyMessage(AfkPartyMessageFormatter.Format(config));
    }

    public void Dispose()
        => ResetFillCycle();

    private void ScheduleSend(Configuration config)
    {
        CancelPendingSend();

        var delaySeconds = Math.Clamp(config.AfkPartyMessageDelaySeconds, 5, 60);
        var message = AfkPartyMessageFormatter.Format(config);
        pendingSendCts = new CancellationTokenSource();
        var token = pendingSendCts.Token;

        _ = delayAsync(TimeSpan.FromSeconds(delaySeconds), token).ContinueWith(
            task =>
            {
                if (task.IsCanceled)
                    return;

                if (!armedForCurrentFill)
                    return;

                if (!AfkPartyMessagePolicy.ShouldTrigger(config, isClientAfkProvider()))
                    return;

                sender.TrySendPartyMessage(message);
            },
            token,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private void ResetFillCycle()
    {
        armedForCurrentFill = false;
        CancelPendingSend();
    }

    private void CancelPendingSend()
    {
        pendingSendCts?.Cancel();
        pendingSendCts?.Dispose();
        pendingSendCts = null;
    }
}
