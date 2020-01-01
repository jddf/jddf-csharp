using System.Collections.Generic;
using Xunit;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jddf.Jddf.Test
{
    public class SchemaTest
    {
        [Fact]
        public void SerializeDeserialize()
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

            var serializeOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
            serializeOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            string serialized = JsonSerializer.Serialize(schema, serializeOptions);
            Assert.Equal(
                @"{""definitions"":{""foo"":{}},""ref"":""foo"",""type"":""uint8"",""enum"":[""""],""elements"":{},""properties"":{""foo"":{}},""optionalProperties"":{""foo"":{}},""additionalProperties"":true,""values"":{},""discriminator"":{""tag"":""foo"",""mapping"":{""foo"":{}}}}",
                serialized
            );

            // Comparing instances of Schema does not work, due to details of
            // which instance of the various collections are instantiated.
            // Instead, for this test we will satisfy ourselves with having the
            // serialized representations be the same.
            Schema deserialized = JsonSerializer.Deserialize<Schema>(serialized, serializeOptions);
            string reserialized = JsonSerializer.Serialize(deserialized, serializeOptions);

            Assert.Equal(serialized, reserialized);
        }

        [Fact]
        public void Verify()
        {
            var serializeOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            List<InvalidSchemaTestCase> testCases = JsonSerializer.Deserialize<List<InvalidSchemaTestCase>>(System.IO.File.ReadAllText("../../../../../spec/tests/invalid-schemas.json"), serializeOptions);
            foreach (InvalidSchemaTestCase testCase in testCases) {
                // This test case is not handled, as System.Text.Json does not
                // complain if a set is populated from JSON with repeated
                // values.
                if (testCase.Name.Equals("enum contains repated values")) {
                    continue;
                }

                bool ok = false;
                try {
                    Schema schema = JsonSerializer.Deserialize<Schema>(testCase.Schema.GetRawText(), serializeOptions);
                    if (schema == null) {
                        ok = true;
                    } else {
                        schema.Verify();
                    }
                } catch (JsonException) {
                    ok = true;
                } catch (InvalidOperationException) {
                    ok = true;
                }

                Assert.True(ok, testCase.Name);

            }
        }

        private class InvalidSchemaTestCase
        {
            public string Name { get; set; }
            public JsonElement Schema { get; set; }
        }
    }
}
