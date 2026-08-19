using MsgPack;
using Shared.Faction;

namespace Messages;

public struct FactionRadioHistory
{
	public FactionType Faction;

	public FactionRadioRecord[] Messages;

	public static void Pack(Packer packer, FactionRadioHistory val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack((int)val.Faction);
		if (val.Messages == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Messages.Length);
		for (int i = 0; i < val.Messages.Length; i++)
		{
			FactionRadioRecord.Pack(packer, val.Messages[i]);
		}
	}

	public static FactionRadioHistory Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FactionRadioHistory result = default(FactionRadioHistory);
		if (num < 0 || 4 < num)
		{
			result.Faction = FactionType.Invalid;
		}
		else
		{
			result.Faction = (FactionType)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Messages = new FactionRadioRecord[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ref FactionRadioRecord reference = ref result.Messages[i];
			reference = FactionRadioRecord.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FactionRadioHistory Faction={Faction} Messages={Messages}>";
	}
}
