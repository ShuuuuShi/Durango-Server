using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

	[CompilerGenerated]
	private sealed class _003CCoAttack_003Ed__50 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public VehicleCatapult _003C_003E4__this;

		public VehicleProjectileFired msg;

		private AnimatingModel _003CanimatingProp_003E5__2;

		private double _003CbeginAt_003E5__3;

		private Quaternion _003CorigRot_003E5__4;

		private Vector3 _003CtargetPos_003E5__5;

		private Quaternion _003CdestRot_003E5__6;

		private uint _003CsoundInstanceId_003E5__7;

		private float _003Cduration_003E5__8;

		private float _003CshootDelay_003E5__9;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoAttack_003Ed__50(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CanimatingProp_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			VehicleCatapult vehicleCatapult = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				Artifact artifact = vehicleCatapult.GetArtifact();
				if (artifact == null)
				{
					return false;
				}
				_003CanimatingProp_003E5__2 = vehicleCatapult.GetComponent<AnimatingModel>();
				_003CbeginAt_003E5__3 = vehicleCatapult.GetServerTime();
				_003CorigRot_003E5__4 = vehicleCatapult._bodyTransform.localRotation;
				_003CtargetPos_003E5__5 = msg.TargetPosition.ToClientPosition();
				float y = Maths.NormalizeAngDeg(Maths.CalcYawWithTarget(_003CtargetPos_003E5__5, artifact.Center) + vehicleCatapult._yawBias);
				_003CdestRot_003E5__6 = Quaternion.Euler(0f, y, 0f);
				_003CsoundInstanceId_003E5__7 = SoundManager.PlayEvent("sfx_catapult_revolve_start", SoundPosition.Fix(artifact.Center));
				if ((bool)_003CanimatingProp_003E5__2)
				{
					_003CanimatingProp_003E5__2.Play(vehicleCatapult._turnMotion);
				}
				goto IL_016b;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_016b;
			case 2:
				_003C_003E1__state = -1;
				_003Cduration_003E5__8 = _003CanimatingProp_003E5__2.Play(vehicleCatapult._attackMotion, loop: false);
				_003CshootDelay_003E5__9 = (float)(msg.ShootAt - vehicleCatapult.GetServerTime());
				_003C_003E2__current = new WaitForSeconds(_003CshootDelay_003E5__9);
				_003C_003E1__state = 3;
				return true;
			case 3:
			{
				_003C_003E1__state = -1;
				float num2 = (float)(msg.DmgAt - vehicleCatapult.GetServerTime());
				float speed = Maths.Make2D(_003CtargetPos_003E5__5 - vehicleCatapult._attachmentProjectile.position).magnitude / num2;
				vehicleCatapult.ProjectileController.ModifyProjectileSpeed(speed);
				vehicleCatapult.ProjectileController.SetTarget(_003CtargetPos_003E5__5, missed: false);
				vehicleCatapult.ProjectileController.ShootProjectile();
				_003C_003E2__current = new WaitForSeconds(_003Cduration_003E5__8 - _003CshootDelay_003E5__9);
				_003C_003E1__state = 4;
				return true;
			}
			case 4:
				{
					_003C_003E1__state = -1;
					_003CanimatingProp_003E5__2.Play(vehicleCatapult._standMountedMotion);
					break;
				}
				IL_016b:
				if (!(_003CbeginAt_003E5__3 >= msg.PrepareAnimAt))
				{
					double num3 = (vehicleCatapult.GetServerTime() - _003CbeginAt_003E5__3) / (msg.PrepareAnimAt - _003CbeginAt_003E5__3);
					vehicleCatapult._bodyTransform.localRotation = Quaternion.Lerp(_003CorigRot_003E5__4, _003CdestRot_003E5__6, Mathf.Clamp01((float)num3));
					if (!(num3 >= 1.0))
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
				}
				SoundManager.PlayEvent(_003CsoundInstanceId_003E5__7, "sfx_catapult_revolve_end");
				if ((bool)_003CanimatingProp_003E5__2)
				{
					_003CanimatingProp_003E5__2.Play(vehicleCatapult._prepareAttackMotion, loop: false);
					_003C_003E2__current = new WaitForSeconds((float)(msg.FireAnimAt - vehicleCatapult.GetServerTime()));
					_003C_003E1__state = 2;
					return true;
				}
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
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
		Singleton<AssetBundleManager>.Instance().RequestAsset(_magazinePrefabPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, _attachmentMagazine.position, Quaternion.identity) as GameObject;
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
		UIManager.FindScript<CombatGroup>().SetBattleView(on ? CombatGroup.BattleViewMode.Mount : CombatGroup.BattleViewMode.Normal);
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoAttack_003Ed__50(0)
		{
			_003C_003E4__this = this,
			msg = msg
		};
	}
}
