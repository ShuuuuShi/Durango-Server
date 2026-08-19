using System.Collections;
using System.Collections.Generic;
using Durango.Model;
using Durango.Network;
using Durango.UI;
using Durango.UI.InGame;
using Durango.Utils;
using InteractionData;
using Messages;
using UnityEngine;

public class VehicleCatapult : VehicleProp
{
	private enum CatapultState
	{
		Unmounted,
		Stand,
		Turn,
		Shoot
	}

	private CatapultState _state;

	[SerializeField]
	private string _transitionToUnmount = "Turret_Catapult_Stand_Unmount";

	[SerializeField]
	private string _transitionToMount = "Turret_Catapult_Unmount_Stand";

	[SerializeField]
	private string _standUnmountedMotion = "Turret_Catapult_Stand_Before";

	[SerializeField]
	private string _standMountedMotion = "Turret_Catapult_Stand";

	[SerializeField]
	private string _turnMotion = "Turret_Catapult_Turn";

	[SerializeField]
	private string _prepareAttackMotion = "Turret_Catapult_Reload";

	[SerializeField]
	private string _attackMotion = "Turret_Catapult_Attack";

	[SerializeField]
	private Transform _attachmentProjectile;

	[SerializeField]
	private Transform _attachmentMagazine;

	private ProjectileController _projectileController;

	[SerializeField]
	private string _rideMotionSetName = "vehicle_catapult";

	[SerializeField]
	private Transform _bodyTransform;

	[SerializeField]
	private float _yawBias = 90f;

	[SerializeField]
	private string _magazinePrefabPath = "Models/Prop/facility/defence/turret_catapult_01_magazine_set.prefab";

	private CatapultMagazine _magazine;

	[SerializeField]
	private int _thresholdNumProfectileFullModel = 5;

	private int? _numRemainedProjectiles;

	private ICoroutineBinder _lastAttackCoroutine;

	public ProjectileController ProjectileController
	{
		get
		{
			if (_projectileController == null)
			{
				_projectileController = new ProjectileController(_attachmentProjectile);
				WeaponDisplayInfo weaponDisplayInfo = default(WeaponDisplayInfo);
				weaponDisplayInfo.Projectile = "Catapult";
				WeaponDisplayInfo weaponData = weaponDisplayInfo;
				ProjectileController.SetWeaponData(weaponData);
			}
			return _projectileController;
		}
	}

	public override bool IsMovable => false;

	public override float MoveSpeed
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public override string Name
	{
		get
		{
			Artifact artifact = GetArtifact();
			if (artifact == null)
			{
				return string.Empty;
			}
			return artifact.LocalizedName;
		}
	}

	public override bool IgnoreWaterFlow => true;

	public override Vector3 CameraPanOffset => GetArtifact().Center - PlayerBehavior.LocalPlayer.CurrentPosition;

	public override bool IsInteractionMenuVisible => false;

	public override bool EnableRidingStabilizer => false;

	private void Start()
	{
		Singleton<AssetBundleManager>.Instance().RequestAsset(_magazinePrefabPath, typeof(GameObject), delegate(Object asset)
		{
			if (!(asset == null))
			{
				GameObject gameObject = Object.Instantiate(asset, _attachmentMagazine.position, Quaternion.identity) as GameObject;
				_magazine = gameObject.GetComponent<CatapultMagazine>();
				_magazine.transform.parent = _attachmentMagazine;
				UpdateRemainedProjectiles();
			}
		});
	}

	public override void AttachDriver(Driver driver, GameObject saddle = null)
	{
		base.AttachDriver(driver, saddle);
		if (driver.IsLocalPlayer)
		{
			PlayerController.MotionUpdater.UpdateRideMotionSet(_rideMotionSetName);
		}
		SetState(CatapultState.Stand);
		AnimatingModel animatingProp = GetComponent<AnimatingModel>();
		if ((bool)animatingProp)
		{
			float delay = animatingProp.Play(_transitionToMount, loop: false);
			KUtility.DelayedCall(this, delegate
			{
				animatingProp.Play(_standMountedMotion);
			}, delay);
		}
		if (base.IsLocalPlayers)
		{
			SetBattleZoomView(on: true);
			GetRange(out var range, out var deadzone);
			Artifact artifact = GetArtifact();
			Singleton<CatapultRangeView>.Instance().Show(artifact.Center, deadzone, range);
		}
	}

	public override void DetachDriver()
	{
		base.DetachDriver();
		SetState(CatapultState.Unmounted);
		AnimatingModel animatingProp = GetComponent<AnimatingModel>();
		if ((bool)animatingProp)
		{
			float delay = animatingProp.Play(_transitionToUnmount, loop: false);
			if (base.isActiveAndEnabled)
			{
				KUtility.DelayedCall(this, delegate
				{
					animatingProp.Play(_standUnmountedMotion);
				}, delay);
			}
		}
		if (base.IsLocalPlayers)
		{
			SetBattleZoomView(on: false);
			Singleton<CatapultRangeView>.Instance().Hide();
		}
	}

