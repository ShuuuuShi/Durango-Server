using System.Collections.Generic;
using Durango.Render.Effect;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Battle;
using UnityEngine;

public class ProjectileController
{
	private GameObject _arrowObjTemplate;

	private GameObject _arrowObj;

	private GameObject _arrowObjTrail;

	private readonly List<Projectile> _projectileList = new List<Projectile>();

	private readonly Transform _aimBasis;

	private readonly Transform _yawTransform;

	private readonly bool _fireUntargetProjectile;

	[CanBeNull]
	private DamageEffectManager.ProjectileSet _currentProjectileSet;

	private Projectile _lastProjectile;

	private bool _missed = true;

	public BodyPart TargetPart { get; private set; }

	public Vector3? TargetPos { get; private set; }

	public DamageableEntity Target { get; private set; }

	public ProjectileController(Transform aimBasis, Transform yawTransform)
	{
		_yawTransform = yawTransform;
		_aimBasis = aimBasis;
		_fireUntargetProjectile = true;
	}

	public ProjectileController(Transform aimBasis)
	{
		_yawTransform = null;
		_aimBasis = aimBasis;
		_fireUntargetProjectile = false;
	}

	private bool IsOverridableTarget(Projectile projectile)
	{
		if (projectile == null)
		{
			return false;
		}
		if (projectile.HasTarget())
		{
			return false;
		}
		if (projectile.ShootedAt.HasValue && projectile.ShootedAt.Value + 0.5f < Time.time)
		{
			return false;
		}
		return true;
	}

	public void SetTarget(DamageableEntity target, BodyPart part, bool missed)
	{
		if (IsOverridableTarget(_lastProjectile))
		{
			_lastProjectile.Missed = _missed;
			_lastProjectile.SetTarget(target, part);
			return;
		}
		Target = target;
		TargetPart = part;
		TargetPos = null;
		_missed = missed;
	}

	public void SetTarget(Vector3 target, bool missed)
	{
		if (IsOverridableTarget(_lastProjectile))
		{
			_lastProjectile.Missed = _missed;
			_lastProjectile.SetTarget(target);
			return;
		}
		TargetPos = target;
		Target = null;
		TargetPart = BodyPart.Invalid;
		_missed = missed;
	}

	public void SetWeaponData(WeaponDisplayInfo weaponDisplayInfo)
	{
		_currentProjectileSet = null;
		if (string.IsNullOrEmpty(weaponDisplayInfo.Projectile))
		{
			return;
		}
		DamageEffectManager.ProjectileType projectileType = weaponDisplayInfo.Projectile.ToEnum(DamageEffectManager.ProjectileType.Arrow);
		_currentProjectileSet = Singleton<DamageEffectManager>.Instance().ProjectileSets[(int)projectileType].Copy();
		if (_currentProjectileSet == null)
		{
			return;
		}
		if (weaponDisplayInfo.ProjectileSpeed.HasValue)
		{
			_currentProjectileSet.Speed = weaponDisplayInfo.ProjectileSpeed.Value;
		}
		if (weaponDisplayInfo.DetonateDelay.HasValue)
		{
			_currentProjectileSet.ExplosionDelay = weaponDisplayInfo.DetonateDelay.Value;
		}
		Singleton<AssetBundleManager>.Instance().RequestAsset(_currentProjectileSet.ProjectilePrefab.Path, typeof(GameObject), delegate(Object asset)
		{
			if (this != null)
			{
				_arrowObjTemplate = (GameObject)asset;
			}
		});
		SoundManager.PrepareEvent(_currentProjectileSet.ProjectileGlidingSound);
		SoundManager.PrepareEvent(_currentProjectileSet.ExplosionSound);
	}

	public void ModifyProjectileSpeed(float speed)
	{
		if (_currentProjectileSet != null)
		{
			_currentProjectileSet.Speed = speed;
		}
	}

