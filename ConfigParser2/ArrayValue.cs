using System.Collections.Immutable;

namespace ConfigParser2;

public record ArrayValue(ImmutableArray<ConfigValue?> Values) : ConfigValue;