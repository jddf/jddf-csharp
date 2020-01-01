using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        public bool? AdditionalProperties { get; set; }
        public Schema Values { get; set; }
        public Discriminator Discriminator { get; set; }

        public Schema Verify()
        {
            return this.verify(this);
        }

        [JsonIgnore]
        public Form Form
        {
            get
            {
                if (Ref != null)
                {
                    return Form.Ref;
                }
                else if (Type != null)
                {
                    return Form.Type;
                }
                else if (Enum != null)
                {
                    return Form.Enum;
                }
                else if (Elements != null)
                {
                    return Form.Elements;
                }
                else if (Properties != null || OptionalProperties != null)
                {
                    return Form.Properties;
                }
                else if (Values != null)
                {
                    return Form.Values;
                }
                else if (Discriminator != null)
                {
                    return Form.Discriminator;
                }
                else
                {
                    return Form.Empty;
                }
            }
        }

        private Schema verify(Schema root)
        {
            bool isEmpty = true;

            if (Definitions != null)
            {
                if (this != root)
                {
                    throw new System.InvalidOperationException("non-root definition");
                }

                foreach (KeyValuePair<string, Schema> entry in Definitions)
                {
                    entry.Value.verify(root);
                }
            }

            if (Ref != null)
            {
                isEmpty = false;

                if (root.Definitions == null)
                {
                    throw new System.InvalidOperationException("ref but no definitions");
                }

                if (!root.Definitions.ContainsKey(Ref))
                {
                    throw new System.InvalidOperationException("ref to non-existent definition");
                }
            }

            if (Type != null)
            {
                if (!isEmpty)
                {
                    throw new System.InvalidOperationException("invalid form");
                }

                isEmpty = false;
            }

            if (Enum != null)
            {
                if (!isEmpty)
                {
                    throw new System.InvalidOperationException("invalid form");
                }

                isEmpty = false;

                if (Enum.Count == 0)
                {
                    throw new System.InvalidOperationException("enum empty array");
                }
            }

            if (Elements != null)
            {
                if (!isEmpty)
                {
                    throw new System.InvalidOperationException("invalid form");
                }

                isEmpty = false;

                Elements.verify(root);
            }

            if (Properties != null || OptionalProperties != null)
            {
                if (!isEmpty)
                {
                    throw new System.InvalidOperationException("invalid form");
                }

                isEmpty = false;

                if (Properties != null && OptionalProperties != null)
                {
                    foreach (KeyValuePair<string, Schema> entry in Properties)
                    {
                        if (OptionalProperties.ContainsKey(entry.Key))
                        {
                            throw new System.InvalidOperationException("properties and optionalProperties share key");
                        }
                    }
                }
            }


            if (Values != null)
            {
                if (!isEmpty)
                {
                    throw new System.InvalidOperationException("invalid form");
                }

                isEmpty = false;

                Values.verify(root);
            }

            if (Discriminator != null)
            {
                if (!isEmpty)
                {
                    throw new System.InvalidOperationException("invalid form");
                }

                isEmpty = false;

                if (Discriminator.Tag == null)
                {
                    throw new System.InvalidOperationException("discriminator has no tag");
                }

                if (Discriminator.Mapping == null)
                {
                    throw new System.InvalidOperationException("discriminator has no mapping");
                }

                foreach (KeyValuePair<string, Schema> entry in Discriminator.Mapping)
                {
                    entry.Value.verify(root);

                    if (entry.Value.Form != Form.Properties)
                    {
                        throw new System.InvalidOperationException("discriminator mapping value is not of properties form");
                    }

                    if (entry.Value.Properties != null && entry.Value.Properties.ContainsKey(Discriminator.Tag))
                    {
                        throw new System.InvalidOperationException("discriminator mapping value has a property equal to tag's value");
                    }

                    if (entry.Value.OptionalProperties != null && entry.Value.OptionalProperties.ContainsKey(Discriminator.Tag))
                    {
                        throw new System.InvalidOperationException("discriminator mapping value has an optional property equal to tag's value");
                    }
                }
            }

            return this;
        }
    }
}
