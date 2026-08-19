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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		CraftGoal result = default(CraftGoal);
		result.RecipeId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		if (num < 0 || 13 < num)
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
