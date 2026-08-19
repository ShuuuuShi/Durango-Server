using System;
using Shared.Battle;
using UnityEngine;

internal class Projectile
{
	public GameObject Arrow;

	public GameObject Target;

	public Vector3 BeginPos = Vector3.zero;

	public Vector3 EndPos = Vector3.zero;

	public DamageableEntity Damageable;

	public BodyPart Part;

	public float BeginTime = -1f;

	public float HitTime = -1f;

	public DamageResult DamageResult = DamageResult.Invalid;

	public DamageEffectManager.ProjectileSet ProjectileSet;

	public Action Detonated;

	private bool _isDetonated;

	private bool IsMissed()
	{
		return DamageResult == DamageResult.Dodged || DamageResult == DamageResult.AutoDodged || DamageResult == DamageResult.Guarded || DamageResult == DamageResult.AutoGuarded || DamageResult == DamageResult.Missed;
	}

	public bool TimeToDetonate()
	{
		if (HitTime < 0f || _isDetonated)
		{
			return false;
		}
		return HitTime + ProjectileSet.ExplosionDelay < Time.time;
	}

	public void Detonate()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		_isDetonated = true;
		if (Detonated != null)
		{
			Detonated();
		}
		if (!string.IsNullOrEmpty(ProjectileSet.ExplosionParticle.Path))
		{
			ParticleManager.Emit(ProjectileSet.ExplosionParticle.Path, Arrow.transform.position, Quaternion.identity);
		}
		if (!string.IsNullOrEmpty(ProjectileSet.ExplosionSound.Path))
		{
			SoundManager.Play(ProjectileSet.ExplosionSound.Path, Arrow.transform.position);
		}
	}

	public bool NeedToDestroy()
	{
		if (HitTime < 0f)
		{
			return false;
		}
		float num = 0f;
		if (ProjectileSet.StickToTarget)
		{
			num = ((!Damageable || !Damageable.IsAlive) ? 5f : 30f);
		}
		return HitTime + ProjectileSet.ExplosionDelay + num < Time.time || IsMissed();
	}

	public bool Process()
	{
		if ((Object)(object)Arrow != (Object)null && (Object)(object)Target != (Object)null)
		{
			return (!ProjectileSet.CurvedTrajectory) ? ProcessFlatTrajectory() : ProcessCurvedTrajectory();
		}
		return true;
	}

	private bool ProcessFlatTrajectory()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = Target.transform.position;
		Vector3 position2 = Arrow.transform.position;
		Vector3 val = position - position2;
		((Vector3)(ref val)).Normalize();
		Arrow.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, val);
		position2 += val * Time.deltaTime * ProjectileSet.Speed;
		Arrow.transform.position = position2;
		Vector3 val2 = position - position2;
		((Vector3)(ref val2)).Normalize();
		float num = val2.x * val.x + val2.y * val.y + val2.z * val.z;
		return num < -0.9f;
	}

	private bool ProcessCurvedTrajectory()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = KMathUtil.Make2D(EndPos);
		Vector3 val2 = val - BeginPos;
		float magnitude = ((Vector3)(ref val2)).magnitude;
		float num = magnitude / ProjectileSet.Speed;
		float num2 = Mathf.Clamp01((Time.time - BeginTime) / num);
		float num3 = TrajectoryFunc(num2);
		Vector3 val3 = Vector3.Lerp(BeginPos, val, num2);
		val3.y += num3 * magnitude * 0.1f;
		Transform transform = Arrow.transform;
		Vector3 val4 = val3 - Target.transform.position;
		transform.rotation = Quaternion.LookRotation(((Vector3)(ref val4)).normalized, Vector3.up);
		Arrow.transform.position = val3;
		return num2 >= 1f;
	}

	private static float TrajectoryFunc(float x)
	{
		if (x <= 0.8f)
		{
			return -9.8f * x * (x - 0.8f) / 1.568f;
		}
		return -9.8f * (x - 0.8f) * (x - 1f);
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
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Target == (Object)null || (Object)(object)Arrow == (Object)null)
		{
			return;
		}
		BodyPart part = Part;
		Transform bodyPartTransform = Damageable.GetBodyPartTransform(part);
		Arrow.transform.parent = bodyPartTransform;
		Arrow.transform.localPosition = Vector3.zero;
		float num = CalcArrowScaleAtPinned(Damageable);
		Transform val = Arrow.transform.FindChild("arrow");
		if (!((Object)(object)val == (Object)null))
		{
			val.localPosition = new Vector3(0f, 0f, 100f * num);
			ParticleSystem component = ((Component)val).GetComponent<ParticleSystem>();
			if ((Object)(object)component != (Object)null)
			{
				component.startSize *= num;
			}
		}
	}

	private static float CalcArrowScaleAtPinned(DamageableEntity damageable)
	{
		if (Mathf.Abs(-100f) <= Mathf.Epsilon)
		{
			return 1f;
		}
		float num = Mathf.Max(damageable.XRadius, damageable.YRadius);
		float num2 = (num - 50f) / 100f;
		return Mathf.Lerp(1f, 1.5f, num2);
	}
}
