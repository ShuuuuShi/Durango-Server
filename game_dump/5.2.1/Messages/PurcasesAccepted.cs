using MsgPack;

namespace Messages;

public struct PurcasesAccepted
{
	public const uint TypeCode = 5247811u;

	public int TotalCount;

	public int AcceptedCount;

	public string[] Errors;

	public static void Pack(Packer packer, PurcasesAccepted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(5247811u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.TotalCount);
		packer.Pack(val.AcceptedCount);
		if (val.Errors == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Errors.Length);
		for (int i = 0; i < val.Errors.Length; i++)
		{
			packer.PackString(val.Errors[i]);
		}
	}

	public static PurcasesAccepted Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PurcasesAccepted result = default(PurcasesAccepted);
		result.TotalCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.AcceptedCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Errors = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Errors[i] = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PurcasesAccepted TotalCount={TotalCount} AcceptedCount={AcceptedCount} Errors={Errors}>";
	}
}
