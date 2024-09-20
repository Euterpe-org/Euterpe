using System.Text.Json.Nodes;

namespace MuseDashModTools.Extensions;

public static class JsonNodeExtensions
{
    public static string? GetString(this JsonNode node, string key, string? defaultValue = default) =>
        node[key]?.ToString() ?? defaultValue;

    public static T? GetValue<T>(this JsonNode node, string key, Func<string, T?> converter, T? defaultValue = default)
    {
        var value = node[key]?.ToString();
        return value is not null ? converter(value) : defaultValue;
    }
}