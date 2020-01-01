using System.Collections.Generic;

namespace Jddf.Jddf
{
    public class Discriminator
    {
        public string Tag { get; set; }
        public IDictionary<string, Schema> Mapping { get; set; }
    }
}
