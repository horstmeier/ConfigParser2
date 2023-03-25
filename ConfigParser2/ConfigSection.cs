using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using ConfigParser2.ValueTypes;

namespace ConfigParser2;

public partial class ConfigSection
{
    private readonly ImmutableStack<ObjectValue> _values;

    public ConfigSection(ImmutableStack<ObjectValue> values)
    {
        _values = values;
    }

    public ConfigSection(ObjectValue value)
        : this(ImmutableStack.Create(value))
    {
    }

    public ConfigSection(string content) : this(ConfigParser.Create(content))
    {
    }
    
    public ConfigSection(string content, ImmutableDictionary<string, string> defaults)
    {
        var values = ConfigParser.Create(content);
        var objValues = defaults
            .Select(it => new KeyValuePair<string,
                ConfigValue>(it.Key, new StringValue(it.Value)))
            .ToImmutableDictionary();
        var d = values.Values.Aggregate(objValues,
            (current, kvp) =>
                current.SetItem(kvp.Key, kvp.Value));

        _values = ImmutableStack.Create(new ObjectValue(d));
    }
    
    public ConfigSection(FileInfo file) : this(ConfigParser.Create(file))
    {
    }

    public ConfigValue? Get(string key, bool recurse = false)
    {
        if (!_values.Peek().Values.TryGetValue(key, out var value))
        {
            var prev = _values.Pop();
            return recurse && prev.Any() 
                ? new ConfigSection(prev).Get(key, recurse) // Todo: Remove the need for the new
                : null;
        }
        return Translate(value);
    }

    // The method GetString returns the value of the key as a string.
    // If the key does not exist, the method returns null.
    public string GetString(params string[] key)
    {
        return key.Length switch
        {
            0 => string.Empty,
            1 => ValueToString(Get(key[0])),
            _ => GetSection(key[0])?.GetString(key[1..]) ?? string.Empty
        };
    }
    
    public ConfigSection? GetSection(string key)
    {
        if (_values.Peek().Values.TryGetValue(key, out var value) && value is ObjectValue obj)
        {
            return new ConfigSection(_values.Push(obj));
        }

        return null;
    }

    private ConfigValue? Translate(ConfigValue? value)
    {
        if (value is null)
        {
            return null;
        }
        
        if (value is StringValue str)
        {
            var s = TranslateString(str.Value);
            return new StringValue(s);
        }

        if (value is ArrayValue arr)
        {
            return new ArrayValue(arr.Values.Select(Translate).ToImmutableArray());
        }

        if (value is ObjectValue)
        {
            return null;
        }

        return value;
    }
    
    private string TranslateString(string value)
    {
        var regex = KeyRegex();

        return regex.Replace(value, m =>
        {
            var key = m.Groups["key"].Value;
            if (key.StartsWith("env:", StringComparison.InvariantCultureIgnoreCase))
            {
                var subString = key.Substring(4);
                var splits = subString.Split(":");
                if (splits.Length >= 2)
                {
                    var defaultValue = splits[^1];
                    var envKey = subString.Substring(0, subString.Length - defaultValue.Length - 1);
                    return Environment.GetEnvironmentVariable(envKey) ?? defaultValue;
                }
                return Environment.GetEnvironmentVariable(key.Substring(4)) ?? string.Empty;
            }
            //if (key.StartsWith("cmd:"))
            //{
            //    return RunCommand(key.Substring(4)) ?? string.Empty;
            //}
            if (key.StartsWith("file:", StringComparison.InvariantCultureIgnoreCase))
            {
                return File.ReadAllText(key.Substring(5));
            }
            //if (key.StartsWith("json:"))
            //{
            //    return new ConfigReader(key.Substring(5)).Get("value")?.ToString() ?? string.Empty;
            //}
            if (key.StartsWith("base64:", StringComparison.InvariantCultureIgnoreCase))
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(key.Substring(7)));
            }
            if (key.Equals("now", StringComparison.InvariantCultureIgnoreCase))
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (key.Equals("utcnow", StringComparison.InvariantCultureIgnoreCase))
            {
                return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            }
            
            if (key.EndsWith('/'))
            {
                var x = Get(key.Substring(0, key.Length - 1), true);
                if (x is StringValue s)
                {
                    var path = s.Value;
                    if (path.EndsWith(Path.DirectorySeparatorChar) 
                        || path.EndsWith(Path.AltDirectorySeparatorChar))
                    {
                        path = path.Substring(0, path.Length - 1);
                    }
                    return path + Path.DirectorySeparatorChar;
                }
                else
                {
                    throw new Exception($"Key {key} not found or is not a string");
                }
            }
            
            var v = Get(key, true);
            if (v is null)
            {
                throw new Exception($"Key {key} not found");
            }

            return ValueToString(v);
        });
    }

    private static string ValueToString(ConfigValue? v)
    {
        return v switch
        {
            StringValue s => s.Value,
            LongValue l => l.Value.ToString(),
            TrueValue => "true",
            FalseValue => "false",
            NullValue => null,
            _ => string.Empty
        } ?? string.Empty;
    }

    [GeneratedRegex("\\$\\{(?<key>[^}]+)\\}")]
    private static partial Regex KeyRegex();
}