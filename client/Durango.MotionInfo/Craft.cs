using System.Collections.Generic;
using Durango.Logic.Item;

namespace Durango.MotionInfo;

public class Craft
{
	public DictionaryIgnoreCase<DictionaryIgnoreCase<List<CraftMotion>>> motions;

	public string defaultMotion;

	public int Count => (motions != null) ? motions.Count : 0;

	private static int CalcTagMatchesScore(List<TagData> tagData, string[] condTags)
	{
		if (tagData == null)
		{
			return 0;
		}
		if (condTags == null || condTags.Length == 0)
		{
			return 0;
		}
		int num = 0;
		int count = tagData.Count;
		int num2 = condTags.Length;
		for (int i = 0; i < num2; i++)
		{
			string text = condTags[i];
			num--;
			for (int j = 0; j < count; j++)
			{
				if (text == tagData[j].Id)
				{
					num += 2;
					break;
				}
			}
		}
		return num;
	}

	public bool TryGetValue(string recipeID, string workbench, List<TagData> materialTags, out string motion, out string equip)
	{
		if (motions == null)
		{
			motion = null;
			equip = null;
			return false;
		}
		if (string.IsNullOrEmpty(workbench))
		{
			workbench = string.Empty;
		}
		if (motions.TryGetValueWithSubStringKey(workbench, out var value))
		{
			CraftMotion craftMotion = null;
			List<CraftMotion> value2;
			if (string.IsNullOrEmpty(recipeID))
			{
				value.TryGetValue("default", out value2);
				craftMotion = value2?[0];
			}
			else
			{
				if (!value.TryGetValueWithSubStringKey(recipeID, out value2))
				{
					value.TryGetValue("default", out value2);
				}
				int num = value2?.Count ?? 0;
				int num2 = -1;
				for (int i = 0; i < num; i++)
				{
					int num3 = CalcTagMatchesScore(materialTags, value2[i].Tags);
					if (num3 > num2)
					{
						craftMotion = value2[i];
						num2 = num3;
					}
				}
			}
			if (craftMotion == null)
			{
				motion = null;
				equip = null;
				return false;
			}
			motion = ((craftMotion.Motion.Length < 1) ? defaultMotion : craftMotion.Motion[0]);
			equip = ((craftMotion.Motion.Length < 2) ? null : craftMotion.Motion[1]);
			return true;
		}
		motion = null;
		equip = null;
		return false;
	}
}
