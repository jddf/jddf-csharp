using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Jddf.Jddf.Test
{
    public class ValidatorTest
    {
        [Fact]
        public void MaxDepth()
        {
            var serializeOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
            serializeOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            string schemaJson = @"
                {
                    ""definitions"": {
                        """": { ""ref"": """"}
                    },
                    ""ref"": """"
                }
            ";

            string instanceJson = "null";

            Schema schema = JsonSerializer.Deserialize<Schema>(schemaJson, serializeOptions);
            JsonElement instance = JsonSerializer.Deserialize<JsonElement>(instanceJson, serializeOptions);
            Validator validator = new Validator();
            validator.MaxDepth = 3;

            Assert.Throws<MaxDepthExceededException>(() => {
                validator.Validate(schema, instance);
            });
        }

        [Fact]
        public void MaxErrors()
        {
            var serializeOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
            serializeOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            string schemaJson = @"
                {
                    ""elements"": {
                        ""type"": ""string""
                    }
                }
            ";

            string instanceJson = "[null, null, null, null, null]";

            Schema schema = JsonSerializer.Deserialize<Schema>(schemaJson, serializeOptions);
            JsonElement instance = JsonSerializer.Deserialize<JsonElement>(instanceJson, serializeOptions);
            Validator validator = new Validator();
            validator.MaxErrors = 3;

            Assert.Equal(3, validator.Validate(schema, instance).Count);
        }

        [Fact]
        public void ValidationSpec()
        {
            var serializeOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
            serializeOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            foreach (string path in Directory.GetFiles("../../../../../spec/tests/validation"))
            {
                string fileContents = File.ReadAllText(path);
                List<ValidationTestSuite> suites = JsonSerializer.Deserialize<List<ValidationTestSuite>>(fileContents, serializeOptions);

                foreach (ValidationTestSuite suite in suites) {
                    foreach (ValidationTestCase testCase in suite.Instances) {
                        List<ValidationError> expected = testCase.Errors.ConvertAll(
                            new System.Converter<ValidationTestCaseError, ValidationError>(
                                (error) => {
                                    List<string> instancePath = new List<string>(error.InstancePath.Split('/'));
                                    List<string> schemaPath = new List<string>(error.SchemaPath.Split('/'));

                                    instancePath.RemoveAt(0);
                                    schemaPath.RemoveAt(0);

                                    return new ValidationError(instancePath, schemaPath);
                                }
                            )
                        );

                        List<ValidationError> actual = (List<ValidationError>) new Validator().Validate(suite.Schema, testCase.Instance);

                        expected.Sort((ValidationError a, ValidationError b) => {
                            return System.String.Join("", a.SchemaPath).CompareTo(System.String.Join("", b.SchemaPath));
                        });

                        actual.Sort((ValidationError a, ValidationError b) => {
                            return System.String.Join("", a.SchemaPath).CompareTo(System.String.Join("", b.SchemaPath));
                        });

                        Assert.Equal(expected, actual);
                    }
                }
            }
        }

        private class ValidationTestSuite
        {
            public string Name { get; set; }
            public Schema Schema { get; set; }
            public List<ValidationTestCase> Instances { get; set; }
        }

        private class ValidationTestCase
        {
            public JsonElement Instance { get; set; }
            public List<ValidationTestCaseError> Errors { get; set; }
        }

        private class ValidationTestCaseError
        {
            public string InstancePath { get; set; }
            public string SchemaPath { get; set; }
        }
    }
}
