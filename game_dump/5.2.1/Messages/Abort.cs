using MsgPack;

namespace Messages;

public struct Abort
{
	public const uint TypeCode = 1024u;

	public string Text;

	public static void Pack(Packer packer, Abort val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1024u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackString(val.Text);
	}

	public static Abort Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Abort result = default(Abort);
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return "<Abort Text=" + Text + ">";
	}
}
