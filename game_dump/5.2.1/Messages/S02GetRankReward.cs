using MsgPack;
using Shared.Rank;

namespace Messages;

public struct S02GetRankReward
{
	public const uint TypeCode = 222230u;

	public Category Category;

	public string Revision;

	public static void Pack(Packer packer, S02GetRankReward val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(222230u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Category);
		if (val.Revision == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Revision);
		}
	}

	public static S02GetRankReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		S02GetRankReward result = default(S02GetRankReward);
		if (num < 10 || 78 < num)
		{
			result.Category = Category.Invalid;
		}
		else
		{
			result.Category = (Category)num;
		}
		unpacker.Read();
		result.Revision = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<S02GetRankReward Category={Category} Revision={Revision}>";
	}
}
