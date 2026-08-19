using System.Collections.Generic;
using ItemSystem;
using JetBrains.Annotations;
using MotionInfo;

public class MotionMap
{
	private static MotionMap _instance;

	[NotNull]
	private readonly Gathering _gatheringMotions;

	[NotNull]
	private readonly Craft _craftMotions;

	[NotNull]
	private readonly Build _buildMotions;

	private MotionMap()
	{
		_gatheringMotions = KUtility.ParseJsonFile<Gathering>("MotionInfos/gathering_motion_map");
		_craftMotions = KUtility.ParseJsonFile<Craft>("MotionInfos/craft_motion_map");
		_buildMotions = KUtility.ParseJsonFile<Build>("MotionInfos/build_motion_map");
	}

	[NotNull]
	public static MotionMap Instance()
	{
		return (_instance != null) ? _instance : (_instance = new MotionMap());
	}

	public string GetGatheringMotion(string toolTag, string resource, BiomeSpriteInfo info, string gatherSize)
	{
		return GetGatheringMotion(toolTag, resource, 0, info, gatherSize);
	}

	public string GetGatheringMotion(string toolTag, string resource, int animalType, string gatherSize)
	{
		return GetGatheringMotion(toolTag, resource, animalType, null, gatherSize);
	}

	private string GetGatheringMotion(string toolTag, string resource, int animalType, BiomeSpriteInfo info, string gatherSize)
	{
		int num = -1;
		int num2 = 0;
		int i = 0;
		for (int count = _gatheringMotions.Count; i < count; i++)
		{
			int num3 = _gatheringMotions[i].Valid(toolTag, resource, animalType, info, gatherSize);
			if (num3 > num2)
			{
				num = i;
				num2 = num3;
			}
		}
		string result = ((num != -1) ? _gatheringMotions[num].motion : _gatheringMotions.defaultMotion);
		if (animalType >= 2000 && animalType <= 3000)
		{
			result = "Barehand_Butcher";
		}
		return result;
	}

	public void GetCraftMotion(string recipeId, string workbench, List<TagData> tags, out string motion, out string equip)
	{
		if (!_craftMotions.TryGetValue(recipeId, workbench, tags, out motion, out equip))
		{
			motion = _craftMotions.defaultMotion;
			equip = null;
		}
	}

	public void GetBuildMotion(string blueprintId, List<TagData> tags, out string motion, out string equip)
	{
		if (!_buildMotions.TryGetValue(blueprintId, tags, out motion, out equip))
		{
			motion = _buildMotions.defaultMotion;
			equip = null;
		}
	}
}
