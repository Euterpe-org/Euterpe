using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MuseDashModTools.Models.Messages;

public sealed class SelectedPageChangedMessage(string value) : ValueChangedMessage<string>(value)
{
    public static implicit operator SelectedPageChangedMessage(string value) => new(value);
}