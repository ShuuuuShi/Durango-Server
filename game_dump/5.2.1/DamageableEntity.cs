using JetBrains.Annotations;
using Messages;
using Shared.Battle;
using UnityEngine;

public abstract class DamageableEntity : BaseDerivedEntity
{
	private float? _height;

	public int GaugeChanged { get; protected set; }

	public virtual string TargetId { get; set; }

	public bool IsAlive
	{
		get
		{
			Gauge life = GetLife();
			if (life != null)
			{
				return life.Get() > 0f;
			}
			return true;
		}
	}

	public float Height
	{
		get
		{
			if (_height.HasValue)
			{
				return _height.Value;
			}
			_height = CalcHeight();
			return _height.Value;
		}
	}

	public abstract float XRadius { get; }

	public abstract float YRadius { get; }

	public abstract ProjectileController ProjectileController { get; }

	protected DamageableEntity(GameObject gameObject)
		: base(gameObject)
	{
		GaugeChanged = Time.frameCount;
	}

	public abstract Vector3 GetCurrentPosition();

	public abstract Vector3 GetInteractionPosition();

	public abstract string GetEntityId();

	public abstract int GetEntityTypeId();

	public virtual Point2? GetTile()
	{
		return null;
	}

	public abstract Gauge GetLife();

	public abstract string GetName();

	public abstract int GetLevel();

	public virtual float[] GetLifeGaugeRatio()
	{
		return null;
	}

	public abstract void AddGaugeUpdateDelegate();

	public abstract void RemoveGaugeUpdateDelegate();

	public virtual float GetGaugeScale()
	{
		return 1f;
	}

	protected abstract float CalcHeight();

	public abstract Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3));

	public virtual void SetPreDamaged(Damaged dmg)
	{
		if (ProjectileController != null)
		{
			DamageableEntity target = GameSystem<CombatSystem>.Instance().DamageableEntities.Get(dmg.VictimId);
			bool missed = false;
			DamageResult result = dmg.Damage.Result;
			if ((uint)(result - 1) <= 3u || (uint)(result - 6) <= 2u)
			{
				missed = true;
			}
			ProjectileController.SetTarget(target, dmg.Damage.Part, missed);
		}
	}

	public abstract void OnTakeDamage(Damage dmg, [CanBeNull] DamageableEntity attacker);
}
