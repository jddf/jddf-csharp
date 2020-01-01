using System.Collections.Generic;

namespace Jddf.Jddf
{
  public class Schema
  {
    public IDictionary<string, Schema> Definitions { get; set; }
    public string Ref { get; set; }
    public Type? Type { get; set; }
    public ISet<string> Enum { get; set; }
    public Schema Elements { get; set; }
    public IDictionary<string, Schema> Properties { get; set; }
    public IDictionary<string, Schema> OptionalProperties { get; set; }
    public Schema Values { get; set; }
    public Discriminator Discriminator { get; set; }
  }
}
