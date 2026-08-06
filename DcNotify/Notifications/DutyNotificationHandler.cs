using Dalamud.Utility;
using Dnc.Delivery;
using Lumina.Excel.Sheets;

namespace Dnc.Notifications;

public sealed class DutyNotificationHandler
{
    public static DutyNotificationHandler Default { get; } = new(DiscordNotificationSink.Instance);

    private readonly INotificationSink sink;

    public DutyNotificationHandler(INotificationSink sink)
    {
        this.sink = sink;
    }

    public void Handle(ContentFinderCondition duty, Configuration config, bool isClientAfk)
    {
        if (!DutyNotificationPolicy.ShouldNotify(config, isClientAfk))
            return;

        var dutyName = duty.RowId == 0 ? "Duty Roulette" : duty.Name.ToDalamudString().TextValue;
        sink.Deliver("Duty pop", $"Duty registered: '{dutyName}'.");
    }
}
