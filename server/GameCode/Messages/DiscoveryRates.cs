using MsgPack;

namespace Messages;

public struct DiscoveryRates
{
	public const uint TypeCode = 5004u;

	public Pair<string, float>[] Rates;

	public static void Pack(Packer packer, DiscoveryRates val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5004u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Rates == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Rates.Length);
		for (int i = 0; i < val.Rates.Length; i++)
		{
			packer.PackArrayHeader(2);
			if (val.Rates[i].Item1 == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Rates[i].Item1);
			}
			packer.Pack(val.Rates[i].Item2);
		}
	}

	public static DiscoveryRates Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		DiscoveryRates result = default(DiscoveryRates);
		result.Rates = new Pair<string, float>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			float item2 = unpacker.LastReadData.AsSingle();
			ref Pair<string, float> reference = ref result.Rates[i];
			reference = new Pair<string, float>(item, item2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DiscoveryRates Rates={Rates}>";
	}
}
