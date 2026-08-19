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
		unpacker.Read();
		ActionInfo result = default(ActionInfo);
		result.ActionLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.PotentialLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 15 < num)
		{
			result.RelatedCategory = Category.Invalid;
		}
		else
		{
			result.RelatedCategory = (Category)num;
		}
		unpacker.Read();
		result.SuccessRatio = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		if (num2 < 0 || 322 < num2)
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
