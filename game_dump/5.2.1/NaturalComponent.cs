using Durango.Render.Sprite;
using UnityEngine;

public abstract class NaturalComponent : BaseDerivedEntity
{
	public NaturalSpriteObject Natural { get; private set; }

	public Vector3 Position => base.GameObject.transform.position;

	public Durango.Render.Sprite.Sprite Sprite => Natural.Sprite;

	protected NaturalComponent(NaturalSpriteObject natural)
		: base(natural.gameObject)
	{
		Natural = natural;
	}
}
