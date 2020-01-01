using System.Collections.Generic;
using System.Text.Json;

namespace Jddf.Jddf
{
    public class Validator
    {
        public uint MaxDepth { get; set; }
        public uint MaxErrors { get; set; }

        public IList<ValidationError> Validate(Schema schema, JsonElement instance)
        {
            VM vm = new VM();
            vm.Root = schema;
            vm.MaxDepth = MaxDepth;
            vm.MaxErrors = MaxErrors;
            vm.InstanceTokens = new List<string>();
            vm.SchemaTokens = new List<List<string>>() { new List<string>() };
            vm.Errors = new List<ValidationError>();

            try
            {
                vm.Validate(schema, instance);
            }
            catch (VM.MaxErrorsException)
            {
                // Intentionally left blank. MaxErrorsException is just a
                // circuit-breaker, not an actual error condition.
            }

            return vm.Errors;
        }

        private class VM
        {
            private static readonly string[] RFC_3339_FORMATS = new string[] {
                "yyyy-MM-ddTHH:mm:ssK",
                "yyyy-MM-ddTHH:mm:ss.ffK",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss.ffZ"
            };

            public Schema Root { get; set; }
            public uint MaxDepth { get; set; }
            public uint MaxErrors { get; set; }
            public List<string> InstanceTokens { get; set; }
            public List<List<string>> SchemaTokens { get; set; }
            public List<ValidationError> Errors { get; set; }

