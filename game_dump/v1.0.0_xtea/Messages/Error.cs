using MsgPack;

namespace Messages;

public struct Error
{
	public const uint TypeCode = 1022u;

	public string TypeName;

	public string Text;

	public static void Pack(Packer packer, Error val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(1022u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.TypeName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TypeName);
		}
		packer.PackString(val.Text);
	}

	public static Error Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Error result = default(Error);
		result.TypeName = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Error TypeName={TypeName} Text={Text}>";
	}
}
