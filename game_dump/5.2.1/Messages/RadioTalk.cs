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
		unpacker.Read();
		RadioTalk result = default(RadioTalk);
		result.Text = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RadioTalk Text=" + Text + ">";
	}
}
