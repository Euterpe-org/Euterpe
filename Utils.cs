using System;
using Avalonia.Media;

#pragma warning disable CS8603
#pragma warning disable CS8600

namespace MuseDashModToolsUI;

internal static class Utils
{
    internal static IBrush ToBrush(this string hexColorString) => (IBrush)new BrushConverter().ConvertFromString(hexColorString);

    internal static bool IsValidUrl(this string source) => Uri.TryCreate(source, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    internal static bool IsValidPath(this string source) => Uri.TryCreate(source, UriKind.Absolute, out var uriResult) && uriResult.Scheme == Uri.UriSchemeFile;
}