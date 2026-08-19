using MsgPack;

namespace Messages;

public struct POICount
{
	public const uint TypeCode = 901u;

	public byte PortCount;

	public byte WarpholeCount;

	public byte CraterCount;

	public byte RiftCount;

	public static void Pack(Packer packer, POICount val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(901u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.PortCount);
		packer.Pack(val.WarpholeCount);
		packer.Pack(val.CraterCount);
		packer.Pack(val.RiftCount);
	}

	public static POICount Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		POICount result = default(POICount);
		result.PortCount = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.WarpholeCount = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.CraterCount = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.RiftCount = unpacker.LastReadData.AsByte();
		return result;
	}

	public override string ToString()
	{
		return $"<POICount PortCount={PortCount} WarpholeCount={WarpholeCount} CraterCount={CraterCount} RiftCount={RiftCount}>";
	}
}
