using Messages;
using Shared.Battle;
using UnityEngine;

public class ArtifactDamageableEntity : ComponentBasedDamageableEntity<Artifact>
{
	private readonly ProjectileController _projectileController;

	private string _targetId;

	public override float XRadius => (float)(base.OwnerComponent.Size.x * 200) * 0.5f;

	public override float YRadius => (float)(base.OwnerComponent.Size.y * 200) * 0.5f;

	public override ProjectileController ProjectileController => _projectileController;

	public override string TargetId
	{
		get
		{
			return _targetId;
		}
		set
		{
			if (!(_targetId == value))
			{
				_targetId = value;
				base.OwnerComponent.SetEnemy(_targetId);
			}
		}
	}

	public ArtifactDamageableEntity(Artifact component)
		: base(component)
	{
		Defensive artifactComponent = component.GetArtifactComponent<Defensive>();
		if (artifactComponent != null)
		{
			_projectileController = artifactComponent.ProjectileController;
		}
	}

	public override Vector3 GetCurrentPosition()
	{
		return base.OwnerComponent.Center;
	}

	public override Vector3 GetInteractionPosition()
	{
		return base.OwnerComponent.InteractionPosition;
	}

	public override string GetEntityId()
	{
		return base.OwnerComponent.EntityId;
	}

	public override Point2? GetTile()
	{
		return base.OwnerComponent.WorldTile;
	}

	public override int GetEntityTypeId()
	{
		return base.OwnerComponent.EntityType;
	}

	public override Gauge GetLife()
	{
		return base.OwnerComponent.Durability;
	}

	public override string GetName()
	{
		return base.OwnerComponent.LocalizedName;
	}

	public override int GetLevel()
	{
		return base.OwnerComponent.ArtifactState.Level;
	}

	public override float GetGaugeScale()
	{
		if (base.OwnerComponent.Durability != null)
		{
			return base.OwnerComponent.MaxHealth / base.OwnerComponent.Durability.Max();
		}
		return base.GetGaugeScale();
	}

	public override void AddGaugeUpdateDelegate()
	{
		base.OwnerComponent.DurabilityGaugeUpdated += OnUpdateTargetLifeGauge;
	}

	public override void RemoveGaugeUpdateDelegate()
	{
		base.OwnerComponent.DurabilityGaugeUpdated -= OnUpdateTargetLifeGauge;
	}

	private void OnUpdateTargetLifeGauge(Artifact target)
	{
		base.GaugeChanged = Time.frameCount;
	}

	public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))
	{
		Transform transform = base.OwnerComponent.transform;
		return (transform.childCount <= 0) ? transform : transform.GetChild(0);
	}

	public override void SetPreDamaged(Damaged dmg)
	{
		base.SetPreDamaged(dmg);
		if (ProjectileController != null)
		{
			base.OwnerComponent.GetArtifactComponent<Defensive>()?.ShootProjectile(dmg.EventAt);
		}
	}

	public override void OnTakeDamage(Damage dmg, DamageableEntity attacker)
	{
		base.OwnerComponent.OnTakeDamage(dmg, attacker);
	}

	protected override float CalcHeight()
	{
		Renderer[] componentsInChildren = base.OwnerComponent.GetComponentsInChildren<Renderer>();
		Bounds bounds = default(Bounds);
		float num = 0f;
		int i = 0;
		for (int size = KUtility.GetSize(componentsInChildren); i < size; i++)
		{
			if (!(componentsInChildren[i] is ParticleSystemRenderer))
			{
				Bounds bounds2 = componentsInChildren[i].bounds;
				float sqrMagnitude = bounds2.extents.sqrMagnitude;
				if (sqrMagnitude > num)
				{
					num = sqrMagnitude;
					bounds = bounds2;
				}
			}
		}
		return bounds.max.y;
	}
}
