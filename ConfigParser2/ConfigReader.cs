using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using ConfigParser2.ValueTypes;

namespace ConfigParser2;

public class ConfigReader
{
    private readonly ImmutableStack<ObjectValue> _values;

    public ConfigReader(ImmutableStack<ObjectValue> values)
    {
        _values = values;
    }

    public ConfigReader(ObjectValue value)
        : this(ImmutableStack.Create(value))
    {
    }

    public ConfigReader(string content) : this(ConfigParser.Create(content))
    {
    }

    public ConfigReader(FileInfo file) : this(ConfigParser.Create(file))
    {
    }

    public ConfigValue? Get(string key, bool recurse = false)
    {
        if (!_values.Peek().Values.TryGetValue(key, out var value))
        {
            var prev = _values.Pop();
            return recurse && prev.Any() 
                ? new ConfigReader(prev).Get(key, recurse) // Todo: Remove the need for the new
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
            1 => ValueToString(Get(key[0])) ?? string.Empty,
            _ => GetSection(key[0])?.GetString(key[1..]) ?? string.Empty
        };
    }
    
    public ConfigReader? GetSection(string key)
    {
        if (_values.Peek().Values.TryGetValue(key, out var value) && value is ObjectValue obj)
        {
            return new ConfigReader(_values.Push(obj));
        }

        return null;
    }

    private ConfigValue Translate(ConfigValue? value)
    {
        if (value is null)
        {
            return new NullValue();
        }
        
        if (value is StringValue str)
        {
            var s = TranslateString(str.Value);
            return s != null
                ? new StringValue(s)
                : new NullValue();
        }

        if (value is ArrayValue arr)
        {
            return new ArrayValue(arr.Values.Select(Translate).ToImmutableArray());
        }

        if (value is ObjectValue obj)
        {
            return new NullValue();
        }

        return value;
    }

    private string TranslateString(string value)
    {
        var regex = new Regex(@"\$\{(?<key>[^}]+)\}");

        return regex.Replace(value, m =>
        {
            var key = m.Groups["key"].Value;
            if (key.StartsWith("env:"))
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
            if (key.StartsWith("file:"))
            {
                return File.ReadAllText(key.Substring(5)) ?? string.Empty;
            }
            //if (key.StartsWith("json:"))
            //{
            //    return new ConfigReader(key.Substring(5)).Get("value")?.ToString() ?? string.Empty;
            //}
            if (key.StartsWith("base64:"))
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(key.Substring(7))) ?? string.Empty;
            }
            if (key.Equals("now"))
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (key.Equals("utcnow"))
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
}