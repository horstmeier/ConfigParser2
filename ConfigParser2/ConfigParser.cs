using System.Collections.Immutable;
using System.Text.Json;
using ConfigParser2.ValueTypes;

namespace ConfigParser2;

public static class ConfigParser
{
    public static ObjectValue Create(string content)
    {
        if (File.Exists(content))
        {
            return Create(new FileInfo(content));
        }

        var root = JsonDocument.Parse(content).RootElement;
        return Parse(root);
    }

    public static ObjectValue Create(FileInfo file) => Create(File.ReadAllText(file.FullName));

    private static ObjectValue Parse(JsonElement e)
    {
        if (e.ValueKind != JsonValueKind.Object)
        {
            throw new Exception("Root element must be an object");
        }

        return new ObjectValue(e.EnumerateObject()
            .Aggregate(ImmutableDictionary<string, ConfigValue>.Empty, (current, element) =>
                current.Add(element.Name, ParseElement(element.Value))));
    }

    private static ConfigValue ParseElement(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Array =>
                new ArrayValue(element.EnumerateArray().Select(ParseElement)
                    .ToImmutableArray()),
            JsonValueKind.False => new FalseValue(),
            JsonValueKind.Null => new NullValue(),
            JsonValueKind.Number => new LongValue(element.GetInt64()),
            JsonValueKind.Object => Parse(element),
            JsonValueKind.String => new StringValue(element.GetString() ?? ""),
            JsonValueKind.True => new TrueValue(),
            JsonValueKind.Undefined => throw new Exception("Undefined value kind"),
            _ => throw new Exception("Unknown value kind")
        };
}