using System;
using System.Runtime.InteropServices;
using Shared.Battle;
using UnityEngine;

public class Driver : Rider
{
	[SerializeField]
	private string _runMountMotion = "Ride_Run_Mount";

	[SerializeField]
	private string _standMountMotion = "Ride_Stand_Mount";

	private Transform _spineTransform;

	private BoneLookAtTarget _lookAtController;

	private Vehicle _vehicle;

	private string _driverName;

	private bool _isWaitForUnmountFinish;

	public Vehicle Vehicle => _vehicle;

	public float MoveSpeed
	{
		get
		{
			if (Object.op_Implicit((Object)(object)_vehicle))
			{
				return _vehicle.MoveSpeed;
			}
			return 0f;
		}
	}

	public float RotateSpeed
	{
		get
		{
			if (Object.op_Implicit((Object)(object)_vehicle))
			{
				return _vehicle.RotateSpeed;
			}
			return 0f;
		}
	}

	public float CameraHeight
	{
		get
		{
			if (Object.op_Implicit((Object)(object)_vehicle))
			{
				return _vehicle.CameraHeight;
			}
			return 0f;
		}
	}

	public string DriverName
	{
		get
		{
			if (string.IsNullOrEmpty(_driverName))
			{
				PlayerBehavior component = ((Component)this).GetComponent<PlayerBehavior>();
				if ((Object)(object)component != (Object)null)
				{
					_driverName = component.PlayerName;
				}
			}
			return _driverName;
		}
	}