	public float EstimateLaunchingTime(float distance)
	{
		if (_currentProjectileSet == null || _currentProjectileSet.Speed <= 0f)
		{
			return 0f;
		}
		return distance / _currentProjectileSet.Speed;
	}

	public void ChargedProjectile()
	{
		if (_currentProjectileSet == null)
		{
			return;
		}
		if (_arrowObj == null)
		{
			PrepareArrow();
			if (_arrowObj == null)
			{
				return;
			}
		}
		Projectile projectile = new Projectile(_currentProjectileSet);
		projectile.Arrow = _arrowObj.transform;
		_lastProjectile = projectile;
	}

	private void PrepareArrow()
	{
		DamageEffectManager.ProjectileSet currentProjectileSet = _currentProjectileSet;
		if (currentProjectileSet == null)
		{
			return;
		}
		ForceRemoveUnfiredArrow();
		_arrowObj = MakeArrow();
		if (!(_arrowObj == null))
		{
			Transform transform = _arrowObj.transform.Find(currentProjectileSet.ProjectileTrailName);
			if (transform != null)
			{
				_arrowObjTrail = transform.gameObject;
				_arrowObjTrail.SetActive(value: false);
			}
		}
	}

	private GameObject MakeArrow()
	{
		if (_currentProjectileSet == null)
		{
			return null;
		}
		if (_arrowObjTemplate == null)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(_arrowObjTemplate, _aimBasis);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		return gameObject;
	}

	public void OnRemoved()
	{
		ForceRemoveUnfiredArrow();
		int count = _projectileList.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			Projectile projectile = _projectileList[num];
			if (projectile.NeedToDestroy())
			{
				if (projectile.Arrow != null)
				{
					Object.Destroy(projectile.Arrow.gameObject);
				}
				_projectileList.RemoveAt(num);
			}
		}
	}

	public void ForceRemoveUnfiredArrow()
	{
		if (_arrowObj != null)
		{
			Object.Destroy(_arrowObj);
			_arrowObj = null;
		}
	}

	public void ShootProjectile()
	{
		DamageEffectManager.ProjectileSet currentProjectileSet = _currentProjectileSet;
		if (currentProjectileSet == null)
		{
			return;
		}
		if (!_fireUntargetProjectile && !TargetPos.HasValue && Target == null)
		{
			ForceRemoveUnfiredArrow();
			return;
		}
		if (_arrowObj == null)
		{
			ChargedProjectile();
			if (_arrowObj == null)
			{
				return;
			}
		}
		if (_lastProjectile == null)
		{
			return;
		}
		_lastProjectile.BeginTime = Time.time;
		if (_lastProjectile.Arrow != null)
		{
			_lastProjectile.Missed = _missed;
			_lastProjectile.BeginPos = _lastProjectile.Arrow.transform.position;
			if (TargetPos.HasValue)
			{
				_lastProjectile.SetTarget(TargetPos.Value);
			}
			else if (Target != null)
			{
				_lastProjectile.SetTarget(Target, TargetPart);
			}
			SoundManager.PlayEvent(currentProjectileSet.ProjectileGlidingSound, SoundPosition.Fix(_lastProjectile.BeginPos));
		}
		_lastProjectile.MissedTargetPos = _lastProjectile.BeginPos + _yawTransform.forward * 2000f;
		_lastProjectile.Shoot();
		Transform transform = _arrowObj.transform;
		transform.parent = null;
		_arrowObj = null;
		if (_arrowObjTrail != null)
		{
			_arrowObjTrail.SetActive(value: true);
			_arrowObjTrail = null;
		}
		_projectileList.Add(_lastProjectile);
		TargetPos = null;
		Target = null;
		_missed = true;
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
					if (projectile.Arrow != null)
					{
						Object.Destroy(projectile.Arrow.gameObject);
					}
					_projectileList.RemoveAt(num);
				}
				if (!(projectile.HitTime > 0f) && projectile.Process())
				{
					projectile.HitTime = Time.time;
					projectile.Hit();
				}
			}
		}
	}
}
