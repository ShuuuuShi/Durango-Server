using MsgPack;

namespace Messages;

public struct Rename
{
	public const uint TypeCode = 324u;

	public string EntityId;

	public Point2 Tile;

	public string Name;

	public string PrevName;

	public bool IsFirstRename;

	public static void Pack(Packer packer, Rename val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(324u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
		if (val.PrevName == null)
		{
			packer.PackNull();
		}
		else if (val.PrevName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrevName);
		}
		packer.Pack(val.IsFirstRename);
	}

	public static Rename Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Rename result = default(Rename);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Name = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PrevName = null;
		}
		else
		{
			string prevName = unpacker.LastReadData.AsString();
			result.PrevName = prevName;
		}
		unpacker.Read();
		result.IsFirstRename = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Rename EntityId={EntityId} Tile={Tile} Name={Name} PrevName={PrevName} IsFirstRename={IsFirstRename}>";
	}
}