	private void Start()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_spineTransform = base.Owner.GetBodyPartTransform(BodyPart.Back);
		_lookAtController = ((Component)this).gameObject.GetComponent<BoneLookAtTarget>();
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)_vehicle))
		{
			_vehicle.RemoveVehicle();
		}
	}

	public void UseVehicle(Vehicle target, bool playSpawnMotion)
	{
		_vehicle = target;
		_vehicle.SetDriver(this);
		_vehicle.SetupSaddle();
		if (playSpawnMotion)
		{
			PlayMotionIfLocalPlayer("Ride_Whistle");
		}
	}

	public void Mount(Vehicle target, bool startup = false)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)null == (Object)(object)target) && !base.IsRiding)
		{
			_vehicle = target;
			if ((Object)(object)((Component)this).gameObject == (Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject)
			{
				KSingleton<CameraController>.Instance().SetCameraTarget(((Component)_vehicle).gameObject);
			}
			_vehicle.AttachDriver(this);
			CharacterBehavior owner = base.Owner;
			Quaternion rotation = ((Component)_vehicle).transform.rotation;
			owner.TurnToYaw(((Quaternion)(ref rotation)).eulerAngles.y, bSnap: true);
			PetAI component = ((Component)_vehicle).GetComponent<PetAI>();
			if ((Object)(object)component != (Object)null)
			{
				component.BeginRide();
			}
			base.IsRiding = true;
			if (Object.op_Implicit((Object)(object)_lookAtController))
			{
				_lookAtController.SetLookTarget(null);
				_lookAtController.AutoChangeTarget = false;
			}
			PlayerBehavior component2 = ((Component)base.Owner).GetComponent<PlayerBehavior>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.SetWeaponVisible(visible: false);
			}
			RidingStabilizer component3 = ((Component)this).gameObject.GetComponent<RidingStabilizer>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				((Behaviour)component3).enabled = true;
				component3.SetMountHeadBone(_spineTransform);
			}
			if (startup)
			{
				UpdateMovingMotion(isMoving: false, updatePlayerMotion: true);
				return;
			}
			PlayMotionIfLocalPlayer("RideMount", isSampleImmediately: true);
			UpdateMovingMotion(isMoving: false, updatePlayerMotion: false);
		}
	}

	private void PlayMotionIfLocalPlayer(string motion, bool isSampleImmediately = false)
	{
		if ((Object)(object)((Component)this).gameObject == (Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject)
		{
			KSingleton<PlayerController>.Instance().Motion(motion, 0f, 1f, forceTransition: true);
			if (isSampleImmediately)
			{
				PlayerBehavior.LocalPlayer.SampleAnimImmediately();
			}
		}
	}

	public void Unmount(Action onFinishUnmount = null)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)null == (Object)(object)_vehicle || !base.IsRiding || _isWaitForUnmountFinish)
		{
			if (onFinishUnmount != null)
			{
				onFinishUnmount();
			}
			return;
		}
		_isWaitForUnmountFinish = false;
		float num = -1f;
		if ((Object)(object)((Component)this).gameObject == (Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject)
		{
			PlayMotionIfLocalPlayer("RideDismount", isSampleImmediately: true);
			KSingleton<CameraController>.Instance().SetCameraTargetPos(PlayerBehavior.LocalPlayer.InteractionPosition);
			num = PlayerBehavior.LocalPlayer.Anim[PlayerBehavior.LocalPlayer.CurrentAnimClipName].length - 0.1f;
		}
		((Component)_vehicle).transform.parent = null;
		UpdateMovingMotion(isMoving: false, updatePlayerMotion: false);
		RidingStabilizer component = ((Component)this).gameObject.GetComponent<RidingStabilizer>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((Behaviour)component).enabled = false;
		}
		if (num > 0f)
		{
			_isWaitForUnmountFinish = true;
		}
		KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
		{
			if ((Object)(object)((Component)this).gameObject == (Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject)
			{
				KSingleton<CameraController>.Instance().ResetCameraTarget(1f);
			}
			_vehicle.DetachDriver(this);
			PetAI component2 = ((Component)_vehicle).GetComponent<PetAI>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.EndRide();
			}
			base.IsRiding = false;
			PlayMotionIfLocalPlayer("Stand", isSampleImmediately: true);
			PlayerBehavior component3 = ((Component)base.Owner).GetComponent<PlayerBehavior>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				component3.AnimationRefresh(forceRefresh: true);
				component3.SetWeaponVisible(visible: true);
			}
			if (Object.op_Implicit((Object)(object)_lookAtController))
			{
				_lookAtController.AutoChangeTarget = true;
			}
			if (onFinishUnmount != null)
			{
				onFinishUnmount();
			}
			_isWaitForUnmountFinish = false;
		}, num);
	}

	public void ReturnVehicle(bool playReturnMotion)
	{
		if ((Object)null == (Object)(object)_vehicle)
		{
			return;
		}
		Vehicle lastVehicle = _vehicle;
		if (base.IsRiding)
		{
			Unmount(delegate
			{
				ReturnVehicleInternal(playReturnMotion, lastVehicle);
			});
		}
		else
		{
			ReturnVehicleInternal(playReturnMotion, lastVehicle);
		}
	}

	private void ReturnVehicleInternal(bool playReturnMotion, Vehicle lastVehicle)
	{
		_vehicle = null;
		PetAI aiPet = ((Component)lastVehicle).GetComponent<PetAI>();
		if (!((Object)(object)aiPet != (Object)null))
		{
			return;
		}
		KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
		{
			if (playReturnMotion)
			{
				PlayMotionIfLocalPlayer("Ride_SendBack");
			}
			aiPet.Return();
		}, 0.5f);
	}

	public Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, [Optional] Vector3 nearPos)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return base.Owner.GetBodyPartTransform(part, bAllowNull, nearPos);
	}

	public void UpdateMovingMotion(bool isMoving, bool updatePlayerMotion)
	{
		float num = -1f;
		if (Object.op_Implicit((Object)(object)_vehicle))
		{
			_vehicle.UpdateMovingMotion(isMoving);
			num = _vehicle.MotionPlayable.GetCurAnimState().length;
		}
		if (!updatePlayerMotion)
		{
			return;
		}
		PlayerBehavior component = ((Component)base.Owner).GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)component))
		{
			string text = ((!isMoving) ? _standMountMotion : _runMountMotion);
			float num2 = 1f;
			if (num > 0f)
			{
				PlayerAnimationClipInfo playerAnimationClipInfo = component.AnimManager.GetPlayerAnimationClipInfo(text, null);
				num2 = playerAnimationClipInfo.Length / num;
			}
			float playbackRate = num2;
			component.PlayAnimation(text, 0f, playbackRate);
		}
	}
}
