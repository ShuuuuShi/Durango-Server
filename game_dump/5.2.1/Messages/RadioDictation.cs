using MsgPack;

namespace Messages;

public struct RadioDictation
{
	public const uint TypeCode = 2602u;

	public string Text;

	public static void Pack(Packer packer, RadioDictation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2602u);
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

	public static RadioDictation Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RadioDictation result = default(RadioDictation);
		result.Text = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RadioDictation Text=" + Text + ">";
	}
}
