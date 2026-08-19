using Durango.Render.Effect;
using Durango.Render.Particle;
using Durango.Utils;
using JetBrains.Annotations;
using Shared.Battle;
using Shared.Region;
using UnityEngine;

public class Projectile
{
	[CanBeNull]
	public Transform Arrow;

	public Vector3 BeginPos = Vector3.zero;

	public float BeginTime = -1f;

	public float HitTime = -1f;

	public bool Missed;

	[NotNull]
	public readonly DamageEffectManager.ProjectileSet ProjectileSet;

	private float? _untargetLimitUntil;

	private bool _hasTarget;

	private Transform _target;

	private Vector3? _targetPos;

	public Vector3? MissedTargetPos;

	public float? ShootedAt;

	[CanBeNull]
	private DamageableEntity _damageable;

	private bool _isDetonated;

	public Projectile(DamageEffectManager.ProjectileSet set)
	{
		ProjectileSet = set;
		_untargetLimitUntil = Time.time + 5f;
	}

	public bool HasTarget()
	{
		return _hasTarget;
	}

	public void SetTarget([CanBeNull] DamageableEntity target, BodyPart part)
	{
		_hasTarget = true;
		_damageable = target;
		_target = ((!(target == null)) ? target.GetBodyPartTransform(part) : null);
		_targetPos = null;
		_untargetLimitUntil = null;
	}

	public void SetTarget(Vector3 target)
	{
		_hasTarget = true;
		_target = null;
		_targetPos = target;
		_untargetLimitUntil = null;
	}

	public bool TimeToDetonate()
	{
		if (HitTime < 0f || _isDetonated)
		{
			return false;
		}
		return HitTime + ProjectileSet.ExplosionDelay < Time.time;
	}

	public void Shoot()
	{
		ShootedAt = Time.time;
		if (Arrow != null)
		{
			IntegratedEffect.Emit(ProjectileSet.ShootProjectileEffect.Path, Biome.Invalid, Arrow.position, Quaternion.identity);
		}
	}

	public void Detonate()
	{
		_isDetonated = true;
		if (!(Arrow == null))
		{
			Vector3 position = Arrow.position;
			ParticleManager.Emit(ProjectileSet.ExplosionParticle.Path, position, Quaternion.identity);
			IntegratedEffect.Emit(ProjectileSet.ExplosionEffect.Path, Biome.Invalid, position, Quaternion.identity);
			SoundManager.PlayEvent(ProjectileSet.ExplosionSound, SoundPosition.Fix(position));
		}
	}

	public bool NeedToDestroy()
	{
		if (_untargetLimitUntil.HasValue && _untargetLimitUntil.Value < Time.time)
		{
			return true;
		}
		if (HitTime < 0f)
		{
			return false;
		}
		float num = 0f;
		if (ProjectileSet.StickToTarget)
		{
			num = ((!(_damageable != null) || !_damageable.IsAlive) ? 5f : 10f);
		}
		return Missed || HitTime + ProjectileSet.ExplosionDelay + num < Time.time;
	}

	public bool Process()
	{
		float? shootedAt = ShootedAt;
		if (!shootedAt.HasValue)
		{
			return false;
		}
		return (!ProjectileSet.CurvedTrajectory) ? ProcessFlatTrajectory(ProjectileSet) : ProcessCurvedTrajectory(ProjectileSet);
	}

	[Pure]
	private Vector3 GetTargetPos()
	{
		if (_hasTarget || Missed)
		{
			if (_targetPos.HasValue)
			{
				return _targetPos.Value;
			}
			if (_target != null)
			{
				return _target.position;
			}
		}
		return MissedTargetPos.GetValueOrDefault(BeginPos);
	}

	private bool ProcessFlatTrajectory([NotNull] DamageEffectManager.ProjectileSet projectileSet)
	{
		if (Arrow == null)
		{
			return true;
		}
		Vector3 targetPos = GetTargetPos();
		Vector3 position = Arrow.position;
		Vector3 vector = targetPos - position;
		float magnitude = vector.magnitude;
		vector.Normalize();
		Arrow.rotation = Quaternion.LookRotation(-vector);
		float num = Time.deltaTime * projectileSet.Speed;
		bool result = magnitude < 100f;
		position += vector * num;
		Arrow.position = position;
		return result;
	}

	private bool ProcessCurvedTrajectory([NotNull] DamageEffectManager.ProjectileSet projectileSet)
	{
		if (Arrow == null)
		{
			return true;
		}
		Vector3 vector = Maths.Make2D(GetTargetPos());
		float magnitude = (vector - BeginPos).magnitude;
		float num = magnitude / projectileSet.Speed;
		float num2 = Mathf.Clamp01((Time.time - BeginTime) / num);
		float num3 = TrajectoryFunc(num2);
		Vector3 vector2 = Vector3.Lerp(BeginPos, vector, num2);
		vector2.y += num3 * magnitude * projectileSet.CurvedTrajectoryHeightRatio;
		Arrow.rotation = Quaternion.LookRotation((vector2 - Arrow.position).normalized, Vector3.up);
		Arrow.position = vector2;
		return num2 >= 1f;
	}

	private static float TrajectoryFunc(float x)
	{
		return -9.8f * x * (x - 1f);
	}

	public void Hit()
	{
		if (ProjectileSet.StickToTarget)
		{
			AttachArrowToTarget();
		}
	}

	private void AttachArrowToTarget()
	{
		if (Arrow == null || _target == null)
		{
			return;
		}
		Arrow.parent = _target;
		Arrow.localPosition = Vector3.zero;
		float num = CalcArrowScaleAtPinned(_damageable);
		Transform transform = Arrow.Find("arrow");
		if (!(transform == null))
		{
			transform.localPosition = new Vector3(0f, 0f, 100f * num);
			ParticleSystem component = transform.GetComponent<ParticleSystem>();
			if (component != null)
			{
				ParticleSystem.MainModule main = component.main;
				main.startSizeMultiplier *= num;
			}
		}
	}

	private static float CalcArrowScaleAtPinned([CanBeNull] DamageableEntity damageable)
	{
		if (damageable == null)
		{
			return 1f;
		}
		float num = Mathf.Max(damageable.XRadius, damageable.YRadius);
		float t = (num - 50f) / 100f;
		return Mathf.Lerp(1f, 1.5f, t);
	}
}
