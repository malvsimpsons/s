using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Serialization.Json;

public class TechTypeConverter : JsonConverter<NitroxTechType>
{
    public override NitroxTechType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => !reader.HasValueSequence ? null : new NitroxTechType(reader.GetString());

    public override void Write(Utf8JsonWriter writer, NitroxTechType value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);
}
