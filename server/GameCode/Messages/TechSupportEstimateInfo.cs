using MsgPack;

namespace Messages;

public struct TechSupportEstimateInfo
{
	public TechSupportEstimate? Estimate;

	public int RequestCount;

	public static void Pack(Packer packer, TechSupportEstimateInfo val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (!val.Estimate.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			TechSupportEstimate.Pack(packer, val.Estimate.Value);
		}
		packer.Pack(val.RequestCount);
	}

	public static TechSupportEstimateInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TechSupportEstimateInfo result = default(TechSupportEstimateInfo);
		if (unpacker.LastReadData.IsNil)
		{
			result.Estimate = null;
		}
		else
		{
			TechSupportEstimate value = TechSupportEstimate.Unpack(unpacker);
			result.Estimate = value;
		}
		unpacker.Read();
		result.RequestCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<TechSupportEstimateInfo Estimate={Estimate} RequestCount={RequestCount}>";
	}
}
