using MsgPack;

namespace Messages;

public struct RadioText
{
	public const uint TypeCode = 2603u;

	public string Text;

	public static void Pack(Packer packer, RadioText val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2603u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackString(val.Text);
	}

	public static RadioText Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RadioText result = default(RadioText);
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<RadioText Text={Text}>";
	}
}
