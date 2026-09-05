using MsgPack;

namespace Messages;

public struct ResistanceExpCap
{
	public int CapIndex;

	public int[] ExpLimits;

	public float ExpRate;

	public double ExpiresAt;

	public static void Pack(Packer packer, ResistanceExpCap val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.Pack(val.CapIndex);
		if (val.ExpLimits == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.ExpLimits.Length);
			for (int i = 0; i < val.ExpLimits.Length; i++)
			{
				packer.Pack(val.ExpLimits[i]);
			}
		}
		packer.Pack(val.ExpRate);
		packer.Pack(val.ExpiresAt);
	}

	public static ResistanceExpCap Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ResistanceExpCap result = default(ResistanceExpCap);
		result.CapIndex = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ExpLimits = new int[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ExpLimits[i] = unpacker.LastReadData.AsInt32();
		}
		unpacker.Read();
		result.ExpRate = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.ExpiresAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<ResistanceExpCap CapIndex={CapIndex} ExpLimits={ExpLimits} ExpRate={ExpRate} ExpiresAt={ExpiresAt}>";
	}
}
