using MsgPack;

namespace Messages;

public struct Announce
{
	public const uint TypeCode = 322u;

	public string Text;

	public byte Level;

	public static void Pack(Packer packer, Announce val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(322u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.PackString(val.Text);
		packer.Pack(val.Level);
	}

	public static Announce Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Announce result = default(Announce);
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsByte();
		return result;
	}

	public override string ToString()
	{
		return $"<Announce Text={Text} Level={Level}>";
	}
}
