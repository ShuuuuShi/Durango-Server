using MsgPack;

namespace Messages;

public struct GetQuestScoreInfos
{
	public const uint TypeCode = 237920u;

	public string Category;

	public static void Pack(Packer packer, GetQuestScoreInfos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(237920u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
	}

	public static GetQuestScoreInfos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetQuestScoreInfos result = default(GetQuestScoreInfos);
		result.Category = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<GetQuestScoreInfos Category=" + Category + ">";
	}
}
