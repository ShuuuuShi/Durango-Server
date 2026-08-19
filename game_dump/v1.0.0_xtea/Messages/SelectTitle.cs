using MsgPack;

namespace Messages;

public struct SelectTitle
{
	public const uint TypeCode = 2046u;

	public string TitleId;

	public static void Pack(Packer packer, SelectTitle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2046u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.TitleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TitleId);
		}
	}

	public static SelectTitle Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SelectTitle result = default(SelectTitle);
		result.TitleId = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SelectTitle TitleId={TitleId}>";
	}
}
