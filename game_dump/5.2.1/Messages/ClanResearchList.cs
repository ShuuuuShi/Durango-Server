using MsgPack;

namespace Messages;

public struct ClanResearchList
{
	public const uint TypeCode = 5987342u;

	public ClanResearch[] ResearchList;

	public static void Pack(Packer packer, ClanResearchList val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5987342u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ResearchList == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ResearchList.Length);
		for (int i = 0; i < val.ResearchList.Length; i++)
		{
			ClanResearch.Pack(packer, val.ResearchList[i]);
		}
	}

	public static ClanResearchList Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ClanResearchList result = default(ClanResearchList);
		result.ResearchList = new ClanResearch[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref ClanResearch reference = ref result.ResearchList[i];
			reference = ClanResearch.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ClanResearchList ResearchList={ResearchList}>";
	}
}
