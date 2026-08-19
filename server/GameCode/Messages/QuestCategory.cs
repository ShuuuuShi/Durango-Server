using MsgPack;
using Shared.Faction;

namespace Messages;

public struct QuestCategory
{
	public string Category;

	public string Name;

	public FactionType? Faction;

	public string Season;

	public int UnreceivedCount;

	public static void Pack(Packer packer, QuestCategory val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
		packer.PackString(val.Name);
		if (!val.Faction.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack((int)val.Faction.Value);
		}
		if (val.Season == null)
		{
			packer.PackNull();
		}
		else if (val.Season == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Season);
		}
		packer.Pack(val.UnreceivedCount);
	}

	public static QuestCategory Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		QuestCategory result = default(QuestCategory);
		result.Category = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Faction = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			FactionType value = ((num >= 0 && 101 >= num) ? ((FactionType)num) : FactionType.Invalid);
			result.Faction = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Season = null;
		}
		else
		{
			string season = unpacker.LastReadData.AsString();
			result.Season = season;
		}
		unpacker.Read();
		result.UnreceivedCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<QuestCategory Category={Category} Name={Name} Faction={Faction} Season={Season} UnreceivedCount={UnreceivedCount}>";
	}
}
