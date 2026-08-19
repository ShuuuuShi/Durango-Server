using System;
using System.Collections.Generic;
using Messages;
using Shared.Battle;
using TerrainData;
using UnityEngine;

public class DamageEffectManager : KSingleton<DamageEffectManager>
{
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

	public enum ProjectileType
	{
		Invalid = -1,
		Arrow,
		Stone,
		Grenade,
		Count
	}

	[Serializable]
	public class ProjectileSet
	{
		public ParticleType ProjectilePrefab;

		public string ProjectileTrailName = "ArrowTrail";

		public float Speed = 3000f;

		public bool CurvedTrajectory;

		public bool StickToTarget;

		public AudioClipType ProjectileGlidingSound;

		public float ExplosionDelay;

		public ParticleType ExplosionParticle;

		public AudioClipType ExplosionSound;

		public Vibration.Id ExplosionVibrateId = Vibration.Id.None;

		public bool OnlyVibrateAtLocalPlayerAttacked = true;
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
			foreach (EffectSet effectSet in effectList)
			{
				IntegratedEffect.Precache(effectSet.IntegratedEffect);
			}
		}
	}

	public void PlayEffectSet(AttackType type, Result result, Vector3 position, bool isAttackerLocalPlayer)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		EffectSet effectSet = AttackedEffects[(int)type].effectList[(int)result];
		IntegratedEffect.Emit(effectSet.IntegratedEffect, Biome.Unspecified, position, Quaternion.identity);
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

	public EffectSet GetEffectSet(AttackType type, Result result)
	{
		return AttackedEffects[(int)type].effectList[(int)result];
	}

	public static Result ConvertToAttackResult(Damage damage)
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

	public static void PlayDamagerEffectSet(GameObject attacker, Damage damage, Vector3 pos)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Result result = ConvertToAttackResult(damage);
		bool isAttackerLocalPlayer = (Object)(object)attacker == (Object)(object)PlayerBehavior.LocalPlayer;
		KSingleton<DamageEffectManager>.Instance().PlayEffectSet(damage.AttackType, result, pos, isAttackerLocalPlayer);
		if (damage.Value > 0 && (damage.Effects & DamageEffects.Critical) != 0)
		{
			KSingleton<DamageEffectManager>.Instance().PlayEffectSet(damage.AttackType, Result.Critical, pos, isAttackerLocalPlayer);
		}
	}
}