	private void SetBattleZoomView(bool on)
	{
		CombatGroup combatGroup = UIManager.FindScript<CombatGroup>();
		combatGroup.SetBattleView(on ? CombatGroup.BattleViewMode.Mount : CombatGroup.BattleViewMode.Normal);
	}

	public override void ContextActionFinder(List<InteractionMenuData> result)
	{
	}

	private Messages.CatapultState GetCatapultState()
	{
		if (!GetArtifact().ArtifactState.Catapult.HasValue)
		{
			Debug.LogError("CatapultState must be exist in artifact");
			return default(Messages.CatapultState);
		}
		return GetArtifact().ArtifactState.Catapult.Value;
	}

	private void GetRange(out float range, out float deadzone)
	{
		Messages.CatapultState catapultState = GetCatapultState();
		range = catapultState.AtkRangeMax;
		deadzone = catapultState.AtkRangeMin;
	}

	private void SetState(CatapultState state)
	{
		switch (state)
		{
		case CatapultState.Stand:
			Singleton<CatapultRangeView>.Instance().ShowWave(show: true);
			break;
		case CatapultState.Turn:
		case CatapultState.Shoot:
			Singleton<CatapultRangeView>.Instance().ShowWave(show: false);
			break;
		}
	}

	public void FireProjectile(VehicleProjectileFired msg)
	{
		this.StartCoroutine(ref _lastAttackCoroutine, CoAttack(msg));
	}

	public double GetServerTime()
	{
		return Connections.Frontend.GetBufferedServerTime();
	}

	private void Update()
	{
		ProjectileController.UpdateProjectiles();
	}

	public void TurnToYaw(float yaw)
	{
		_bodyTransform.localRotation = Quaternion.Euler(0f, Maths.NormalizeAngDeg(yaw + _yawBias), 0f);
	}

	public void UpdateRemainedProjectiles()
	{
		Artifact artifact = GetArtifact();
		if (!(artifact == null) && artifact.ArtifactState.Catapult.HasValue && !(_magazine == null))
		{
			int remainedProjectilesSize = artifact.ArtifactState.Catapult.Value.RemainedProjectilesSize;
			if (remainedProjectilesSize == 0)
			{
				_magazine.UpdateMagazine(CatapultMagazine.Quantity.None);
			}
			else if (remainedProjectilesSize < _thresholdNumProfectileFullModel)
			{
				_magazine.UpdateMagazine(CatapultMagazine.Quantity.Many);
			}
			else
			{
				_magazine.UpdateMagazine(CatapultMagazine.Quantity.Full);
			}
			if (_numRemainedProjectiles.HasValue && _numRemainedProjectiles.Value < remainedProjectilesSize)
			{
				SoundManager.PlayEvent("sfx_catapult_insert", SoundPosition.Fix(artifact.InteractionPosition));
			}
			_numRemainedProjectiles = remainedProjectilesSize;
		}
	}

	private IEnumerator CoAttack(VehicleProjectileFired msg)
	{
		Artifact artifact = GetArtifact();
		if (artifact == null)
		{
			yield break;
		}
		AnimatingModel animatingProp = GetComponent<AnimatingModel>();
		double beginAt = GetServerTime();
		Quaternion origRot = _bodyTransform.localRotation;
		Vector3 targetPos = msg.TargetPosition.ToClientPosition();
		float destYaw = Maths.NormalizeAngDeg(Maths.CalcYawWithTarget(targetPos, artifact.Center) + _yawBias);
		Quaternion destRot = Quaternion.Euler(0f, destYaw, 0f);
		uint soundInstanceId = SoundManager.PlayEvent("sfx_catapult_revolve_start", SoundPosition.Fix(artifact.Center));
		if ((bool)animatingProp)
		{
			animatingProp.Play(_turnMotion);
		}
		while (!(beginAt >= msg.PrepareAnimAt))
		{
			double ratio = (GetServerTime() - beginAt) / (msg.PrepareAnimAt - beginAt);
			_bodyTransform.localRotation = Quaternion.Lerp(origRot, destRot, Mathf.Clamp01((float)ratio));
			if (ratio >= 1.0)
			{
				break;
			}
			yield return null;
		}
		SoundManager.PlayEvent(soundInstanceId, "sfx_catapult_revolve_end");
		if ((bool)animatingProp)
		{
			animatingProp.Play(_prepareAttackMotion, loop: false);
			yield return new WaitForSeconds((float)(msg.FireAnimAt - GetServerTime()));
			float duration = animatingProp.Play(_attackMotion, loop: false);
			float shootDelay = (float)(msg.ShootAt - GetServerTime());
			yield return new WaitForSeconds(shootDelay);
			float firingDuration = (float)(msg.DmgAt - GetServerTime());
			float projectileSpeed = Maths.Make2D(targetPos - _attachmentProjectile.position).magnitude / firingDuration;
			ProjectileController.ModifyProjectileSpeed(projectileSpeed);
			ProjectileController.SetTarget(targetPos, missed: false);
			ProjectileController.ShootProjectile();
			yield return new WaitForSeconds(duration - shootDelay);
			animatingProp.Play(_standMountedMotion);
		}
	}
}
