using System;
using System.Collections.Generic;
using Messages;
using Shared.Battle;
using UnityEngine;

public class ProjectileController
{
	private GameObject _arrowObjTemplate;

	private GameObject _arrowObj;

	private GameObject _arrowObjTrail;

	private readonly List<Projectile> _projectileList = new List<Projectile>();

	private DamageEffectManager.ProjectileSet _currentProjectileSet;

	private Projectile _lastProjectile;

	private Transform _aimBasis;

	public BodyPart LastAttackBodyPart { get; set; }

	public bool ProjectileWeaponEquipped => _currentProjectileSet != null;

	public event Action ProjectileDetonated;

	public ProjectileController(Transform aimBasis)
	{
		_aimBasis = aimBasis;
	}

	public void SetWeaponData(WeaponDisplayInfo weaponDisplayInfo)
	{
		_currentProjectileSet = null;
		if (!string.IsNullOrEmpty(weaponDisplayInfo.Projectile))
		{
			DamageEffectManager.ProjectileType projectileType = weaponDisplayInfo.Projectile.ToEnum(DamageEffectManager.ProjectileType.Arrow);
			_currentProjectileSet = KSingleton<DamageEffectManager>.Instance().ProjectileSets[(int)projectileType];
			if (weaponDisplayInfo.ProjectileSpeed.HasValue)
			{
				_currentProjectileSet.Speed = weaponDisplayInfo.ProjectileSpeed.Value;
			}
			if (weaponDisplayInfo.DetonateDelay.HasValue)
			{
				_currentProjectileSet.ExplosionDelay = weaponDisplayInfo.DetonateDelay.Value;
			}
			KSingleton<AssetBundleManager>.Instance().RequestAsset(_currentProjectileSet.ProjectilePrefab.Path, typeof(GameObject), delegate(Object asset)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				_arrowObjTemplate = (GameObject)asset;
			});
			SoundManager.Cache(_currentProjectileSet.ProjectileGlidingSound.Path);
			SoundManager.Cache(_currentProjectileSet.ExplosionSound.Path);
		}
	}

	public float EstimateLaunchingTime(float distance)
	{
		return distance / _currentProjectileSet.Speed;
	}

	public void OnChargedProjectile(GameObject target)
	{
		if ((Object)(object)_arrowObj == (Object)null)
		{
			PrepareArrow();
			if ((Object)(object)_arrowObj == (Object)null)
			{
				return;
			}
		}
		Projectile projectile = new Projectile();
		projectile.Arrow = _arrowObj;
		projectile.Target = target;
		FillProjectileBehaviors(projectile);
		if ((Object)(object)target != (Object)null)
		{
			projectile.Damageable = DamageableEntity.Create(target);
		}
		_lastProjectile = projectile;
	}

	private void PrepareArrow()
	{
		_arrowObj = MakeArrow();
		if (!((Object)(object)_arrowObj == (Object)null) && _currentProjectileSet != null)
		{
			Transform val = _arrowObj.transform.FindChild(_currentProjectileSet.ProjectileTrailName);
			if ((Object)(object)val != (Object)null)
			{
				_arrowObjTrail = ((Component)val).gameObject;
				_arrowObjTrail.SetActive(false);
			}
		}
	}

	private GameObject MakeArrow()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_arrowObjTemplate == (Object)null)
		{
			return null;
		}
		GameObject val = (GameObject)Object.Instantiate((Object)(object)_arrowObjTemplate, _aimBasis);
		val.transform.localPosition = Vector3.zero;
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		return val;
	}

	public void OnRemoved()
	{
		ForceRemovUnfiredArrow();
		int count = _projectileList.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			Projectile projectile = _projectileList[num];
			if (projectile.NeedToDestroy())
			{
				Object.Destroy((Object)(object)projectile.Arrow);
				_projectileList.RemoveAt(num);
			}
		}
	}

	public void ForceRemovUnfiredArrow()
	{
		if ((Object)(object)_arrowObj != (Object)null)
		{
			Object.Destroy((Object)(object)_arrowObj);
			_arrowObj = null;
		}
	}

	public void OnShootProjectile(GameObject target)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null)
		{
			return;
		}
		if ((Object)(object)_arrowObj == (Object)null)
		{
			OnChargedProjectile(target);
			if ((Object)(object)_arrowObj == (Object)null)
			{
				return;
			}
		}
		if (_lastProjectile != null)
		{
			DamageableEntity damageableEntity = DamageableEntity.Create(target);
			target = ((Component)damageableEntity.GetBodyPartTransform(LastAttackBodyPart)).gameObject;
			_lastProjectile.BeginTime = Time.time;
			_lastProjectile.Target = target;
			_lastProjectile.BeginPos = _lastProjectile.Arrow.transform.position;
			_lastProjectile.EndPos = _lastProjectile.Target.transform.position;
			_arrowObj.transform.parent = null;
			_arrowObj = null;
			if ((Object)(object)_arrowObjTrail != (Object)null)
			{
				_arrowObjTrail.SetActive(true);
				_arrowObjTrail = null;
			}
			if (!string.IsNullOrEmpty(_currentProjectileSet.ProjectileGlidingSound.Path))
			{
				SoundManager.Play(_currentProjectileSet.ProjectileGlidingSound.Path);
			}
			_projectileList.Add(_lastProjectile);
			SoundManager.Cache(_currentProjectileSet.ExplosionSound.Path);
		}
	}

	public void UpdateProjectiles()
	{
		int count = _projectileList.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			Projectile projectile = _projectileList[num];
			if (projectile == null)
			{
				_projectileList.RemoveAt(num);
			}
			else
			{
				if (projectile.TimeToDetonate())
				{
					projectile.Detonate();
				}
				if (projectile.NeedToDestroy())
				{
					Object.Destroy((Object)(object)projectile.Arrow);
					_projectileList.RemoveAt(num);
				}
				if (!(projectile.HitTime > 0f) && projectile.Process())
				{
					projectile.HitTime = Time.time;
					projectile.Part = LastAttackBodyPart;
					projectile.Hit();
				}
			}
		}
	}

	private void FillProjectileBehaviors(Projectile projectile)
	{
		if (_currentProjectileSet == null)
		{
			return;
		}
		projectile.ProjectileSet = _currentProjectileSet;
		projectile.Detonated = delegate
		{
			if (this.ProjectileDetonated != null)
			{
				this.ProjectileDetonated();
			}
		};
	}

	public void DamageResultReceived(Damage damage)
	{
		if (_lastProjectile != null)
		{
			_lastProjectile.DamageResult = damage.Result;
			_lastProjectile = null;
		}
	}
}
