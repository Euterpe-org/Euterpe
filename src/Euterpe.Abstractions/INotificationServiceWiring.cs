using Ursa.Controls;

namespace Euterpe.Abstractions;

public interface INotificationServiceWiring
{
    WindowNotificationManager Notifier { set; }
}