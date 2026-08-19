using System.Runtime.InteropServices;
using Shared.Battle;
using UnityEngine;

public abstract class DamageableEntity : BaseDerivedEntity
{
	public bool GaugeChanged { get; set; }

	public bool IsAlive => GetLife().Get() > 0f;

	public abstract float XRadius { get; }

	public abstract float YRadius { get; }

	protected DamageableEntity(GameObject gameObject)
		: base(gameObject)
	{
		GaugeChanged = true;
	}

	public abstract Vector3 GetCurrentPosition();

	public abstract ulong GetEntityId();

	public abstract int GetEntityTypeId();

	public abstract Gauge GetLife();

	public abstract string GetName();

	public abstract int GetLevel();

	public abstract void AddLifeGaugeUpdateDelegate();

	public abstract void RemoveLifeGaugeUpdateDelegate();

	public virtual float GetGaugeScale()
	{
		return 1f;
	}

	public abstract Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, [Optional] Vector3 nearPos);

	public static DamageableEntity Create(GameObject obj)
	{
		CharacterBehavior component = obj.GetComponent<CharacterBehavior>();
		if (Object.op_Implicit((Object)(object)component))
		{
			return new CharacterDamageableEntity(component);
		}
		Artifact component2 = obj.GetComponent<Artifact>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			return new ArtifactDamageableEntity(component2);
		}
		return null;
	}

	public static bool IsDamageableEntity(GameObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return false;
		}
		CharacterBehavior component = obj.GetComponent<CharacterBehavior>();
		if (Object.op_Implicit((Object)(object)component))
		{
			return true;
		}
		Artifact component2 = obj.GetComponent<Artifact>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			return true;
		}
		return false;
	}
}
