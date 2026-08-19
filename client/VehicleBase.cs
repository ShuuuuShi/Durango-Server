using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Render.Camera;
using Durango.Utils;
using InteractionData;
using Messages;
using Shared.Battle;
using UnityEngine;

public abstract class VehicleBase : MonoBehaviour
{
	[SerializeField]
	private float _rotateSpeed = 200f;

	[SerializeField]
	private float _cameraHeight = 300f;

	[SerializeField]
	private string _attachmentName = "Attachment_Mount_01";

	[SerializeField]
	private float _spinePitchAtRiding = -120f;

	public virtual string EntityId => string.Empty;

	public virtual bool IsMovable => true;

	public virtual bool IsRemoveWithDriver => false;

	public virtual float MoveSpeed { get; set; }

	public float RotateSpeed => _rotateSpeed;

	public float CameraHeight => _cameraHeight;

	protected bool IsRidingMe => (bool)Driver && Driver.IsRiding && Driver.Vehicle == this;

	public virtual bool IsAlive => true;

	public abstract string Name { get; }

	public string OwnerName => (!Driver) ? string.Empty : Driver.DriverName;

	public bool IsLocalPlayers => (bool)Driver && Driver.IsLocalPlayer;

	public abstract bool IgnoreWaterFlow { get; }

	protected Driver Driver { get; private set; }

	public virtual bool IsHovering => false;

	public virtual Vector3 CameraPanOffset => Vector3.zero;

	public float SpinePitchAtRiding => _spinePitchAtRiding;

	public virtual bool EnableRidingStabilizer => true;

	public virtual void SetDriver(Driver driver)
	{
		Driver = driver;
	}

	public virtual bool SetupSaddle(bool setupSaddle = true, bool setupHelmet = true, bool hasRope = false)
	{
		return false;
	}

	public virtual void AttachDriver(Driver driver, GameObject saddle = null)
	{
		Driver = driver;
		bool isLocalPlayer = Driver.IsLocalPlayer;
		if (SetupSaddle(setupSaddle: true, setupHelmet: true, isLocalPlayer))
		{
			return;
		}
		if (saddle == null)
		{
			saddle = base.gameObject;
		}
		Transform transform = KUtility.FindTransformByName(saddle, _attachmentName);
		if (null == transform)
		{
			Debug.LogError("No " + _attachmentName + " in " + saddle);
			return;
		}
		Vector3 lossyScale = transform.transform.parent.lossyScale;
		float x = driver.transform.localScale.x;
		transform.transform.localScale = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z) * x;
		driver.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
		driver.transform.rotation = base.transform.rotation;
		if (IsMovable)
		{
			base.transform.parent = driver.transform;
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			CharacterBehavior component = GetComponent<CharacterBehavior>();
			if (component != null)
			{
				component.TurnToYaw(0f, bSnap: true);
			}
		}
		Transform bodyPartTransform = driver.GetBodyPartTransform(BodyPart.Body);
		bodyPartTransform.parent = transform;
		bodyPartTransform.localPosition = Vector3.zero;
	}

	public virtual void DetachDriver()
	{
		if (!(Driver == null))
		{
			if (IsMovable)
			{
				base.transform.parent = null;
			}
			Transform bodyPartTransform = Driver.GetBodyPartTransform(BodyPart.Body);
			CharacterBehavior component = Driver.GetComponent<CharacterBehavior>();
			bodyPartTransform.parent = ((!component) ? Driver.transform : component.MeshObjectTransform);
		}
	}

	public virtual float GetVehicleMotionLength()
	{
		return 0f;
	}

	public void InteractionTouched()
	{
		KUtility.DelayedCall(this, MakeInteractionMenuList, 0.1f);
	}

	private void MakeInteractionMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		AddInteractionMenus(menuList);
		menuList.Apply();
		Singleton<CameraController>.Instance().Target(base.gameObject, 0.3f);
	}

	protected virtual void AddInteractionMenus(InteractionMenuList menuList)
	{
		menuList.Name = Name;
	}

	public virtual bool ReserveUnmount(Action onFinishUnmount = null)
	{
		return false;
	}

	public void RemoveVehicle()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public virtual Vector3 CalcPosBiasForChunkUpdate()
	{
		return Vector3.zero;
	}

	public static void RequestUnmountIfRiding(bool immediately = false, Action onFinishUnmount = null)
	{
		if (PlayerBehavior.LocalPlayer.IsRiding)
		{
			Connections.Frontend.Send(default(Unmount));
			if (immediately)
			{
				PlayerBehavior.LocalPlayer.Driver.Unmount(onFinishUnmount);
				return;
			}
		}
		onFinishUnmount?.Invoke();
	}

	public abstract void ContextActionFinder(List<InteractionMenuData> result);
}
