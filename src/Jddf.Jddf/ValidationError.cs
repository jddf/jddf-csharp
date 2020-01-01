using System;
using System.Collections.Generic;
using System.Linq;

namespace Jddf.Jddf
{
    public class ValidationError : IEquatable<ValidationError>
    {
        public IList<string> InstancePath { get; set; }
        public IList<string> SchemaPath { get; set; }

        public ValidationError(IList<string> instancePath, IList<string> schemaPath)
        {
            InstancePath = instancePath;
            SchemaPath = schemaPath;
        }

        public bool Equals(ValidationError other)
        {
            return other != null && InstancePath.SequenceEqual(other.InstancePath) && SchemaPath.SequenceEqual(other.SchemaPath);
        }

        public override int GetHashCode()
        {
            var hashCode = -435878218;
            hashCode = hashCode * -1521134295 + EqualityComparer<IList<string>>.Default.GetHashCode(InstancePath);
            hashCode = hashCode * -1521134295 + EqualityComparer<IList<string>>.Default.GetHashCode(SchemaPath);
            return hashCode;
        }
    }
}
