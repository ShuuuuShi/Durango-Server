using MsgPack;

namespace Messages;

public struct Recipes
{
	public const uint TypeCode = 120u;

	public string[] Ids;

	public string[] LikedRecipeIds;

	public string[] NewRecipeIds;

	public static void Pack(Packer packer, Recipes val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(120u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.Ids == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Ids.Length);
			for (int i = 0; i < val.Ids.Length; i++)
			{
				if (val.Ids[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Ids[i]);
				}
			}
		}
		if (val.LikedRecipeIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.LikedRecipeIds.Length);
			for (int j = 0; j < val.LikedRecipeIds.Length; j++)
			{
				if (val.LikedRecipeIds[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.LikedRecipeIds[j]);
				}
			}
		}
		if (val.NewRecipeIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.NewRecipeIds.Length);
		for (int k = 0; k < val.NewRecipeIds.Length; k++)
		{
			if (val.NewRecipeIds[k] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.NewRecipeIds[k]);
			}
		}
	}

	public static Recipes Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Recipes result = default(Recipes);
		result.Ids = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Ids[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.LikedRecipeIds = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.LikedRecipeIds[j] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.NewRecipeIds = new string[num3];
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			result.NewRecipeIds[k] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Recipes Ids={Ids} LikedRecipeIds={LikedRecipeIds} NewRecipeIds={NewRecipeIds}>";
	}
}
