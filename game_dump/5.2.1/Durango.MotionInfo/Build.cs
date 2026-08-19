using System.Collections.Generic;
using Durango.Logic.Item;

namespace Durango.MotionInfo;

public class Build
{
	public DictionaryIgnoreCase<List<BuildMotion>> motions;

	public string defaultMotion;

	public int Count
	{
		get
		{
			if (motions == null)
			{
				return 0;
			}
			return motions.Count;
		}
	}

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

	public bool TryGetValue(string blueprintID, List<TagData> materialTags, out string motion, out string equip)
	{
		if (motions == null)
		{
			motion = null;
			equip = null;
			return false;
		}
		motion = defaultMotion;
		equip = null;
		BuildMotion buildMotion = null;
		if (string.IsNullOrEmpty(blueprintID))
		{
			return false;
		}
		if (!motions.TryGetValueWithSubStringKey(blueprintID, out var value))
		{
			return false;
		}
		int num = value?.Count ?? 0;
		int num2 = -1;
		for (int i = 0; i < num; i++)
		{
			int num3 = CalcTagMatchesScore(materialTags, value[i].Tags);
			if (num3 > num2)
			{
				buildMotion = value[i];
				num2 = num3;
			}
		}
		if (buildMotion == null)
		{
			motion = defaultMotion;
			equip = null;
			return false;
		}
		motion = ((buildMotion.Motion.Length < 1) ? defaultMotion : buildMotion.Motion[0]);
		equip = ((buildMotion.Motion.Length < 2) ? null : buildMotion.Motion[1]);
		return true;
	}
}
