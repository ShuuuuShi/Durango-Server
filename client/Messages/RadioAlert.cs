using MsgPack;

namespace Messages;

public struct RadioAlert
{
	public const uint TypeCode = 2610u;

	public string Text;

	public static void Pack(Packer packer, RadioAlert val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2610u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackString(val.Text);
	}

	public static RadioAlert Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RadioAlert result = default(RadioAlert);
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<RadioAlert Text={Text}>";
	}
}
