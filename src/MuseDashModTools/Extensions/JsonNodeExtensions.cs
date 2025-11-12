using System.Text.Json.Nodes;

namespace MuseDashModTools.Extensions;

public static class JsonNodeExtensions
{
    extension(JsonNode node)
    {
        public string? GetString(string key, string? defaultValue = null) =>
            node[key]?.ToString() ?? defaultValue;

        public T? GetValue<T>(string key, Func<string, T?> converter, T? defaultValue = default)
        {
            var value = node[key]?.ToString();
            return value is not null ? converter(value) : defaultValue;
        }
    }
}