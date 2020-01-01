using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json;
using System;

namespace Jddf.Jddf.Test
{
  public class SchemaTest
  {
    [Fact]
    public void TestingFromJson()
    {
      string json = @"{
                    ""definitions"": { ""foo"": {}},
                    ""ref"": ""foo"",
                    ""type"": ""int8"",
                    ""enum"": [""""],
                    ""elements"": {},
                    ""properties"": { ""foo"": {}},
                    ""optionalProperties"": { ""foo"": {}},
                    ""values"": {},
                    ""discriminator"": { ""tag"": ""foo"", ""mapping"": {""foo"": {}}}
                }";

      Schema schema = JsonConvert.DeserializeObject<Schema>(json);

      Assert.Equal(new Dictionary<string, Schema>() { { "foo", new Schema() } }, schema.Definitions);
      Assert.Equal("foo", schema.Ref);
      Assert.Equal(Type.Int8, schema.Type);
      Assert.Equal(new HashSet<string>() { "" }, schema.Enum);
      Assert.Equal(new Schema(), schema.Elements);
      Assert.Equal(new Dictionary<string, Schema>() { { "foo", new Schema() } }, schema.Properties);
      Assert.Equal(new Dictionary<string, Schema>() { { "foo", new Schema() } }, schema.OptionalProperties);
      Assert.Equal(new Schema(), schema.Values);
      Assert.Equal("foo", schema.Discriminator.Tag);
      Assert.Equal(new Dictionary<string, Schema>() { { "foo", new Schema() } }, schema.Discriminator.Mapping);
    }

    [Fact]
    public void TestingToJson()
    {
      Schema schema = new Schema();
      schema.Definitions = new Dictionary<string, Schema>() { { "foo", new Schema() } };
      schema.Ref = "foo";
      schema.Type = Type.Int8;
      schema.Enum = new HashSet<string>() { "" };
      schema.Elements = new Schema();
      schema.Properties = new Dictionary<string, Schema>() { { "foo", new Schema() } };
      schema.OptionalProperties = new Dictionary<string, Schema>() { { "foo", new Schema() } };
      schema.Values = new Schema();
      schema.Discriminator = new Discriminator();
      schema.Discriminator.Tag = "foo";
      schema.Discriminator.Mapping = new Dictionary<string, Schema>() { { "foo", new Schema() } };

      Assert.Equal(
          @"{""definitions"":{""foo"":{}},""ref"":""foo"",""type"":""int8"",""enum"":[""""],""elements"":{},""properties"":{""foo"":{}},""optionalProperties"":{""foo"":{}},""values"":{},""discriminator"":{""tag"":""foo"",""mapping"":{""foo"":{}}}}",
          JsonConvert.SerializeObject(schema)
        );
    }
  }
}
