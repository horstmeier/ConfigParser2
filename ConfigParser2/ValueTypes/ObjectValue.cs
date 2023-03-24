using System.Collections.Immutable;

namespace ConfigParser2.ValueTypes;

public record ObjectValue(ImmutableDictionary<string, ConfigValue> Values) : ConfigValue;