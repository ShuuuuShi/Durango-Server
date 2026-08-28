using MsgPack;

namespace Messages;

public struct IslandTravelOptions
{
    public const uint TypeCode = 2131u;

    public string[] Ids;
    public string[] Names;
    public int[] RequiredLevels;

    public static void Pack(Packer packer, IslandTravelOptions val, bool hint = false)
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
        PackStrings(packer, val.Ids);
        PackStrings(packer, val.Names);
        if (val.RequiredLevels == null)
        {
            packer.PackArrayHeader(0);
        }
        else
        {
            packer.PackArrayHeader(val.RequiredLevels.Length);
            for (int i = 0; i < val.RequiredLevels.Length; i++)
            {
                packer.Pack(val.RequiredLevels[i]);
            }
        }
    }

    private static void PackStrings(Packer packer, string[] values)
    {
        if (values == null)
        {
            packer.PackArrayHeader(0);
            return;
        }
        packer.PackArrayHeader(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            packer.PackString(values[i] ?? string.Empty);
        }
    }

    public static IslandTravelOptions Unpack(Unpacker unpacker)
    {
        IslandTravelOptions result = default;
        unpacker.Read();
        int count = unpacker.LastReadData.AsInt32();
        result.Ids = new string[count];
        for (int i = 0; i < count; i++)
        {
            unpacker.Read();
            result.Ids[i] = unpacker.LastReadData.AsString();
        }
        unpacker.Read();
        count = unpacker.LastReadData.AsInt32();
        result.Names = new string[count];
        for (int i = 0; i < count; i++)
        {
            unpacker.Read();
            result.Names[i] = unpacker.LastReadData.AsString();
        }
        unpacker.Read();
        count = unpacker.LastReadData.AsInt32();
        result.RequiredLevels = new int[count];
        for (int i = 0; i < count; i++)
        {
            unpacker.Read();
            result.RequiredLevels[i] = unpacker.LastReadData.AsInt32();
        }
        return result;
    }

    public override string ToString() => $"<IslandTravelOptions Count={Ids?.Length ?? 0}>";
}
