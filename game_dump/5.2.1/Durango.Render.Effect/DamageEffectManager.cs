using System;
using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Battle;
using Shared.Region;
using UnityEngine;

namespace Durango.Render.Effect;

public class DamageEffectManager : Singleton<DamageEffectManager>
{
	public enum ProjectileType
	{
		Invalid = -1,
		Arrow,
		Stone,
		Grenade,
		Catapult,
		TurretBow,
		Count
	}

	public enum Result
	{
		Hit,
		Critical,
		Blocked,
		Miss,
		Count
	}

	[Serializable]
	public class EffectSet
	{
		public IntegratedEffectType IntegratedEffect;

		public Vibration.Id vibrateId = Vibration.Id.None;

		public bool OnlyVibrateAtLocalPlayerAttacked = true;
	}

	[Serializable]
	public class DamageEffect
	{
		public EffectSet[] effectList;
	}

	[Serializable]
	public class ProjectileSet
	{
		public ParticleType ProjectilePrefab;

		public string ProjectileTrailName = "ArrowTrail";

		public float Speed = 3000f;

		public bool CurvedTrajectory;

		public float CurvedTrajectoryHeightRatio = 0.1f;

		public bool StickToTarget;

		public SoundEventType ProjectileGlidingSound;

		public IntegratedEffectType ShootProjectileEffect;

		public float ExplosionDelay;

		public ParticleType ExplosionParticle;

		public IntegratedEffectType ExplosionEffect;

		public SoundEventType ExplosionSound;

		public Vibration.Id ExplosionVibrateId = Vibration.Id.None;

		public bool OnlyVibrateAtLocalPlayerAttacked = true;

		public ProjectileSet Copy()
		{
			return (ProjectileSet)MemberwiseClone();
		}
	}

	[SerializeField]
	private List<DamageEffect> _attackedEffects;

	[SerializeField]
	private ProjectileSet[] _projectileSets;

	public List<DamageEffect> AttackedEffects
	{
		get
		{
			return _attackedEffects;
		}
		set
		{
			_attackedEffects = value;
		}
	}

	public ProjectileSet[] ProjectileSets
	{
		get
		{
			return _projectileSets;
		}
		set
		{
			_projectileSets = value;
		}
	}

	protected override void OnAwake()
	{
		int count = AttackedEffects.Count;
		for (int i = 0; i < count; i++)
		{
			EffectSet[] effectList = AttackedEffects[i].effectList;
			for (int j = 0; j < effectList.Length; j++)
			{
				IntegratedEffect.Precache(effectList[j].IntegratedEffect);
			}
		}
		count = ProjectileSets.Length;
		for (int k = 0; k < count; k++)
		{
			IntegratedEffect.Precache(ProjectileSets[k].ShootProjectileEffect);
			IntegratedEffect.Precache(ProjectileSets[k].ExplosionEffect);
		}
	}

	public void PlayEffectSet(AttackType type, Result result, Vector3 position, bool isAttackerLocalPlayer)
	{
		EffectSet effectSet = AttackedEffects[(int)type].effectList[(int)result];
		IntegratedEffect.Emit(effectSet.IntegratedEffect, Biome.Invalid, position, Quaternion.identity);
		if (effectSet.vibrateId == Vibration.Id.None)
		{
			return;
		}
		if (effectSet.OnlyVibrateAtLocalPlayerAttacked)
		{
			if (isAttackerLocalPlayer)
			{
				Vibration.Vibrate((int)effectSet.vibrateId);
			}
		}
		else
		{
			Vibration.Vibrate((int)effectSet.vibrateId);
		}
	}

	private static Result ConvertToAttackResult(Damage damage)
	{
		Result result = Result.Hit;
		switch (damage.Result)
		{
		case DamageResult.Guarded:
		case DamageResult.AutoGuarded:
			result = Result.Blocked;
			break;
		case DamageResult.Dodged:
		case DamageResult.Missed:
		case DamageResult.AutoDodged:
			result = Result.Miss;
			break;
		default:
			if (damage.Value <= 0)
			{
				result = Result.Miss;
			}
			break;
		}
		return result;
	}

	public static void PlayDamageEffectSet([CanBeNull] DamageableEntity attacker, Damage damage, Vector3 pos)
	{
		if (Singleton<DamageEffectManager>.HasInstance())
		{
			Result result = ConvertToAttackResult(damage);
			bool isAttackerLocalPlayer = attacker != null && attacker.GetEntityId() == GameManager.PlayerId;
			Singleton<DamageEffectManager>.Instance().PlayEffectSet(damage.AttackType, result, pos, isAttackerLocalPlayer);
			if (damage.Value > 0 && (damage.Effects & DamageEffects.Critical) != 0)
			{
				Singleton<DamageEffectManager>.Instance().PlayEffectSet(damage.AttackType, Result.Critical, pos, isAttackerLocalPlayer);
			}
		}
	}
}
