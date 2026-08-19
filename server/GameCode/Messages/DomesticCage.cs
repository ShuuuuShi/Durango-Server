using MsgPack;

namespace Messages;

public struct DomesticCage
{
	public const uint TypeCode = 694358u;

	public byte Size;

	public byte RemainSize;

	public DomesticationInfo[] Reins;

	public static void Pack(Packer packer, DomesticCage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(694358u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.Size);
		packer.Pack(val.RemainSize);
		if (val.Reins == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Reins.Length);
		for (int i = 0; i < val.Reins.Length; i++)
		{
			DomesticationInfo.Pack(packer, val.Reins[i]);
		}
	}

	public static DomesticCage Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DomesticCage result = default(DomesticCage);
		result.Size = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.RemainSize = unpacker.LastReadData.AsByte();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Reins = new DomesticationInfo[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref DomesticationInfo reference = ref result.Reins[i];
			reference = DomesticationInfo.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DomesticCage Size={Size} RemainSize={RemainSize} Reins={Reins}>";
	}
}
