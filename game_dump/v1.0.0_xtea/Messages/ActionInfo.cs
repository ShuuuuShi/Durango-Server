using MsgPack;
using Shared.Ability;
using Shared.Skill;

namespace Messages;

public struct ActionInfo
{
	public int ActionLevel;

	public int PotentialLevel;

	public Category RelatedCategory;

	public float SuccessRatio;

	public Derived RelatedAbility;

	public static void Pack(Packer packer, ActionInfo val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		packer.Pack(val.ActionLevel);
		packer.Pack(val.PotentialLevel);
		packer.Pack((int)val.RelatedCategory);
		packer.Pack(val.SuccessRatio);
		packer.Pack((int)val.RelatedAbility);
	}

	public static ActionInfo Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ActionInfo result = default(ActionInfo);
		result.ActionLevel = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.PotentialLevel = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		if (num < 0 || 13 < num)
		{
			result.RelatedCategory = Category.Invalid;
		}
		else
		{
			result.RelatedCategory = (Category)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.SuccessRatio = ((MessagePackObject)(ref lastReadData4)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		if (num2 < 0 || 301 < num2)
		{
			result.RelatedAbility = Derived.Invalid;
		}
		else
		{
			result.RelatedAbility = (Derived)num2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ActionInfo ActionLevel={ActionLevel} PotentialLevel={PotentialLevel} RelatedCategory={RelatedCategory} SuccessRatio={SuccessRatio} RelatedAbility={RelatedAbility}>";
	}
}
