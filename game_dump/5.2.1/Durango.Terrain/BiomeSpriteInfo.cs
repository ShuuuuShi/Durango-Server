using Durango.Render.Sprite;
using Durango.Utils;
using Durango.Utils.Extensions;
using Shared.Region;

namespace Durango.Terrain;

public class BiomeSpriteInfo
{
	public int EntityType;

	public string[] SpriteNames;

	public string CollectibleId;

	public string Icon;

	public string Name;

	public bool Additive;

	public string Particle;

	public Biome[] Survivability;

	public bool IsCraft;

	private SpriteObjectType _spriteObjectType = SpriteObjectType.TypeCount;

	public SpriteObjectType SpriteObjectType
	{
		get
		{
			if (_spriteObjectType != SpriteObjectType.TypeCount)
			{
				return _spriteObjectType;
			}
			int size = KUtility.GetSize(SpriteNames);
			_spriteObjectType = Singleton<SpriteManager>.Instance().GetSpriteObjectType((size <= 0) ? string.Empty : SpriteNames[0]);
			return _spriteObjectType;
		}
		set
		{
			_spriteObjectType = value;
		}
	}

	public bool HasSprite(string spriteName)
	{
		if (SpriteNames == null)
		{
			return false;
		}
		return SpriteNames.ContainsIgnoreCase(spriteName);
	}
}
