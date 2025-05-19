namespace Nitrox.Server.Subnautica.Models.Logging.Redaction;

internal readonly record struct SensitiveData<T>(T Data) : ISensitiveData
{
    public static implicit operator SensitiveData<T>(T data) => new(data);
    public override string ToString() =>
        Data switch
        {
            string s => s,
            _ => Data.ToString() ?? ""
        };
}
