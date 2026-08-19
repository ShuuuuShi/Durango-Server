using MsgPack;

namespace Messages;

public struct ExtendEstateActivation
{
	public const uint TypeCode = 3822u;

	public string EstateId;

	public long Cost;

	public static void Pack(Packer packer, ExtendEstateActivation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3822u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EstateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EstateId);
		}
		packer.Pack(val.Cost);
	}

	public static ExtendEstateActivation Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ExtendEstateActivation result = default(ExtendEstateActivation);
		result.EstateId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Cost = unpacker.LastReadData.AsInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<ExtendEstateActivation EstateId={EstateId} Cost={Cost}>";
	}
}
