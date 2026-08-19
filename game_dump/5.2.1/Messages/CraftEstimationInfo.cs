using MsgPack;

namespace Messages;

public struct CraftEstimationInfo
{
	public const uint TypeCode = 406132u;

	public int CraftLevel;

	public CraftEstimation? CraftEstimation;

	public static void Pack(Packer packer, CraftEstimationInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(406132u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.CraftLevel);
		if (!val.CraftEstimation.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.CraftEstimation.Pack(packer, val.CraftEstimation.Value);
		}
	}

	public static CraftEstimationInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CraftEstimationInfo result = default(CraftEstimationInfo);
		result.CraftLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CraftEstimation = null;
		}
		else
		{
			CraftEstimation value = Messages.CraftEstimation.Unpack(unpacker);
			result.CraftEstimation = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CraftEstimationInfo CraftLevel={CraftLevel} CraftEstimation={CraftEstimation}>";
	}
}
