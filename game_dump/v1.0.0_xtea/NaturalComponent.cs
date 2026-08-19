using UnityEngine;

public abstract class NaturalComponent : BaseDerivedEntity
{
	public NaturalObject Natural { get; private set; }

	public Vector3 Position => base.GameObject.transform.position;

	public KSprite KSprite => Natural.KSprite;

	protected NaturalComponent(NaturalObject natural)
		: base(((Component)natural).gameObject)
	{
		Natural = natural;
	}
}
