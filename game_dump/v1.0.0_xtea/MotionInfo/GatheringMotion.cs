using NetworkEnums;

namespace MotionInfo;

public struct GatheringMotion
{
	public string[] tool;

	public string[] resource;

	public EntityType[] animal;

	public string[] size;

	public string[] sprite;

	public int[] type;

	public string motion;

	public int Valid(string toolTag, string targetResource, int animalType, BiomeSpriteInfo info, string gatherSize)
	{
		int num = 0;
		if (tool != null)
		{
			num = ((tool.Length != 0 && !tool.ContainsIgnoreCase(toolTag)) ? (num - 1) : (num + 1));
		}
		if (resource != null)
		{
			num = ((resource.Length != 0 && !resource.ContainsIgnoreCase(targetResource)) ? (num - 1) : (num + 1));
		}
		if (animal != null && animalType >= 2000 && animalType <= 3000)
		{
			num = ((animal.Length != 0) ? (num - 1) : (num + 1));
		}
		if (size != null)
		{
			num = ((size.Length != 0 && !size.ContainsIgnoreCase(gatherSize)) ? (num - 1) : (num + 1));
		}
		if (info != null)
		{
			if (sprite != null)
			{
				num = ((sprite.Length != 0 && !sprite.Any(info.HasSprite)) ? (num - 1) : (num + 1));
			}
			if (type != null)
			{
				num = ((type.Length != 0 && !type.Contains(info.EntityType)) ? (num - 1) : (num + 1));
			}
		}
		return num;
	}
}
