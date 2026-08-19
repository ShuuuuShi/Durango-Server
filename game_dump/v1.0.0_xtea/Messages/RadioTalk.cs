using MsgPack;

namespace Messages;

public struct RadioTalk
{
	public const uint TypeCode = 2601u;

	public string Text;

	public static void Pack(Packer packer, RadioTalk val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2601u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Text == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Text);
		}
	}

	public static RadioTalk Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RadioTalk result = default(RadioTalk);
		result.Text = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RadioTalk Text={Text}>";
	}
}
