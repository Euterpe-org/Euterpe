using System.ComponentModel;
using System.Runtime.CompilerServices;
using MuseDashModTools.Localization;
using R3;

namespace MuseDashModTools.Common;

public sealed class LocalizedString : INotifyPropertyChanged
{
    public string Value => XAML.GetResourceString(field);

    private LocalizedString(string resourceKey)
    {
        Value = resourceKey;

        Observable.FromEventHandler(
                h => LocalizationManager.CultureChanged += h,
                h => LocalizationManager.CultureChanged -= h)
            .Subscribe(this, (_, state) => state.OnPropertyChanged(nameof(Value)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override string ToString() => Value;

    public static implicit operator LocalizedString(string resourceKey) => new(resourceKey);

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}