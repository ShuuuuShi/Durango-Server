using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Terrain;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;

namespace Durango.MotionInfo;

public class MotionMap
{
	private static MotionMap _instance;

	private readonly Gathering _gatheringMotions;

	private readonly Craft _craftMotions;

	private readonly Build _buildMotions;

	private readonly Ride _rideMotions;

	private readonly InteractionMotions _restMotions;

	private readonly InteractionMotions _useMotions;

	private MotionMap()
	{
		_gatheringMotions = Json.ReadFromFile<Gathering>("MotionInfos/gathering_motion_map");
		_craftMotions = Json.ReadFromFile<Craft>("MotionInfos/craft_motion_map");
		_buildMotions = Json.ReadFromFile<Build>("MotionInfos/build_motion_map");
		_rideMotions = Json.ReadFromFile<Ride>("MotionInfos/ride_motion_map");
		_restMotions = Json.ReadFromFile<InteractionMotions>("MotionInfos/rest_motion_map");
		_useMotions = Json.ReadFromFile<InteractionMotions>("MotionInfos/use_motion_map");
	}

	[NotNull]
	public static MotionMap Instance()
	{
		return (_instance != null) ? _instance : (_instance = new MotionMap());
	}

	public string GetGatheringMotion(string toolTag, string resource, ushort naturalType, BiomeSpriteInfo info, string gatherSize)
	{
		return GetGatheringMotion(toolTag, resource, 0, info, gatherSize, naturalType);
	}

	public string GetGatheringMotion(string toolTag, string resource, int animalType, string gatherSize)
	{
		return GetGatheringMotion(toolTag, resource, animalType, null, gatherSize, 0);
	}

	private string GetGatheringMotion(string toolTag, string resource, int animalType, BiomeSpriteInfo info, string gatherSize, ushort naturalType)
	{
		int num = -1;
		int num2 = 0;
		int i = 0;
		for (int count = _gatheringMotions.Count; i < count; i++)
		{
			int num3 = _gatheringMotions[i].Valid(toolTag, resource, animalType, info, gatherSize, naturalType);
			if (num3 > num2)
			{
				num = i;
				num2 = num3;
			}
		}
		return (num != -1) ? _gatheringMotions[num].motion : _gatheringMotions.defaultMotion;
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

	public string GetInteractionMotion(Interaction interaction, string blueprintId, string attachType)
	{
		InteractionMotions interactionMotions = null;
		switch (interaction)
		{
		case Interaction.Rest:
		case Interaction.Wash:
			interactionMotions = _restMotions;
			break;
		case Interaction.GetStatusEffect:
			interactionMotions = _useMotions;
			break;
		}
		return interactionMotions?.Get(blueprintId, attachType);
	}

	public RideMotionSet GetRideMotion(string vehicleName)
	{
		return _rideMotions.Get(vehicleName);
	}
}
