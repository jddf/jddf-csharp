using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jddf.Jddf
{
  public class Discriminator
  {
    [JsonProperty("tag")]
    public string Tag { get; set; }

    [JsonProperty("mapping")]
    public IDictionary<string, Schema> Mapping { get; set; }

    public override bool Equals(object obj)
    {
      return obj is Discriminator discriminator &&
             Tag == discriminator.Tag &&
             EqualityComparer<IDictionary<string, Schema>>.Default.Equals(Mapping, discriminator.Mapping);
    }

    public override int GetHashCode()
    {
      var hashCode = -393007024;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Tag);
      hashCode = hashCode * -1521134295 + EqualityComparer<IDictionary<string, Schema>>.Default.GetHashCode(Mapping);
      return hashCode;
    }
  }
}
