using MsgPack;

namespace Messages;

public struct StrangeRadio
{
	public const uint TypeCode = 3634u;

	public string[] Messages;

	public static void Pack(Packer packer, StrangeRadio val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3634u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Messages == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Messages.Length);
		for (int i = 0; i < val.Messages.Length; i++)
		{
			packer.PackString(val.Messages[i]);
		}
	}

	public static StrangeRadio Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		StrangeRadio result = default(StrangeRadio);
		result.Messages = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Messages[i] = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("<StrangeRadio Messages={0}>", Messages);
	}
}
