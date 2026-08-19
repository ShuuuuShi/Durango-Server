using Messages;
using Shared.Battle;
using UnityEngine;

public class CharacterDamageableEntity : ComponentBasedDamageableEntity<CharacterBehavior>
{
	private readonly ProjectileController _projectileController;

	public override float XRadius => base.OwnerComponent.XRadius;

	public override float YRadius => base.OwnerComponent.YRadius;

	public override ProjectileController ProjectileController => _projectileController;

	public CharacterDamageableEntity(CharacterBehavior component)
		: base(component)
	{
		PlayerBehavior playerBehavior = component as PlayerBehavior;
		if (playerBehavior != null)
		{
			_projectileController = playerBehavior.ProjectileController;
		}
	}

	public override Vector3 GetCurrentPosition()
	{
		return base.OwnerComponent.CurrentPosition;
	}

	public override Vector3 GetInteractionPosition()
	{
		return base.OwnerComponent.InteractionPosition;
	}

	public override string GetEntityId()
	{
		return base.OwnerComponent.EntityId;
	}

	public override int GetEntityTypeId()
	{
		return base.OwnerComponent.EntityTypeId;
	}

	public override Gauge GetLife()
	{
		return base.OwnerComponent.Life;
	}

	public override string GetName()
	{
		return base.OwnerComponent.GetName();
	}

	public override float[] GetLifeGaugeRatio()
	{
		return base.OwnerComponent.GetLifeGaugeRatio();
	}

	public override int GetLevel()
	{
		return base.OwnerComponent.Level;
	}

	public override void AddGaugeUpdateDelegate()
	{
		base.OwnerComponent.SurvivalGaugeUpdated += OnUpdateTargetLifeGauge;
	}

	public override void RemoveGaugeUpdateDelegate()
	{
		base.OwnerComponent.SurvivalGaugeUpdated -= OnUpdateTargetLifeGauge;
	}

	private void OnUpdateTargetLifeGauge(CharacterBehavior target)
	{
		base.GaugeChanged = Time.frameCount;
	}

	public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))
	{
		return base.OwnerComponent.GetBodyPartTransform(part, bAllowNull, nearPos);
	}

	public override void OnTakeDamage(Damage dmg, DamageableEntity attacker)
	{
		base.OwnerComponent.OnTakeDamage(dmg, attacker);
	}

	protected override float CalcHeight()
	{
		Collider[] componentsInChildren = base.OwnerComponent.gameObject.GetComponentsInChildren<Collider>();
		Bounds bounds = default(Bounds);
		float num = 0f;
		int i = 0;
		for (int size = KUtility.GetSize(componentsInChildren); i < size; i++)
		{
			Bounds bounds2 = componentsInChildren[i].bounds;
			float sqrMagnitude = bounds2.extents.sqrMagnitude;
			if (sqrMagnitude > num)
			{
				num = sqrMagnitude;
				bounds = bounds2;
			}
		}
		return bounds.max.y;
	}
}
