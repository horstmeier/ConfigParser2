using System.Collections.Immutable;

namespace ConfigParser2;

public record ObjectValue(ImmutableDictionary<string, ConfigValue> Values) : ConfigValue;