using MsgPack;

namespace Messages;

public struct RequestTechSupportEstimate
{
	public const uint TypeCode = 59141u;

	public string EntityId;

	public Point2 Tile;

	public string ItemId;

	public int ReformSlotIndex;

	public string[] TagsToLock;

	public static void Pack(Packer packer, RequestTechSupportEstimate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(59141u);
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
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
		packer.Pack(val.ReformSlotIndex);
		if (val.TagsToLock == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.TagsToLock.Length);
		for (int i = 0; i < val.TagsToLock.Length; i++)
		{
			if (val.TagsToLock[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.TagsToLock[i]);
			}
		}
	}

	public static RequestTechSupportEstimate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RequestTechSupportEstimate result = default(RequestTechSupportEstimate);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.ItemId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ReformSlotIndex = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.TagsToLock = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.TagsToLock[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RequestTechSupportEstimate EntityId={EntityId} Tile={Tile} ItemId={ItemId} ReformSlotIndex={ReformSlotIndex} TagsToLock={TagsToLock}>";
	}
}
