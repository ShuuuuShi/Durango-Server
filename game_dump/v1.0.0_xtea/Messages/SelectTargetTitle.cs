using MsgPack;

namespace Messages;

public struct SelectTargetTitle
{
	public const uint TypeCode = 3503u;

	public string TitleId;

	public static void Pack(Packer packer, SelectTargetTitle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3503u);
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

	public static SelectTargetTitle Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SelectTargetTitle result = default(SelectTargetTitle);
		result.TitleId = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SelectTargetTitle TitleId={TitleId}>";
	}
}
