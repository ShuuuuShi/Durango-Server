using MsgPack;

namespace Messages;

public struct Market
{
	public const uint TypeCode = 5010u;

	public ulong Id;

	public ulong RegionId;

	public Point2 Tile;

	public ulong SellerId;

	public double? ExpiresAt;

	public string Name;

	public ScribbleContent? Scribble;

	public static void Pack(Packer packer, Market val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(8);
			packer.Pack(5010u);
		}
		else
		{
			packer.PackArrayHeader(7);
		}
		packer.Pack(val.Id);
		packer.Pack(val.RegionId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.SellerId);
		if (!val.ExpiresAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ExpiresAt.Value);
		}
		if (val.Name == null)
		{
			packer.PackNull();
		}
		else if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
		if (!val.Scribble.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			ScribbleContent.Pack(packer, val.Scribble.Value);
		}
	}

	public static Market Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Market result = default(Market);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.RegionId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.SellerId = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.ExpiresAt = null;
		}
		else
		{
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			double value = ((MessagePackObject)(ref lastReadData5)).AsDouble();
			result.ExpiresAt = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData6)).IsNil)
		{
			result.Name = null;
		}
		else
		{
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			string name = ((MessagePackObject)(ref lastReadData7)).AsString();
			result.Name = name;
		}
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData8)).IsNil)
		{
			result.Scribble = null;
		}
		else
		{
			ScribbleContent value2 = ScribbleContent.Unpack(unpacker);
			result.Scribble = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Market Id={Id} RegionId={RegionId} Tile={Tile} SellerId={SellerId} ExpiresAt={ExpiresAt} Name={Name} Scribble={Scribble}>";
	}
}
