using MsgPack;

namespace Messages;

public struct RecommendedRecipes
{
	public const uint TypeCode = 3646u;

	public string[] RecipeIds;

	public static void Pack(Packer packer, RecommendedRecipes val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3646u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.RecipeIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.RecipeIds.Length);
		for (int i = 0; i < val.RecipeIds.Length; i++)
		{
			if (val.RecipeIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.RecipeIds[i]);
			}
		}
	}

	public static RecommendedRecipes Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		RecommendedRecipes result = default(RecommendedRecipes);
		result.RecipeIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string[] recipeIds = result.RecipeIds;
			int num2 = i;
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			recipeIds[num2] = ((MessagePackObject)(ref lastReadData2)).AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("<RecommendedRecipes RecipeIds={0}>", RecipeIds);
	}
}
