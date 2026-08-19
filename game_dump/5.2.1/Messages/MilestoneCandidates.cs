using MsgPack;

namespace Messages;

public struct MilestoneCandidates
{
	public const uint TypeCode = 800011u;

	public Pair<string, float>[] Result;

	public Pair<string, float>[] Original;

	public static void Pack(Packer packer, MilestoneCandidates val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(800011u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Result == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Result.Length);
			for (int i = 0; i < val.Result.Length; i++)
			{
				packer.PackArrayHeader(2);
				if (val.Result[i].Item1 == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Result[i].Item1);
				}
				packer.Pack(val.Result[i].Item2);
			}
		}
		if (val.Original == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Original.Length);
		for (int j = 0; j < val.Original.Length; j++)
		{
			packer.PackArrayHeader(2);
			if (val.Original[j].Item1 == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Original[j].Item1);
			}
			packer.Pack(val.Original[j].Item2);
		}
	}

	public static MilestoneCandidates Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		MilestoneCandidates result = default(MilestoneCandidates);
		result.Result = new Pair<string, float>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			float item2 = unpacker.LastReadData.AsSingle();
			ref Pair<string, float> reference = ref result.Result[i];
			reference = new Pair<string, float>(item, item2);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Original = new Pair<string, float>[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			unpacker.Read();
			string item3 = unpacker.LastReadData.AsString();
			unpacker.Read();
			float item4 = unpacker.LastReadData.AsSingle();
			ref Pair<string, float> reference2 = ref result.Original[j];
			reference2 = new Pair<string, float>(item3, item4);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<MilestoneCandidates Result={Result} Original={Original}>";
	}
}
