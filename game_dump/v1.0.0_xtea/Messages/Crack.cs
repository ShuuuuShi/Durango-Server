using MsgPack;

namespace Messages;

public struct Crack
{
	public double? ActivatedSince;

	public double? ActivatedUntil;

	public int CurrentInvestment;

	public int RequiredInvestment;

	public int InvestmentUnit;

	public static void Pack(Packer packer, Crack val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (!val.ActivatedSince.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ActivatedSince.Value);
		}
		if (!val.ActivatedUntil.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ActivatedUntil.Value);
		}
		packer.Pack(val.CurrentInvestment);
		packer.Pack(val.RequiredInvestment);
		packer.Pack(val.InvestmentUnit);
	}

	public static Crack Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Crack result = default(Crack);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.ActivatedSince = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			double value = ((MessagePackObject)(ref lastReadData2)).AsDouble();
			result.ActivatedSince = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.ActivatedUntil = null;
		}
		else
		{
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			double value2 = ((MessagePackObject)(ref lastReadData4)).AsDouble();
			result.ActivatedUntil = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.CurrentInvestment = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.RequiredInvestment = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.InvestmentUnit = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<Crack ActivatedSince={ActivatedSince} ActivatedUntil={ActivatedUntil} CurrentInvestment={CurrentInvestment} RequiredInvestment={RequiredInvestment} InvestmentUnit={InvestmentUnit}>";
	}
}
