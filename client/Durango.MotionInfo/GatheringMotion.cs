using System.Linq;
using Durango.Network;
using Durango.Terrain;
using Durango.Utils.Extensions;

namespace Durango.MotionInfo;

public struct GatheringMotion
{
	public string[] tool;

	public string[] resource;

	public EntityType[] animal;

	public string[] size;

	public string[] sprite;

	public int[] type;

	public string motion;

	public int Valid(string toolTag, string targetResource, int animalType, BiomeSpriteInfo info, string gatherSize, ushort naturalType)
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
			num = ((animal.Length != 0) ? (num - 1) : (num + 5));
		}
		if (size != null)
		{
			num = ((size.Length != 0 && !size.ContainsIgnoreCase(gatherSize)) ? (num - 1) : (num + 1));
		}
		if (info != null && sprite != null)
		{
			num = ((sprite.Length != 0 && !sprite.Any(info.HasSprite)) ? (num - 1) : (num + 1));
		}
		if (type != null && naturalType != 0)
		{
			num = ((!type.Contains(naturalType)) ? (num - 1) : (num + 5));
		}
		return num;
	}
}
