using MsgPack;

namespace Messages;

/// <summary>Optional M5 handshake sent after Welcome and before Ready.</summary>
public struct ModHello
{
    public const uint TypeCode = 2000000002u;
    public int Protocol;
    public string ManifestJson;
    public string CatalogHash;

    public static void Pack(Packer packer, ModHello value, bool hint = false)
    {
        packer.PackArrayHeader(hint ? 4 : 3);
        if (hint) packer.Pack(TypeCode);
        packer.Pack(value.Protocol);
        packer.PackString(value.ManifestJson ?? "");
        packer.PackString(value.CatalogHash ?? "");
    }

    public static ModHello Unpack(Unpacker unpacker)
    {
        unpacker.Read(); ModHello result = default; result.Protocol = unpacker.LastReadData.AsInt32();
        unpacker.Read(); result.ManifestJson = unpacker.LastReadData.AsString();
        unpacker.Read(); result.CatalogHash = unpacker.LastReadData.AsString();
        return result;
    }

    public override string ToString() => $"<ModHello Protocol={Protocol} CatalogHash={CatalogHash}>";
}
