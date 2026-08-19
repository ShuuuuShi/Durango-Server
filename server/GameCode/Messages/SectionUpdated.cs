using MsgPack;

namespace Messages;

public struct SectionUpdated
{
	public const uint TypeCode = 5298430u;

	public string EntityId;

	public Point2 Tile;

	public InventorySectionInfos? Added;

	public string Removed;

	public Pair<string, string>? Renamed;

	public static void Pack(Packer packer, SectionUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(5298430u);
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
		if (!val.Added.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			InventorySectionInfos.Pack(packer, val.Added.Value);
		}
		if (val.Removed == null)
		{
			packer.PackNull();
		}
		else if (val.Removed == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Removed);
		}
		if (!val.Renamed.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		if (val.Renamed.Value.Item1 == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Renamed.Value.Item1);
		}
		if (val.Renamed.Value.Item2 == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Renamed.Value.Item2);
		}
	}

	public static SectionUpdated Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SectionUpdated result = default(SectionUpdated);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Added = null;
		}
		else
		{
			InventorySectionInfos value = InventorySectionInfos.Unpack(unpacker);
			result.Added = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Removed = null;
		}
		else
		{
			string removed = unpacker.LastReadData.AsString();
			result.Removed = removed;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Renamed = null;
		}
		else
		{
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			string item2 = unpacker.LastReadData.AsString();
			Pair<string, string> value2 = new Pair<string, string>(item, item2);
			result.Renamed = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SectionUpdated EntityId={EntityId} Tile={Tile} Added={Added} Removed={Removed} Renamed={Renamed}>";
	}
}
