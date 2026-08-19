using System;
using Durango.Model;
using Durango.Render.Camera;
using Durango.Utils;
using JetBrains.Annotations;
using Shared.Battle;
using Shared.Teleport;
using UnityEngine;

public class Driver : Rider
{
	private BoneLookAtTarget _lookAtController;

	private VehicleBase _vehicle;

	private string _driverName;

	[CanBeNull]
	public VehicleBase Vehicle
	{
		get
		{
			return _vehicle;
		}
		private set
		{
			if (_vehicle != value)
			{
				_vehicle = value;
				if (this.VehicleChanged != null)
				{
					this.VehicleChanged();
				}
			}
		}
	}

	public float MoveSpeed
	{
		get
		{
			if (Vehicle != null)
			{
				return Vehicle.MoveSpeed;
			}
			return 0f;
		}
	}

	public float RotateSpeed
	{
		get
		{
			if (Vehicle != null)
			{
				return Vehicle.RotateSpeed;
			}
			return 0f;
		}
	}

	public float CameraHeight
	{
		get
		{
			if (Vehicle != null)
			{
				return Vehicle.CameraHeight;
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
				PlayerBehavior component = GetComponent<PlayerBehavior>();
				if (component != null)
				{
					_driverName = component.PlayerName;
				}
			}
			return _driverName;
		}
	}

	public bool IsWaitForUnmountMotionFinish { get; private set; }

	public bool IsHovering
	{
		get
		{
			if (base.IsRiding && Vehicle != null)
			{
				return Vehicle.IsHovering;
			}
			return false;
		}
	}

	public bool IsWarpAllowed
	{
		get
		{
			if (base.IsRiding)
			{
				return IsRidingKindOf<VehiclePet>();
			}
			return true;
		}
	}

	public Vector3 CameraPanOffset
	{
		get
		{
			if (base.IsRiding && Vehicle != null)
			{
				return Vehicle.CameraPanOffset;
			}
			return Vector3.zero;
		}
	}

	public bool IsLocalPlayer
	{
		get
		{
			PlayerBehavior playerBehavior = base.Owner as PlayerBehavior;
			if (playerBehavior != null)
			{
				return playerBehavior.IsLocalPlayer;
			}
			return false;
		}
	}

	public float SpinePitchAtRiding
	{
		get
		{
			if (Vehicle != null)
			{
				return Vehicle.SpinePitchAtRiding;
			}
			return -120f;
		}
	}

	public event Action VehicleChanged;

	private void Awake()
	{
		_lookAtController = base.gameObject.GetComponent<BoneLookAtTarget>();
	}

	private void OnDestroy()
	{
		if (Vehicle != null && Vehicle.IsRemoveWithDriver)
		{
			Vehicle.RemoveVehicle();
		}
	}

	public bool IsVehicleKindOf<T>()
	{
		if (Vehicle != null)
		{
			return Vehicle is T;
		}
		return false;
	}

	public bool IsRidingKindOf<T>()
	{
		if (base.IsRiding)
		{
			return IsVehicleKindOf<T>();
		}
		return false;
	}

	public Vector3 CalcCameraOrigin(Vector3 origin)
	{
		if (!base.IsRiding)
		{
			return origin;
		}
		origin.y += CameraHeight;
		origin += CameraPanOffset;
		return origin;
	}

	public void SetVehicle(VehicleBase target, bool playSpawnMotion)
	{
		Vehicle = target;
		if (Vehicle != null)
		{
			Vehicle.SetDriver(this);
			VehicleBase vehicle = Vehicle;
			bool isLocalPlayer = IsLocalPlayer;
			vehicle.SetupSaddle(setupSaddle: true, setupHelmet: true, isLocalPlayer);
		}
		if (playSpawnMotion)
		{
			PlayMotionIfLocalPlayer("Ride_Whistle");
		}
	}

	public void Mount(VehicleBase target, Action onFinishMount = null)
	{
		if (!(null == target) && !base.IsRiding)
		{
			Vehicle = target;
			PetAI component = Vehicle.GetComponent<PetAI>();
			if (component != null)
			{
				component.BeginRide();
			}
			if (IsLocalPlayer)
			{
				Singleton<CameraController>.Instance().Target(Vehicle.gameObject, 0.3f);
				PlayerController.MotionUpdater.Mount();
			}
			base.Owner.RootMotionMovable.SetInPlaceMotionMode(isInPlaceMotion: true);
			Vehicle.AttachDriver(this);
			Singleton<CameraController>.Instance().Target(null, 0.3f);
			if (IsLocalPlayer)
			{
				Singleton<PlayerController>.Instance().TurnToYaw(Vehicle.transform.rotation.eulerAngles.y, snap: true);
			}
			else
			{
				base.Owner.TurnToYaw(Vehicle.transform.rotation.eulerAngles.y, bSnap: true);
			}
			base.IsRiding = true;
			if ((bool)_lookAtController)
			{
				_lookAtController.SetLookTarget(null);
				_lookAtController.AutoChangeTarget = false;
			}
			PlayerBehavior component2 = base.Owner.GetComponent<PlayerBehavior>();
			if ((bool)component2)
			{
				component2.SetEquipmentVisible(visible: false);
				component2.PlaneShadowEnabled = false;
			}
			onFinishMount?.Invoke();
		}
	}

