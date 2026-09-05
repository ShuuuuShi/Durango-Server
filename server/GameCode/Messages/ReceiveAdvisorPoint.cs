using MsgPack;
using Shared.Skill;

namespace Messages;

public struct ReceiveAdvisorPoint
{
	public const uint TypeCode = 3905u;

	public Skill[] Skills;

	public Category[] SkillCategories;

	public static void Pack(Packer packer, ReceiveAdvisorPoint val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3905u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Skills == null)
		{
			packer.PackNull();
		}
		else if (val.Skills == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Skills.Length);
			for (int i = 0; i < val.Skills.Length; i++)
			{
				Skill.Pack(packer, val.Skills[i]);
			}
		}
		if (val.SkillCategories == null)
		{
			packer.PackNull();
			return;
		}
		if (val.SkillCategories == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.SkillCategories.Length);
		for (int j = 0; j < val.SkillCategories.Length; j++)
		{
			packer.Pack((int)val.SkillCategories[j]);
		}
	}

	public static ReceiveAdvisorPoint Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReceiveAdvisorPoint result = default(ReceiveAdvisorPoint);
		if (unpacker.LastReadData.IsNil)
		{
			result.Skills = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			Skill[] array = new Skill[num];
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				ref Skill reference = ref array[i];
				reference = Skill.Unpack(unpacker);
			}
			result.Skills = array;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SkillCategories = null;
		}
		else
		{
			int num2 = unpacker.LastReadData.AsInt32();
			Category[] array2 = new Category[num2];
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				int num3 = unpacker.LastReadData.AsInt32();
				if (num3 < 0 || 15 < num3)
				{
					array2[j] = Category.Invalid;
				}
				else
				{
					array2[j] = (Category)num3;
				}
			}
			result.SkillCategories = array2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ReceiveAdvisorPoint Skills={Skills} SkillCategories={SkillCategories}>";
	}
}
