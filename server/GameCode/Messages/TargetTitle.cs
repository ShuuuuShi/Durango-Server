using MsgPack;

namespace Messages;

public struct TargetTitle
{
	public const uint TypeCode = 3907u;

	public string TitleId;

	public static void Pack(Packer packer, TargetTitle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3907u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.TitleId == null)
		{
			packer.PackNull();
		}
		else if (val.TitleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TitleId);
		}
	}

	public static TargetTitle Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TargetTitle result = default(TargetTitle);
		if (unpacker.LastReadData.IsNil)
		{
			result.TitleId = null;
		}
		else
		{
			string titleId = unpacker.LastReadData.AsString();
			result.TitleId = titleId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TargetTitle TitleId={TitleId}>";
	}
}
