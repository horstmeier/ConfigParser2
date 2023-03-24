using System.Collections.Immutable;

namespace ConfigParser2.ValueTypes;

public record ArrayValue(ImmutableArray<ConfigValue?> Values) : ConfigValue;