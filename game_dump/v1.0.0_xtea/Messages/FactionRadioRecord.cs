using MsgPack;

namespace Messages;

public struct FactionRadioRecord
{
	public string[] Messages;

	public double ReceivedAt;

	public static void Pack(Packer packer, FactionRadioRecord val, bool hint = false)
	{
		packer.PackArrayHeader(2);
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
		packer.Pack(val.ReceivedAt);
	}

	public static FactionRadioRecord Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FactionRadioRecord result = default(FactionRadioRecord);
		result.Messages = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Messages[i] = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ReceivedAt = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<FactionRadioRecord Messages={Messages} ReceivedAt={ReceivedAt}>";
	}
}
