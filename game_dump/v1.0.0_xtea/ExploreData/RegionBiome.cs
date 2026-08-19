using System.Text;
using Shared.Survival;
using TerrainData;
using Yaml;
using Yaml.Util;

namespace ExploreData;

public struct RegionBiome
{
	public Biome Biome;

	public Shared.Survival.FatigueCategory[] Categories;

	public void ToText(ref StringBuilder str, bool containCategory)
	{
		str.Append(LocalizeUtil.Get(Biome));
		if (!containCategory)
		{
			return;
		}
		int num = 0;
		int i = 0;
		for (int num2 = ((Categories != null) ? Categories.Length : 0); i < num2; i++)
		{
			Yaml.FatigueCategory fatigueCategory = SingletonDict<Shared.Survival.FatigueCategory, Yaml.FatigueCategory>.Get(Categories[i]);
			if (fatigueCategory != null)
			{
				str.Append((num != 0) ? ", " : "[");
				str.Append(fatigueCategory.name);
				num++;
			}
		}
		if (num > 0)
		{
			str.Append("]");
		}
	}
}
