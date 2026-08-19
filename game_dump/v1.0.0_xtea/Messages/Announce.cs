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
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		Announce result = default(Announce);
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData)).AsByte();
		return result;
	}

	public override string ToString()
	{
		return $"<Announce Text={Text} Level={Level}>";
	}
}