	private void PlayMotionIfLocalPlayer(string motion, bool forceTransition = false)
	{
		if (IsLocalPlayer)
		{
			LocalMotionUpdater motionUpdater = PlayerController.MotionUpdater;
			bool forceTransition2 = forceTransition;
			motionUpdater.Motion(motion, 0f, 1f, forceTransition2);
		}
	}

	public void Unmount(Action onFinishUnmount = null, bool immediately = false)
	{
		if (Vehicle == null || !base.IsRiding || IsWaitForUnmountMotionFinish)
		{
			if (onFinishUnmount != null)
			{
				onFinishUnmount();
			}
			return;
		}
		IsWaitForUnmountMotionFinish = false;
		float num = -1f;
		if (IsLocalPlayer)
		{
			PlayerController.MotionUpdater.DisMount(immediately);
			if (!immediately)
			{
				Singleton<CameraController>.Instance().Target(PlayerBehavior.LocalPlayer.CameraOrigin, 0.3f);
				num = PlayerController.MotionUpdater.GetDisMountMotionLength() - 0.1f;
				GameSystem<InputSystem>.Instance().MoveLock = true;
			}
		}
		if (Vehicle.IsMovable)
		{
			Vehicle.transform.parent = null;
		}
		if (num > 0f)
		{
			IsWaitForUnmountMotionFinish = true;
		}
		VehicleBase lastVehicle = Vehicle;
		KUtility.DelayedCall(this, delegate
		{
			if (lastVehicle != null)
			{
				lastVehicle.DetachDriver();
				PetAI component = lastVehicle.GetComponent<PetAI>();
				if (component != null)
				{
					component.EndRide();
				}
			}
			PlayerBehavior playerBehavior = base.Owner as PlayerBehavior;
			if (IsLocalPlayer)
			{
				Singleton<CameraController>.Instance().Target(null, 1f);
				if (!immediately)
				{
					GameSystem<InputSystem>.Instance().MoveLock = false;
					CharacterBehavior characterBehavior = ((!(lastVehicle != null)) ? null : lastVehicle.GetComponent<CharacterBehavior>());
					Vector3 vector = ((!(characterBehavior != null)) ? base.Owner.GetSidePos(left: true, 2f) : characterBehavior.GetSidePos(left: true, 2f));
					if (Singleton<PlayerController>.Instance().IsMovablePosition(vector))
					{
						Singleton<PlayerController>.Instance().Teleport(vector, TeleportType.Unknown, instance: true);
					}
				}
			}
			base.IsRiding = false;
			base.Owner.RootMotionMovable.SetInPlaceMotionMode(isInPlaceMotion: false);
			PlayMotionIfLocalPlayer("Stand", immediately);
			if ((bool)playerBehavior)
			{
				playerBehavior.SetEquipmentVisible(visible: true);
				playerBehavior.PlaneShadowEnabled = true;
			}
			if ((bool)_lookAtController)
			{
				_lookAtController.AutoChangeTarget = true;
			}
			if (onFinishUnmount != null)
			{
				onFinishUnmount();
			}
			IsWaitForUnmountMotionFinish = false;
		}, num);
	}

	public bool ReserveUnmount(Action onFinishUnmount = null)
	{
		if (null == Vehicle || !base.IsRiding || IsWaitForUnmountMotionFinish)
		{
			return false;
		}
		return Vehicle.ReserveUnmount(onFinishUnmount);
	}

	public void ReturnVehicle(bool playReturnMotion, Action onReturn = null)
	{
		if (base.IsRiding)
		{
			Unmount(delegate
			{
				ReturnVehicleInternal(playReturnMotion, onReturn);
			});
		}
		else
		{
			ReturnVehicleInternal(playReturnMotion, onReturn);
		}
	}

	private void ReturnVehicleInternal(bool playReturnMotion, Action onReturn)
	{
		if (playReturnMotion)
		{
			PlayMotionIfLocalPlayer("Ride_SendBack");
		}
		onReturn?.Invoke();
		Vehicle = null;
	}

	public Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, Vector3 nearPos = default(Vector3))
	{
		return base.Owner.GetBodyPartTransform(part, bAllowNull, nearPos);
	}

	public float GetVehicleMotionLength()
	{
		if (Vehicle == null)
		{
			return -1f;
		}
		return Vehicle.GetVehicleMotionLength();
	}

	public Vector3 CalcPosBiasForChunkUpdate()
	{
		if (!base.IsRiding || Vehicle == null)
		{
			return Vector3.zero;
		}
		return Vehicle.CalcPosBiasForChunkUpdate();
	}

	public void TransferEvent(Driver rider)
	{
		TransferEvent((Rider)rider);
		VehicleChanged += rider.VehicleChanged;
		rider.VehicleChanged = null;
	}
}
