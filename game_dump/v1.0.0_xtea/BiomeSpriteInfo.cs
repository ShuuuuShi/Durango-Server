using UnityEngine;

public class BiomeSpriteInfo
{
	public int EntityType;

	public string[] SpriteNames;

	public string CollectibleId;

	public string Icon;

	public SpriteObjectType SpriteObjectType;

	public SpriteColliderSize SpriteColliderSize;

	public bool IsShakable;

	public bool IsSwayable;

	public string StumpName;

	public int RandomYaw;

	public Vector2 RandomHeight;

	public Vector2 RandomSize;

	public Vector2 RandomBrightness;

	public bool HasSprite(string spriteName)
	{
		if (SpriteNames == null)
		{
			return false;
		}
		return SpriteNames.ContainsIgnoreCase(spriteName);
	}
}