            public void Validate(Schema schema, JsonElement instance, string parentTag = null)
            {
                switch (schema.Form)
                {
                    case Form.Ref:
                        if (SchemaTokens.Count == MaxDepth)
                        {
                            throw new MaxDepthExceededException();
                        }

                        SchemaTokens.Add(new List<string>() { "definitions", schema.Ref });
                        Validate(Root.Definitions[schema.Ref], instance);
                        SchemaTokens.RemoveAt(SchemaTokens.Count - 1);
                        break;
                    case Form.Type:
                        pushSchemaToken("type");
                        switch (schema.Type)
                        {
                            case Type.Boolean:
                                if (instance.ValueKind != JsonValueKind.True && instance.ValueKind != JsonValueKind.False)
                                {
                                    pushError();
                                }
                                break;
                            case Type.Float32:
                            case Type.Float64:
                                if (instance.ValueKind != JsonValueKind.Number)
                                {
                                    pushError();
                                }
                                break;
                            case Type.Int8:
                                validateInt(-128, 127, instance);
                                break;
                            case Type.Uint8:
                                validateInt(0, 255, instance);
                                break;
                            case Type.Int16:
                                validateInt(-32768, 32767, instance);
                                break;
                            case Type.Uint16:
                                validateInt(0, 65535, instance);
                                break;
                            case Type.Int32:
                                validateInt(-2147483648, 2147483647, instance);
                                break;
                            case Type.Uint32:
                                validateInt(0, 4294967295L, instance);
                                break;
                            case Type.String:
                                if (instance.ValueKind != JsonValueKind.String)
                                {
                                    pushError();
                                }
                                break;
                            case Type.Timestamp:
                                if (instance.ValueKind == JsonValueKind.String)
                                {
                                    try
                                    {
                                        System.DateTimeOffset.ParseExact(instance.GetString(), RFC_3339_FORMATS, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal);
                                    } catch (System.FormatException)
                                    {
                                        pushError();
                                    }
                                }
                                else
                                {
                                    pushError();
                                }
                                break;
                        }

                        popSchemaToken();
                        break;
                    case Form.Enum:
                        pushSchemaToken("enum");

                        if (instance.ValueKind == JsonValueKind.String)
                        {
                            if (!schema.Enum.Contains(instance.GetString()))
                            {
                                pushError();
                            }
                        }
                        else
                        {
                            pushError();
                        }

                        popSchemaToken();
                        break;
                    case Form.Elements:
                        pushSchemaToken("elements");
                        if (instance.ValueKind == JsonValueKind.Array)
                        {
                            for (int i = 0; i < instance.GetArrayLength(); i++)
                            {
                                pushInstanceToken(i.ToString());
                                Validate(schema.Elements, instance[i]);
                                popInstanceToken();
                            }
                        }
                        else
                        {
                            pushError();
                        }
                        popSchemaToken();
                        break;
                    case Form.Properties:
                        if (instance.ValueKind == JsonValueKind.Object)
                        {
                            if (schema.Properties != null)
                            {
                                pushSchemaToken("properties");
                                foreach (KeyValuePair<string, Schema> entry in schema.Properties) {
                                    pushSchemaToken(entry.Key);
                                    if (instance.TryGetProperty(entry.Key, out JsonElement value)) {
                                        pushInstanceToken(entry.Key);
                                        Validate(entry.Value, value);
                                        popInstanceToken();
                                    }
                                    else
                                    {
                                        pushError();
                                    }
                                    popSchemaToken();
                                }
                                popSchemaToken();
                            }

                            if (schema.OptionalProperties != null)
                            {
                                pushSchemaToken("optionalProperties");
                                foreach (KeyValuePair<string, Schema> entry in schema.OptionalProperties) {
                                    pushSchemaToken(entry.Key);
                                    if (instance.TryGetProperty(entry.Key, out JsonElement value)) {
                                        pushInstanceToken(entry.Key);
                                        Validate(entry.Value, value);
                                        popInstanceToken();
                                    }
                                    popSchemaToken();
                                }
                                popSchemaToken();
                            }

                            if (schema.AdditionalProperties != true)
                            {
                                foreach (JsonProperty property in instance.EnumerateObject())
                                {
                                    bool inProperties = schema.Properties != null && schema.Properties.ContainsKey(property.Name);
                                    bool inOptionalProperties = schema.OptionalProperties != null && schema.OptionalProperties.ContainsKey(property.Name);
                                    bool discriminatorTagException = property.Name.Equals(parentTag);
                                    if (!inProperties && !inOptionalProperties && !discriminatorTagException)
                                    {
                                        pushInstanceToken(property.Name);
                                        pushError();
                                        popInstanceToken();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (schema.Properties != null)
                            {
                                pushSchemaToken("properties");
                            }
                            else
                            {
                                pushSchemaToken("optionalProperties");
                            }

                            pushError();
                            popSchemaToken();
                        }

                        break;
                    case Form.Values:
                        pushSchemaToken("values");
                        if (instance.ValueKind == JsonValueKind.Object)
                        {
                            foreach (JsonProperty property in instance.EnumerateObject())
                            {
                                pushInstanceToken(property.Name);
                                Validate(schema.Values, property.Value);
                                popInstanceToken();
                            }
                        }
                        else
                        {
                            pushError();
                        }
                        popSchemaToken();
                        break;
                    case Form.Discriminator:
                        pushSchemaToken("discriminator");
                        if (instance.ValueKind == JsonValueKind.Object)
                        {
                            if (instance.TryGetProperty(schema.Discriminator.Tag, out JsonElement tag))
                            {
                                if (tag.ValueKind == JsonValueKind.String)
                                {
                                    if (schema.Discriminator.Mapping.ContainsKey(tag.GetString()))
                                    {
                                        pushSchemaToken("mapping");
                                        pushSchemaToken(tag.GetString());
                                        Validate(schema.Discriminator.Mapping[tag.GetString()], instance, schema.Discriminator.Tag);
                                        popSchemaToken();
                                        popSchemaToken();
                                    }
                                    else
                                    {
                                        pushSchemaToken("mapping");
                                        pushInstanceToken(schema.Discriminator.Tag);
                                        pushError();
                                        popInstanceToken();
                                        popSchemaToken();
                                    }
                                }
                                else
                                {
                                    pushSchemaToken("tag");
                                    pushInstanceToken(schema.Discriminator.Tag);
                                    pushError();
                                    popInstanceToken();
                                    popSchemaToken();
                                }
                            }
                            else
                            {
                                pushSchemaToken("tag");
                                pushError();
                                popSchemaToken();
                            }
                        }
                        else
                        {
                            pushError();
                        }
                        popSchemaToken();
                        break;
                }
            }

            private void validateInt(long min, long max, JsonElement instance)
            {
                if (instance.ValueKind == JsonValueKind.Number)
                {
                    double val = instance.GetDouble();
                    if (val < min || val > max || val != System.Math.Round(val)) {
                        pushError();
                    }
                }
                else
                {
                    pushError();
                }
            }

            private void pushInstanceToken(string token)
            {
                InstanceTokens.Add(token);
            }

            private void popInstanceToken()
            {
                InstanceTokens.RemoveAt(InstanceTokens.Count - 1);
            }

            private void pushSchemaToken(string token)
            {
                SchemaTokens[SchemaTokens.Count - 1].Add(token);
            }

            private void popSchemaToken()
            {
                SchemaTokens[SchemaTokens.Count - 1].RemoveAt(SchemaTokens[SchemaTokens.Count - 1].Count - 1);
            }

            private void pushError()
            {
                Errors.Add(new ValidationError(new List<string>(InstanceTokens), new List<string>(SchemaTokens[SchemaTokens.Count - 1])));

                if (Errors.Count == MaxErrors)
                {
                    throw new MaxErrorsException();
                }
            }

            public class MaxErrorsException : System.Exception
            {
                public MaxErrorsException()
                {
                }
            }
        }
    }
}
