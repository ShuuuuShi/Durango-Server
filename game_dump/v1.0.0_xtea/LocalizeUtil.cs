using System;
using L10N;
using Shared.Ability;
using Yaml;
using Yaml.Util;

public static class LocalizeUtil
{
	public static string Get(Enum e)
	{
		return LocalizeSystem.Get(GetKey(e));
	}

	public static string GetKey(Enum e)
	{
		return $"#{e.GetType().FullName}.{e}";
	}

	public static string FormatLevel(int lv)
	{
		return T._("{0:lv:}", lv);
	}

	public static string ActionTagName(string tag)
	{
		return tag switch
		{
			"default" => T._("일반 공격"), 
			"barehand" => T._("맨손"), 
			"melee" => T._("근접"), 
			"onehand" => T._("한손"), 
			"twohand" => T._("양손"), 
			"ranged" => T._("투사"), 
			"attack" => T._("공격"), 
			"defense" => T._("방어"), 
			"sword" => T._("검"), 
			"axe" => T._("도끼"), 
			"blunt" => T._("둔기"), 
			"lance" => T._("장창"), 
			"sling" => T._("슬링"), 
			"bow" => T._("활"), 
			"crossbow" => T._("석궁"), 
			"dodge" => T._("구르기"), 
			"guard" => T._("무기 막기"), 
			"flurry" => T._("난타"), 
			"sweeping" => T._("휩쓸기"), 
			"smash" => T._("강타"), 
			"strike" => T._("일격"), 
			"stab" => T._("찌르기"), 
			"dash" => T._("돌진"), 
			"charge" => T._("돌진"), 
			"hard_throwing" => T._("역투"), 
			"aimedshot" => T._("조준사격"), 
			"quickshot" => T._("속사"), 
			"kick" => T._("발차기"), 
			"combination" => T._("연타"), 
			"tackle" => T._("몸통박치기"), 
			_ => tag, 
		};
	}

	public static string ModifierName(string modifier)
	{
		SkillModifier skillModifier = SingletonDict<string, SkillModifier>.Get(modifier);
		if (skillModifier != null && !Gettext.IsEmpty(skillModifier.name))
		{
			return skillModifier.name;
		}
		return modifier;
	}

	public static string ModifierIncreaseText(string modifier, float value)
	{
		SkillModifier skillModifier = SingletonDict<string, SkillModifier>.Get(modifier);
		if (skillModifier != null)
		{
			string text = ((skillModifier.increase_type != IncreaseType.Ratio) ? "0.#" : "0.##%");
			if (skillModifier.apply_type == ApplyType.Replace)
			{
				return $"{skillModifier.name} {value.ToString(text)}";
			}
			text = Math.Abs(value).ToString(text);
			if ((value > 0f && !skillModifier.inverse) || (value < 0f && skillModifier.inverse))
			{
				return T._("{0} {1} 증가", skillModifier.name, text);
			}
			if ((value < 0f && !skillModifier.inverse) || (value > 0f && skillModifier.inverse))
			{
				return T._("{0} {1} 감소", skillModifier.name, text);
			}
		}
		return null;
	}
}
