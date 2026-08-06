using System.Collections.Generic;
using Dnc.Delivery;

namespace DcNotify.TestSupport;

public sealed class FakeNotificationSink : INotificationSink
{
    public sealed record Delivery(string Title, string Text, byte[]? CompositionPng);

    public List<Delivery> Deliveries { get; } = new();

    public void Deliver(string title, string text, byte[]? compositionPng = null)
        => Deliveries.Add(new Delivery(title, text, compositionPng));

    public void Clear() => Deliveries.Clear();
}
