using MsgPack;

namespace Messages;

public struct POICount
{
	public const uint TypeCode = 901u;

	public byte PortCount;

	public byte WarpholeCount;

	public byte CraterCount;

	public static void Pack(Packer packer, POICount val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(901u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.PortCount);
		packer.Pack(val.WarpholeCount);
		packer.Pack(val.CraterCount);
	}

	public static POICount Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		POICount result = default(POICount);
		result.PortCount = ((MessagePackObject)(ref lastReadData)).AsByte();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.WarpholeCount = ((MessagePackObject)(ref lastReadData2)).AsByte();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.CraterCount = ((MessagePackObject)(ref lastReadData3)).AsByte();
		return result;
	}

	public override string ToString()
	{
		return $"<POICount PortCount={PortCount} WarpholeCount={WarpholeCount} CraterCount={CraterCount}>";
	}
}
