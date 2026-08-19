using MsgPack;

namespace Messages;

public struct SetSharedConcertMusic
{
	public const uint TypeCode = 63459180u;

	public string EntityId;

	public Point2 Tile;

	public int Order;

	public string SharedSheetId;

	public string MusicName;

	public static void Pack(Packer packer, SetSharedConcertMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(63459180u);
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
		if (val.SharedSheetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SharedSheetId);
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

	public static SetSharedConcertMusic Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetSharedConcertMusic result = default(SetSharedConcertMusic);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Order = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.SharedSheetId = unpacker.LastReadData.AsString();
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
		return $"<SetSharedConcertMusic EntityId={EntityId} Tile={Tile} Order={Order} SharedSheetId={SharedSheetId} MusicName={MusicName}>";
	}
}
