using System;

namespace tk2dRuntime.TileMap;

[Serializable]
public class SpriteChannel
{
	public SpriteChunk[] chunks;

	public SpriteChannel()
	{
		chunks = new SpriteChunk[0];
	}
}
