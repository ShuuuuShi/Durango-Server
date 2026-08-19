using MsgPack;

namespace Messages;

public struct Seasons
{
	public const uint TypeCode = 871246u;

	public Season[] _Seasons;

	public static void Pack(Packer packer, Seasons val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(871246u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Seasons == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._Seasons.Length);
		for (int i = 0; i < val._Seasons.Length; i++)
		{
			Season.Pack(packer, val._Seasons[i]);
		}
	}

	public static Seasons Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Seasons result = default(Seasons);
		result._Seasons = new Season[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Season reference = ref result._Seasons[i];
			reference = Season.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Seasons _Seasons={_Seasons}>";
	}
}
