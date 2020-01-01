using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jddf.Jddf
{
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
  public class Schema
  {
    [JsonProperty("definitions")]
    public IDictionary<string, Schema> Definitions { get; set; }

    [JsonProperty("ref")]
    public string Ref { get; set; }

    [JsonProperty("type")]
    public Type? Type { get; set; }

    [JsonProperty("enum")]
    public ISet<string> Enum { get; set; }

    [JsonProperty("elements")]
    public Schema Elements { get; set; }

    [JsonProperty("properties")]
    public IDictionary<string, Schema> Properties { get; set; }

    [JsonProperty("optionalProperties")]
    public IDictionary<string, Schema> OptionalProperties { get; set; }

    [JsonProperty("values")]
    public Schema Values { get; set; }

    [JsonProperty("discriminator")]
    public Discriminator Discriminator { get; set; }

    public override bool Equals(object obj)
    {
      return obj is Schema schema &&
             EqualityComparer<IDictionary<string, Schema>>.Default.Equals(Definitions, schema.Definitions) &&
             Ref == schema.Ref &&
             Type == schema.Type &&
             EqualityComparer<ISet<string>>.Default.Equals(Enum, schema.Enum) &&
             EqualityComparer<Schema>.Default.Equals(Elements, schema.Elements) &&
             EqualityComparer<IDictionary<string, Schema>>.Default.Equals(Properties, schema.Properties) &&
             EqualityComparer<IDictionary<string, Schema>>.Default.Equals(OptionalProperties, schema.OptionalProperties) &&
             EqualityComparer<Schema>.Default.Equals(Values, schema.Values);
    }

    public override int GetHashCode()
    {
      var hashCode = 1184893339;
      hashCode = hashCode * -1521134295 + EqualityComparer<IDictionary<string, Schema>>.Default.GetHashCode(Definitions);
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Ref);
      hashCode = hashCode * -1521134295 + Type.GetHashCode();
      hashCode = hashCode * -1521134295 + EqualityComparer<ISet<string>>.Default.GetHashCode(Enum);
      hashCode = hashCode * -1521134295 + EqualityComparer<Schema>.Default.GetHashCode(Elements);
      hashCode = hashCode * -1521134295 + EqualityComparer<IDictionary<string, Schema>>.Default.GetHashCode(Properties);
      hashCode = hashCode * -1521134295 + EqualityComparer<IDictionary<string, Schema>>.Default.GetHashCode(OptionalProperties);
      hashCode = hashCode * -1521134295 + EqualityComparer<Schema>.Default.GetHashCode(Values);
      return hashCode;
    }
  }
}
