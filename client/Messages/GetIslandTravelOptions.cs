using MsgPack;

namespace Messages;

public struct GetIslandTravelOptions
{
    public const uint TypeCode = 2130u;

    public string EntityId;
    public Point2 Tile;

    public static void Pack(Packer packer, GetIslandTravelOptions val, bool hint = false)
    {
        if (hint)
        {
            packer.PackArrayHeader(4);
            packer.Pack(TypeCode);
        }
        else
        {
            packer.PackArrayHeader(3);
        }
        packer.PackString(val.EntityId ?? string.Empty);
        packer.PackArrayHeader(2);
        packer.Pack((ushort)val.Tile.x);
        packer.Pack((ushort)val.Tile.y);
    }

    public static GetIslandTravelOptions Unpack(Unpacker unpacker)
    {
        unpacker.Read();
        GetIslandTravelOptions result = default;
        result.EntityId = unpacker.LastReadData.AsString();
        unpacker.Read();
        unpacker.ReadUInt16(out ushort x);
        result.Tile.x = x;
        unpacker.ReadUInt16(out ushort y);
        result.Tile.y = y;
        return result;
    }

    public override string ToString() => $"<GetIslandTravelOptions EntityId={EntityId} Tile={Tile}>";
}
