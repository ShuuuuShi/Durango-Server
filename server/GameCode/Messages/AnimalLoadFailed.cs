using MsgPack;

namespace Messages;

public struct AnimalLoadFailed
{
    public const uint TypeCode = 2000000003u;
    public string EntityId;
    public string Reason;

    public static void Pack(Packer packer, AnimalLoadFailed value, bool hint = false)
    {
        packer.PackArrayHeader(hint ? 3 : 2);
        if (hint) packer.Pack(TypeCode);
        packer.PackString(value.EntityId ?? "");
        packer.PackString(value.Reason ?? "");
    }

    public static AnimalLoadFailed Unpack(Unpacker unpacker)
    {
        unpacker.Read();
        AnimalLoadFailed result = default;
        result.EntityId = unpacker.LastReadData.AsString();
        unpacker.Read();
        result.Reason = unpacker.LastReadData.AsString();
        return result;
    }
}