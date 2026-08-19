using System;

namespace tk2dRuntime.TileMap;

[Serializable]
public class LayerSprites
{
	public int[] spriteIds;

	public LayerSprites()
	{
		spriteIds = new int[0];
	}
}
