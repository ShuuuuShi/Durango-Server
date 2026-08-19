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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		BuildGoal result = default(BuildGoal);
		result.BlueprintId = ((MessagePackObject)(ref lastReadData)).AsString();
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
		return $"<BuildGoal BlueprintId={BlueprintId} Category={Category}>";
	}
}
