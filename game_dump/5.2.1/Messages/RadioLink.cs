using MsgPack;

namespace Messages;

public struct RadioLink
{
	public const uint TypeCode = 2611u;

	public string Text;

	public string Link;

	public static void Pack(Packer packer, RadioLink val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2611u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Text == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Text);
		}
		if (val.Link == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Link);
		}
	}

	public static RadioLink Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RadioLink result = default(RadioLink);
		result.Text = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Link = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RadioLink Text=" + Text + " Link=" + Link + ">";
	}
}
