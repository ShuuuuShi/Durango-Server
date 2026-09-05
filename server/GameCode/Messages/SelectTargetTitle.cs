using MsgPack;

namespace Messages;

public struct SelectTargetTitle
{
	public const uint TypeCode = 3900u;

	public string TitleId;

	public static void Pack(Packer packer, SelectTargetTitle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3900u);
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
		unpacker.Read();
		SelectTargetTitle result = default(SelectTargetTitle);
		result.TitleId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SelectTargetTitle TitleId={TitleId}>";
	}
}
