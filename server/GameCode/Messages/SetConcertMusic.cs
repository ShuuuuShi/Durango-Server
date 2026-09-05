using MsgPack;

namespace Messages;

public struct SetConcertMusic
{
	public const uint TypeCode = 63459080u;

	public string EntityId;

	public Point2 Tile;

	public int Order;

	public int? Slot;

	public string MusicName;

	public static void Pack(Packer packer, SetConcertMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(63459080u);
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
		packer.Pack(val.Order);
		if (!val.Slot.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Slot.Value);
		}
		if (val.MusicName == null)
		{
			packer.PackNull();
		}
		else if (val.MusicName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.MusicName);
		}
	}

	public static SetConcertMusic Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetConcertMusic result = default(SetConcertMusic);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Order = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Slot = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Slot = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.MusicName = null;
		}
		else
		{
			string musicName = unpacker.LastReadData.AsString();
			result.MusicName = musicName;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetConcertMusic EntityId={EntityId} Tile={Tile} Order={Order} Slot={Slot} MusicName={MusicName}>";
	}
}
