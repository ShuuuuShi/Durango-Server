using MsgPack;
using Shared.Skill;

namespace Messages;

public struct CraftGoal
{
	public const uint TypeCode = 3510u;

	public string RecipeId;

	public Category Category;

	public static void Pack(Packer packer, CraftGoal val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3510u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.RecipeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RecipeId);
		}
		packer.Pack((int)val.Category);
	}

	public static CraftGoal Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CraftGoal result = default(CraftGoal);
		result.RecipeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 15 < num)
		{
			result.Category = Category.Invalid;
		}
		else
		{
			result.Category = (Category)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CraftGoal RecipeId={RecipeId} Category={Category}>";
	}
}
