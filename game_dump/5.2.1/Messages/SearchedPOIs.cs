using MsgPack;

namespace Messages;

public struct SearchedPOIs
{
	public const uint TypeCode = 905u;

	public SearchResult[] Results;

	public double SearchedAt;

	public static void Pack(Packer packer, SearchedPOIs val, bool hint = false)
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

	public static SearchedPOIs Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SearchedPOIs result = default(SearchedPOIs);
		result.Results = new SearchResult[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref SearchResult reference = ref result.Results[i];
			reference = SearchResult.Unpack(unpacker);
		}
		unpacker.Read();
		result.SearchedAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<SearchedPOIs Results={Results} SearchedAt={SearchedAt}>";
	}
}
