using MsgPack;

namespace Messages;

public struct SearchedWarpholes
{
	public const uint TypeCode = 905u;

	public SearchResult[] Results;

	public double SearchedAt;

	public static void Pack(Packer packer, SearchedWarpholes val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(905u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Results == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Results.Length);
			for (int i = 0; i < val.Results.Length; i++)
			{
				SearchResult.Pack(packer, val.Results[i]);
			}
		}
		packer.Pack(val.SearchedAt);
	}

	public static SearchedWarpholes Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		SearchedWarpholes result = default(SearchedWarpholes);
		result.Results = new SearchResult[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref SearchResult reference = ref result.Results[i];
			reference = SearchResult.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.SearchedAt = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<SearchedWarpholes Results={Results} SearchedAt={SearchedAt}>";
	}
}
