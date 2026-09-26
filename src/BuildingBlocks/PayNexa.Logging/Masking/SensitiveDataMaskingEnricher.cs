using Serilog.Core;
using Serilog.Events;

namespace PayNexa.Logging.Masking;

public sealed class SensitiveDataMaskingEnricher : ILogEventEnricher
{
    public const string Redacted = "***REDACTED***";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var (name, value) in logEvent.Properties.ToArray())
        {
            var masked = Mask(name, value);

            if (!ReferenceEquals(masked, value))
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty(name, masked));
            }
        }
    }

    internal static LogEventPropertyValue Mask(string name, LogEventPropertyValue value)
    {
        var kind = SensitiveFields.Classify(name);

        if (kind == SensitiveFieldKind.Secret)
        {
            return new ScalarValue(Redacted);
        }

        return value switch
        {
            ScalarValue { Value: string text } when kind != SensitiveFieldKind.None => new ScalarValue(SensitiveFields.Mask(kind, text)),
            StructureValue structure => MaskStructure(structure),
            SequenceValue sequence => MaskSequence(name, sequence),
            DictionaryValue dictionary => MaskDictionary(dictionary),
            _ => value,
        };
    }

    private static LogEventPropertyValue MaskStructure(StructureValue structure)
    {
        var changed = false;
        var properties = new List<LogEventProperty>(structure.Properties.Count);

        foreach (var property in structure.Properties)
        {
            var masked = Mask(property.Name, property.Value);
            changed |= !ReferenceEquals(masked, property.Value);
            properties.Add(ReferenceEquals(masked, property.Value) ? property : new LogEventProperty(property.Name, masked));
        }

        return changed ? new StructureValue(properties, structure.TypeTag) : structure;
    }

    private static LogEventPropertyValue MaskSequence(string name, SequenceValue sequence)
    {
        var elements = sequence.Elements.Select(element => Mask(name, element)).ToArray();
        return elements.SequenceEqual(sequence.Elements, ReferenceEqualityComparer.Instance)
            ? sequence
            : new SequenceValue(elements);
    }

    private static LogEventPropertyValue MaskDictionary(DictionaryValue dictionary)
    {
        var changed = false;
        var entries = new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>(dictionary.Elements.Count);

        foreach (var (key, value) in dictionary.Elements)
        {
            var masked = Mask(key.Value?.ToString() ?? string.Empty, value);
            changed |= !ReferenceEquals(masked, value);
            entries.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>(key, masked));
        }

        return changed ? new DictionaryValue(entries) : dictionary;
    }

}
