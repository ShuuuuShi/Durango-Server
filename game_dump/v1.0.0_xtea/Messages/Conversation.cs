using MsgPack;

namespace Messages;

public struct Conversation
{
	public const uint TypeCode = 2412u;

	public ulong Id;

	public Message_[] Messages;

	public ulong[] EntityIds;

	public static void Pack(Packer packer, Conversation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2412u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.Id);
		if (val.Messages == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Messages.Length);
			for (int i = 0; i < val.Messages.Length; i++)
			{
				Message_.Pack(packer, val.Messages[i]);
			}
		}
		if (val.EntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.EntityIds.Length);
		for (int j = 0; j < val.EntityIds.Length; j++)
		{
			packer.Pack(val.EntityIds[j]);
		}
	}

	public static Conversation Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Conversation result = default(Conversation);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Messages = new Message_[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Message_ reference = ref result.Messages[i];
			reference = Message_.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.EntityIds = new ulong[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ulong[] entityIds = result.EntityIds;
			int num3 = j;
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			entityIds[num3] = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Conversation Id={Id} Messages={Messages} EntityIds={EntityIds}>";
	}
}
