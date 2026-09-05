using MsgPack;

namespace Messages;

public struct RequestQuestReward
{
	public const uint TypeCode = 237923u;

	public string QuestId;

	public static void Pack(Packer packer, RequestQuestReward val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(237923u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.QuestId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.QuestId);
		}
	}

	public static RequestQuestReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RequestQuestReward result = default(RequestQuestReward);
		result.QuestId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RequestQuestReward QuestId={QuestId}>";
	}
}
