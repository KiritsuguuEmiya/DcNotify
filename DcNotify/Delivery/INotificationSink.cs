namespace Dnc.Delivery;

public interface INotificationSink
{
    void Deliver(string title, string text, byte[]? compositionPng = null);
}
