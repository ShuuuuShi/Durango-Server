using MsgPack;

namespace Messages;

public struct SearchProducts
{
	public const uint TypeCode = 5016u;

	public string ItemName;

	public string PrototypeId;

	public string[][] NestedTags;

	public string Category;

	public string[] SubCategories;

	public PriceRangePredicate? Price;

	public RangePredicate? Level;

	public SortCondition Sort;

	public int Skip;

	public static void Pack(Packer packer, SearchProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(10);
			packer.Pack(5016u);
		}
		else
		{
			packer.PackArrayHeader(9);
		}
		if (val.ItemName == null)
		{
			packer.PackNull();
		}
		else if (val.ItemName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemName);
		}
		if (val.PrototypeId == null)
		{
			packer.PackNull();
		}
		else if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		if (val.NestedTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.NestedTags.Length);
			for (int i = 0; i < val.NestedTags.Length; i++)
			{
				if (val.NestedTags[i] == null)
				{
					packer.PackArrayHeader(0);
					continue;
				}
				packer.PackArrayHeader(val.NestedTags[i].Length);
				for (int j = 0; j < val.NestedTags[i].Length; j++)
				{
					if (val.NestedTags[i][j] == null)
					{
						packer.PackString(string.Empty);
					}
					else
					{
						packer.PackString(val.NestedTags[i][j]);
					}
				}
			}
		}
		if (val.Category == null)
		{
			packer.PackNull();
		}
		else if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
		if (val.SubCategories == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.SubCategories.Length);
			for (int k = 0; k < val.SubCategories.Length; k++)
			{
				if (val.SubCategories[k] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.SubCategories[k]);
				}
			}
		}
		if (!val.Price.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			PriceRangePredicate.Pack(packer, val.Price.Value);
		}
		if (!val.Level.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RangePredicate.Pack(packer, val.Level.Value);
		}
		SortCondition.Pack(packer, val.Sort);
		packer.Pack(val.Skip);
	}

	public static SearchProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SearchProducts result = default(SearchProducts);
		if (unpacker.LastReadData.IsNil)
		{
			result.ItemName = null;
		}
		else
		{
			string itemName = unpacker.LastReadData.AsString();
			result.ItemName = itemName;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PrototypeId = null;
		}
		else
		{
			string prototypeId = unpacker.LastReadData.AsString();
			result.PrototypeId = prototypeId;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.NestedTags = new string[num][];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			result.NestedTags[i] = new string[num2];
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				result.NestedTags[i][j] = unpacker.LastReadData.AsString();
			}
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Category = null;
		}
		else
		{
			string category = unpacker.LastReadData.AsString();
			result.Category = category;
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.SubCategories = new string[num3];
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			result.SubCategories[k] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Price = null;
		}
		else
		{
			PriceRangePredicate value = PriceRangePredicate.Unpack(unpacker);
			result.Price = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Level = null;
		}
		else
		{
			RangePredicate value2 = RangePredicate.Unpack(unpacker);
			result.Level = value2;
		}
		unpacker.Read();
		result.Sort = SortCondition.Unpack(unpacker);
		unpacker.Read();
		result.Skip = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SearchProducts ItemName={ItemName} PrototypeId={PrototypeId} NestedTags={NestedTags} Category={Category} SubCategories={SubCategories} Price={Price} Level={Level} Sort={Sort} Skip={Skip}>";
	}
}
