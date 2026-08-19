using MsgPack;

namespace Messages;

public struct SendMail
{
	public const uint TypeCode = 2077u;

	public ulong RecipientId;

	public string Text;

	public ulong[] ItemIds;

	public static void Pack(Packer packer, SendMail val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2077u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.RecipientId);
		if (val.Text == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Text);
		}
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ItemIds.Length);
		for (int i = 0; i < val.ItemIds.Length; i++)
		{
			packer.Pack(val.ItemIds[i]);
		}
	}

	public static SendMail Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SendMail result = default(SendMail);
		result.RecipientId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Text = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.ItemIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] itemIds = result.ItemIds;
			int num2 = i;
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			itemIds[num2] = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SendMail RecipientId={RecipientId} Text={Text} ItemIds={ItemIds}>";
	}
}
