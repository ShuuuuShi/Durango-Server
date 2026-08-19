using MsgPack;
using Shared.Faction;

namespace Messages;

public struct FactionRadio
{
	public const uint TypeCode = 3633u;

	public FactionType Faction;

	public string[] Messages;

	public bool ShowPortrait;

	public static void Pack(Packer packer, FactionRadio val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3633u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Faction);
		if (val.Messages == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Messages.Length);
			for (int i = 0; i < val.Messages.Length; i++)
			{
				packer.PackString(val.Messages[i]);
			}
		}
		packer.Pack(val.ShowPortrait);
	}

	public static FactionRadio Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FactionRadio result = default(FactionRadio);
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
		result.Messages = new string[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			result.Messages[i] = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.ShowPortrait = ((MessagePackObject)(ref lastReadData3)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<FactionRadio Faction={Faction} Messages={Messages} ShowPortrait={ShowPortrait}>";
	}
}
