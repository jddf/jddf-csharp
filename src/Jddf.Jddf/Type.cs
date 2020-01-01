using System.Runtime.Serialization;

namespace Jddf.Jddf
{
    public enum Type
    {
        [EnumMember(Value = "boolean")]
        Boolean,

        [EnumMember(Value = "float32")]
        Float32,

        [EnumMember(Value = "float64")]
        Float64,

        [EnumMember(Value = "int8")]
        Int8,

        [EnumMember(Value = "uint8")]
        Uint8,

        [EnumMember(Value = "int16")]
        Int16,

        [EnumMember(Value = "uint16")]
        Uint16,

        [EnumMember(Value = "int32")]
        Int32,

        [EnumMember(Value = "int64")]
        Uint32,

        [EnumMember(Value = "string")]
        String,

        [EnumMember(Value = "timestamp")]
        Timestamp
    }
}
