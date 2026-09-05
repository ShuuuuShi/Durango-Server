using MsgPack;

namespace Messages;

public struct Info
{
	public const uint TypeCode = 1023u;

	public string Text;

	public static void Pack(Packer packer, Info val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1023u);
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

	public static Info Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Info result = default(Info);
		result.Text = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Info Text={Text}>";
	}
}
