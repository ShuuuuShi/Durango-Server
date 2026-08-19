using MsgPack;

namespace Messages;

public struct FactionRadioHistories
{
	public const uint TypeCode = 3632u;

	public FactionRadioHistory[] Histories;

	public static void Pack(Packer packer, FactionRadioHistories val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3632u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Histories == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Histories.Length);
		for (int i = 0; i < val.Histories.Length; i++)
		{
			FactionRadioHistory.Pack(packer, val.Histories[i]);
		}
	}

	public static FactionRadioHistories Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FactionRadioHistories result = default(FactionRadioHistories);
		result.Histories = new FactionRadioHistory[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref FactionRadioHistory reference = ref result.Histories[i];
			reference = FactionRadioHistory.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FactionRadioHistories Histories={Histories}>";
	}
}
