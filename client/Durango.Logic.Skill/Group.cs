using System.Collections.Generic;

namespace Durango.Logic.Skill;

public class Group
{
	public string Name;

	public List<Bundle> Skills;

	public Bundle Skill;

	public int RenderPrioirty => (Skill != null) ? Skill.RenderPriority : int.MaxValue;

	public bool Contains(string bundleId)
	{
		int i = 0;
		for (int count = Skills.Count; i < count; i++)
		{
			if (Skills[i].Id == bundleId)
			{
				return true;
			}
		}
		return false;
	}

	public int GetLearnableCount()
	{
		if (Skill != null)
		{
			return Skill.GetLearnableCount();
		}
		int num = 0;
		int i = 0;
		for (int size = KUtility.GetSize(Skills); i < size; i++)
		{
			num += Skills[i].GetLearnableCount();
		}
		return num;
	}

	public int HighestLevel()
	{
		int num = 0;
		int i = 0;
		for (int num2 = ((Skill != null) ? 1 : KUtility.GetSize(Skills)); i < num2; i++)
		{
			Bundle bundle = ((Skill != null) ? Skill : Skills[i]);
			int num3 = bundle.HighestLevel();
			if (num3 > num)
			{
				num = num3;
			}
		}
		return num;
	}

	public int NearestNextAvailableCategoryLevel()
	{
		int num = 1000000;
		int i = 0;
		for (int num2 = ((Skill != null) ? 1 : KUtility.GetSize(Skills)); i < num2; i++)
		{
			Bundle bundle = ((Skill != null) ? Skill : Skills[i]);
			int num3 = bundle.NearestNextAvailableCategoryLevel();
			if (num3 < num)
			{
				num = num3;
			}
		}
		return num;
	}

	public bool HasNew()
	{
		int i = 0;
		for (int num = ((Skill != null) ? 1 : KUtility.GetSize(Skills)); i < num; i++)
		{
			Bundle bundle = ((Skill != null) ? Skill : Skills[i]);
			if (bundle.HasNew())
			{
				return true;
			}
		}
		return false;
	}

	public void SetRead()
	{
		int i = 0;
		for (int num = ((Skill != null) ? 1 : KUtility.GetSize(Skills)); i < num; i++)
		{
			Bundle bundle = ((Skill != null) ? Skill : Skills[i]);
			int j = 0;
			for (int num2 = KUtility.GetSize(bundle.Sub) + 1; j < num2; j++)
			{
				Skill skill = ((j != 0) ? bundle.Sub[j - 1] : bundle.Base);
				if (skill != null)
				{
					for (int k = 0; k < skill.MaxLevel; k++)
					{
						int level = k + 1;
						skill.Get(level).IsNew = false;
					}
				}
			}
		}
	}

	public void Sort()
	{
		if (Skills != null)
		{
			Skills.Sort(Comparison);
		}
	}

	private static int Comparison(Bundle s1, Bundle s2)
	{
		int num = s1.RenderPriority - s2.RenderPriority;
		if (num == 0 && s1.Base != null && s2.Base != null)
		{
			num = s1.Base.Get(1).CategoryLevel - s2.Base.Get(1).CategoryLevel;
		}
		return num;
	}
}
