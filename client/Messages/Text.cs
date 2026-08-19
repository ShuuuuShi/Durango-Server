using MsgPack;

namespace Messages;

public struct Text
{
	public const uint TypeCode = 222u;

	public string _Text;

	public static void Pack(Packer packer, Text val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(222u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackString(val._Text);
	}

	public static Text Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Text result = default(Text);
		result._Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Text _Text={_Text}>";
	}
}
