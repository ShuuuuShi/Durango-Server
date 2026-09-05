using MsgPack;

namespace Messages;

public struct RadioNotice
{
	public const uint TypeCode = 2604u;

	public string Text;

	public static void Pack(Packer packer, RadioNotice val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2604u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackString(val.Text);
	}

	public static RadioNotice Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RadioNotice result = default(RadioNotice);
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<RadioNotice Text={Text}>";
	}
}
