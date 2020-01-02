using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Jddf.Jddf.Test
{
    public class SchemaTestNewtonsoft
    {
        [Fact]
        public void SerializeDeserializeJsonNet()
        {
            Schema schema = new Schema();
            schema.Definitions = new Dictionary<string, Schema>() { { "foo", new Schema() } };
            schema.Ref = "foo";
            schema.Type = Type.Uint8;
            schema.Enum = new HashSet<string>() { "" };
            schema.Elements = new Schema();
            schema.Properties = new Dictionary<string, Schema>() { { "foo", new Schema() } };
            schema.OptionalProperties = new Dictionary<string, Schema>() { { "foo", new Schema() } };
            schema.AdditionalProperties = true;
            schema.Values = new Schema();
            schema.Discriminator = new Discriminator();
            schema.Discriminator.Tag = "foo";
            schema.Discriminator.Mapping = new Dictionary<string, Schema>() { { "foo", new Schema() } };

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            string serialized = JsonConvert.SerializeObject(schema, serializerSettings);

            Assert.Equal(
                @"{""definitions"":{""foo"":{}},""ref"":""foo"",""type"":""uint8"",""enum"":[""""],""elements"":{},""properties"":{""foo"":{}},""optionalProperties"":{""foo"":{}},""additionalProperties"":true,""values"":{},""discriminator"":{""tag"":""foo"",""mapping"":{""foo"":{}}}}",
                serialized
            );

            // Comparing instances of Schema does not work, due to details of
            // which instance of the various collections are instantiated.
            // Instead, for this test we will satisfy ourselves with having the
            // serialized representations be the same.
            Schema deserialized = JsonConvert.DeserializeObject<Schema>(serialized, serializerSettings);
            string reserialized = JsonConvert.SerializeObject(deserialized, serializerSettings);

            Assert.Equal(serialized, reserialized);
        }
    }
}
