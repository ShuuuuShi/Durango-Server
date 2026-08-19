using MsgPack;

namespace Messages;

public struct Crack
{
	public double? ActivatedSince;

	public double? ActivatedUntil;

	public int CurrentInvestment;

	public int RequiredInvestment;

	public int InvestmentUnit;

	public string[] PotentialBiocoms;

	public static void Pack(Packer packer, Crack val, bool hint = false)
	{
		packer.PackArrayHeader(6);
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
		if (val.PotentialBiocoms == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.PotentialBiocoms.Length);
		for (int i = 0; i < val.PotentialBiocoms.Length; i++)
		{
			packer.PackString(val.PotentialBiocoms[i]);
		}
	}

	public static Crack Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Crack result = default(Crack);
		if (unpacker.LastReadData.IsNil)
		{
			result.ActivatedSince = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.ActivatedSince = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ActivatedUntil = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.ActivatedUntil = value2;
		}
		unpacker.Read();
		result.CurrentInvestment = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.RequiredInvestment = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.InvestmentUnit = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.PotentialBiocoms = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.PotentialBiocoms[i] = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Crack ActivatedSince={ActivatedSince} ActivatedUntil={ActivatedUntil} CurrentInvestment={CurrentInvestment} RequiredInvestment={RequiredInvestment} InvestmentUnit={InvestmentUnit} PotentialBiocoms={PotentialBiocoms}>";
	}
}
