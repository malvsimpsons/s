extern alias JB;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NitroxModel.DataStructures;

namespace Nitrox.Server.Subnautica.Database.Converters;

internal sealed class NitroxInt3Converter() : ValueConverter<NitroxInt3, byte[]>(static id => Int3ToBytes(id),
                                                                                 static bytes => BytesToInt3(bytes))
{
    [SkipLocalsInit]
    static unsafe byte[] Int3ToBytes(in NitroxInt3 int3)
    {
        byte[] result = new byte[sizeof(NitroxInt3)];
        MemoryMarshal.Write(result, int3);
        return result;
    }

    static NitroxInt3 BytesToInt3(byte[] bytes) => MemoryMarshal.Read<NitroxInt3>(bytes);
}
