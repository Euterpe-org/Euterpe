namespace Euterpe.Mvvm;

public sealed record EnumOption<TEnum>(TEnum Value, LocalizedString Display) where TEnum : struct, Enum;
