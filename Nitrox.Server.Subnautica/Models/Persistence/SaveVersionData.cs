using System;
using System.Runtime.Serialization;

namespace Nitrox.Server.Subnautica.Models.Persistence;

[DataContract]
internal record SaveVersionData
{
    [DataMember(Order = 1)]
    public int Major { get; init; }

    [DataMember(Order = 2)]
    public int Minor { get; init; }

    [DataMember(Order = 3)]
    public int Build { get; init; }

    [DataMember(Order = 4)]
    public int Revision { get; init; }

    public SaveVersionData(Version version)
    {
        Major = version.Major;
        Minor = version.Minor;
        Build = version.Build;
        Revision = version.Revision;
    }

    public SaveVersionData()
    {
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Build}.{Revision}";
    }
}
