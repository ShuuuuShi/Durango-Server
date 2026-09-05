using MsgPack;

namespace Messages;

public struct TechSupportEstimateResult
{
	public const uint TypeCode = 59142u;

	public TechSupportEstimate Estimate;

	public int RequestCount;

	public static void Pack(Packer packer, TechSupportEstimateResult val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(59142u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		TechSupportEstimate.Pack(packer, val.Estimate);
		packer.Pack(val.RequestCount);
	}

	public static TechSupportEstimateResult Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TechSupportEstimateResult result = default(TechSupportEstimateResult);
		result.Estimate = TechSupportEstimate.Unpack(unpacker);
		unpacker.Read();
		result.RequestCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<TechSupportEstimateResult Estimate={Estimate} RequestCount={RequestCount}>";
	}
}
