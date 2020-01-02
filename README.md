# jddf-csharp ![Nuget](https://img.shields.io/nuget/v/Jddf.Jddf)

This package is a C# / .NET implementation of **JSON Data Definition Format**.
In particular, it lets you:

1. Validate input data is valid against a JDDF schema,
2. Get a list of validation errors from that input data, and
3. Build your own tooling on top of JSON Data Definition Format

This package integrates with both [`Newtonsoft.Json`][newtonsoft] (also known as
`Json.NET`) and the newer [`System.Text.Json`][system].

[newtonsoft]: https://www.nuget.org/packages/Newtonsoft.Json
[system]: https://www.nuget.org/packages/System.Text.Json

## Installation

This package is available on nuget.org under the name `Jddf.Jddf`.

[The `Jddf.Jddf` package page on nuget.org][nuget] has information on how to
install this package as a dependency. If you're using the .NET CLI, you can
install `Jddf.Jddf` by running:

```bash
dotnet add package Jddf.Jddf
```

[nuget]: https://www.nuget.org/packages/Jddf.Jddf/

## Usage

> The examples in this section use C#, but `Jddf.Jddf` is compatible with all
> Common Language Infrastructure languages, such as F# or Visual Basic .NET.

### Parsing a JDDF Schema from JSON

You can create a JDDF schema, represented by the class `Jddf.Jddf.Schema`, from
a JSON document using either `Newtonsoft.Json` or `System.Text.Json`.

Whichever JSON package you choose, let's first say you already have a JDDF in
JSON format in a variable:

```cs
// The syntax of this string may seem strange.
//
// The @" syntax in C# defines a "verbatim string literal". In these @" strings,
// you can include a quotation mark (") by escaping it with another quotation
// mark.
string schemaJson = @"
  {
    ""properties"": {
      ""name"": { ""type"": ""string"" },
      ""age"": { ""type"": ""uint8"" },
      ""phones"": {
        ""elements"": { ""type"": ""string"" }
      }
    }
  }
";
```

How you parse this JSON into a JDDF schema (an instance of `Jddf.Jddf.Schema`)
depends on what JSON library you're using:

* Parsing a JDDF schema using `System.Text.Json`:

  ```cs
  using Jddf.Jddf;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  JsonSerializerOptions serializeOptions = new JsonSerializerOptions
  {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      IgnoreNullValues = true
  };
  serializeOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

  Schema schema = JsonSerializer.Deserialize<Schema>(schemaJson, serializeOptions);
  ```

* Parsing a JDDF schema using `Newtonsoft.Json`:

  ```cs
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json.Serialization;

  JsonSerializerSettings serializerSettings = new JsonSerializerSettings
  {
      ContractResolver = new DefaultContractResolver
      {
          NamingStrategy = new CamelCaseNamingStrategy()
      },
      NullValueHandling = NullValueHandling.Ignore,
      Converters = new List<JsonConverter> { new StringEnumConverter() }
  };

  Schema schema = JsonConvert.DeserializeObject<Schema>(schemaJson, serializerSettings);
  ```

### Validating JSON data against a JDDF schema

Now that you have a JDDF schema in hand, you can validate JSON data against it.
To do so, you will:

1. Parse the JSON input using `System.Text.Json`,
2. Create an instance of `Jddf.Jddf.Validator`,
3. Run `Validator.Validate`, passing in the parsed JSON from step (1) and the
   schema you parsed in the previous section.

Here's an example of how you'd do that:

```cs
// Populate `schema` using one of the two approaches shown above.
Schema schema = ; // ...

// This input data is perfect. It satisfies all the schema requirements.
string inputOkJson = @"
  {
    ""name"": ""John Doe"",
    ""age"": 43,
    ""phones"": [
      ""+44 1234567"",
      ""+44 2345678"",
    ]
  }
";

// This input data has problems. "name" is missing, "age" has the wrong type,
// and "phones[1]" has the wrong type.
string inputBadJson = @"
  {
    ""age"": ""43"",
    ""phones"": [
      ""+44 1234567"",
      442345678,
    ],
  }
";

// Parse the input data, and run them against a validator.
Validator validator = new Validator();

JsonElement inputOk = JsonSerializer.Deserialize<JsonElement>(inputOkJson);
IList<ValidationError> errorsOk = validator.validate(schema, inputOk);

JsonElement inputBad = JsonSerializer.Deserialize<JsonElement>(inputBadJson);
IList<ValidationError> errorsBad = validator.validate(schema, inputBad);

// inputOk passes the schema's validation. There are no validation errors.
System.Console.WriteLine(errorsOk.Count); // Outputs: 0

// inputBad fails the schema's validation. There is a nonzero number of errors.
System.Console.WriteLine(errorsBad.Count); // Outputs: 3

// Outputs:
//
// (empty string)
// properties/name
//
// This error indicates that "name" is missing, and was required.
System.Console.WriteLine(System.String.Join("/", errorsBad[0].InstancePath));
System.Console.WriteLine(System.String.Join("/", errorsBad[0].SchemaPath));

// Outputs:
//
// age
// properties/age/type
//
// This error indicates that "age" has the wrong type.
System.Console.WriteLine(System.String.Join("/", errorsBad[1].InstancePath));
System.Console.WriteLine(System.String.Join("/", errorsBad[1].SchemaPath));

// Outputs:
//
// phones/1
// properties/phones/elements/type
//
// This error indicates that "phones[1]" has the wrong type.
System.Console.WriteLine(System.String.Join("/", errorsBad[2].InstancePath));
System.Console.WriteLine(System.String.Join("/", errorsBad[2].SchemaPath));
```
