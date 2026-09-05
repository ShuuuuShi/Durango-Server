using MsgPack;
using Shared.Skill;

namespace Messages;

public struct BuildGoal
{
	public const uint TypeCode = 3511u;

	public string BlueprintId;

	public Category Category;

	public static void Pack(Packer packer, BuildGoal val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3511u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.BlueprintId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BlueprintId);
		}
		packer.Pack((int)val.Category);
	}

	public static BuildGoal Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BuildGoal result = default(BuildGoal);
		result.BlueprintId = unpacker.LastReadData.AsString();
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
		return $"<BuildGoal BlueprintId={BlueprintId} Category={Category}>";
	}
}
