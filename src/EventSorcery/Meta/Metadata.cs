using System.Text.Json.Serialization;

namespace EventSorcery.Meta;

public interface IMetadata
{
    string CorrelationId { get; }
    string CausationId { get; }
    
    IMetadata CloneWith(params KeyValuePair<string, string>[] keyValuePairs);
    IMetadata CloneWith(IEnumerable<KeyValuePair<string, string>> keyValuePairs);
}

public class Metadata : MetadataContainer, IMetadata
{
    public static IMetadata Empty { get; } = new Metadata();
    
    public static IMetadata With(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
    {
        return new Metadata(keyValuePairs);
    }

    public static IMetadata With(params KeyValuePair<string, string>[] keyValuePairs)
    {
        return new Metadata(keyValuePairs);
    }

    public static IMetadata With(IDictionary<string, string> keyValuePairs)
    {
        return new Metadata(keyValuePairs);
    }
    
    [JsonIgnore]
    public string CorrelationId
    {
        get { return GetMetadataValue(MetadataKeys.CorrelationId); }
        set { Add(MetadataKeys.CorrelationId, value); }
    }
        
    [JsonIgnore]
    public string CausationId
    {
        get { return GetMetadataValue(MetadataKeys.CausationId); }
        set { Add(MetadataKeys.CausationId, value); }
    }

    public Metadata()
    {
        // Empty
    }

    public Metadata(IDictionary<string, string> keyValuePairs)
        : base(keyValuePairs)
    {
    }

    public Metadata(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value))
    {
    }

    public Metadata(params KeyValuePair<string, string>[] keyValuePairs)
        : this((IEnumerable<KeyValuePair<string, string>>)keyValuePairs)
    {
    }

    public IMetadata CloneWith(params KeyValuePair<string, string>[] keyValuePairs)
    {
        return CloneWith((IEnumerable<KeyValuePair<string, string>>)keyValuePairs);
    }

    public IMetadata CloneWith(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
    {
        var metadata = new Metadata(this);
        foreach (var kv in keyValuePairs)
        {
            if (metadata.ContainsKey(kv.Key))
            {
                throw new ArgumentException($"Key '{kv.Key}' is already present!");
            }
            metadata[kv.Key] = kv.Value;
        }
        return metadata;
    }
    
    public Metadata With<T>(string key, string? value)
    {
        if (value is not null)
            this[key] = value;
        
        return this;
    }
    
    public string? GetString(string key) => TryGetValue(key, out var value) 
        ? value?.ToString() 
        : default;

    public Metadata AddNotNull(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            Add(key, value);
        
        return this;
    }
}