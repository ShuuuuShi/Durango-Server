using MsgPack;

namespace Messages;

public struct SendMail
{
	public const uint TypeCode = 2077u;

	public string RecipientId;

	public string Text;

	public string[] ItemIds;

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
		if (val.RecipientId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RecipientId);
		}
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
			if (val.ItemIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.ItemIds[i]);
			}
		}
	}

	public static SendMail Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SendMail result = default(SendMail);
		result.RecipientId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Text = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ItemIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SendMail RecipientId={RecipientId} Text={Text} ItemIds={ItemIds}>";
	}
}
