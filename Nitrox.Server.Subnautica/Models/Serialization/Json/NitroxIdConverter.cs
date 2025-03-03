using System;
using System.Text.Json;
using NitroxModel.DataStructures;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json;

public class NitroxIdConverter : System.Text.Json.Serialization.JsonConverter<NitroxId>
{
    public override NitroxId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => !reader.HasValueSequence ? null : new NitroxId(reader.GetString());

    public override void Write(Utf8JsonWriter writer, NitroxId value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
