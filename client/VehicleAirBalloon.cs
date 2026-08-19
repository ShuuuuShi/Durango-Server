using System;
using System.Collections.Generic;
using Durango.Logic.Estate;
using Durango.Render.Camera;
using Durango.Render.Screen;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.Control;
using Durango.Utils;
using InteractionData;
using L10N;
using Shared.Region;
using UnityEngine;

public class VehicleAirBalloon : VehicleProp
{
	private enum AirBalloonState
	{
		Landed,
		TakeOff,
		Landing,
		DoNotReuse,
		Farewell
	}

	private AirBalloonState _state;

	private float _stateBegunTime;

	[SerializeField]
	private float _moveSpeed = 1000f;

	[SerializeField]
	private string _localizedName;

	[SerializeField]
	private float _airBalloonZoom = 0.3f;

	private float _prevZoom;

	[SerializeField]
	private float _maxAltitude = 1500f;

	[SerializeField]
	private string _rideMotionSetName = "hot_air_balloon";

	[SerializeField]
	private float _cameraPanOffset = 700f;

	[SerializeField]
	private float _bounceHeight = 50f;

	private float _curCameraPanOffset;

	private Vector3 _lastPosBiasByMoving = Vector3.zero;

	[SerializeField]
	private float _takeoffLandingDuration = 7.5f;

	[SerializeField]
	private Vector3 _fixInertiaBoneRotation = new Vector3(0f, 90f, -90f);

	[SerializeField]
	private float _gravityForce = 3000f;

	[SerializeField]
	private float _inertiaRatio = 1f;

	private Transform _axisObjectTransform;

	private Action _onFinishUnmount;

	private bool _isHovering;

	private float TakeOffSpeed => _maxAltitude / _takeoffLandingDuration;

	public override float MoveSpeed
	{
		get
		{
			return _moveSpeed;
		}
		set
		{
			_moveSpeed = value;
		}
	}

	public override string Name => T._(_localizedName);

	public override bool IgnoreWaterFlow => true;

	public override bool IsHovering => _isHovering;

	public override Vector3 CameraPanOffset => new Vector3(_curCameraPanOffset, 0f, _curCameraPanOffset);

	private Transform AxisObjectTransform
	{
		get
		{
			if (_axisObjectTransform == null)
			{
				_axisObjectTransform = base.transform.GetChild(0);
			}
			return _axisObjectTransform;
		}
	}

	private AirBalloonState State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
			_stateBegunTime = Time.time;
			ContextActionGroupBase contextActionGroupBase = UIManager.FindScript<ContextActionGroupBase>();
			if (contextActionGroupBase != null)
			{
				contextActionGroupBase.RefreshActionList();
			}
		}
	}

	public bool OnLandingArea { get; private set; }

	public override bool IsInteractionMenuVisible => State == AirBalloonState.Landed;

	public override bool IsRemoveWithDriver => true;

	public override void AttachDriver(Driver driver, GameObject saddle = null)
	{
		if (driver.IsLocalPlayer)
		{
			SoundManager.PlayEvent("sfx_air_balloon_get_in", SoundPosition.Fix(base.transform.position));
			PlayerController.MotionUpdater.UpdateRideMotionSet(_rideMotionSetName);
		}
		base.AttachDriver(driver, saddle);
		State = AirBalloonState.TakeOff;
		SetAirballoonZoomView(on: true);
		SetHoveringMode(on: true);
	}

	public override void DetachDriver()
	{
		base.DetachDriver();
		KUtility.DelayedCall(this, delegate
		{
			State = AirBalloonState.Farewell;
		}, 1f);
	}

	public override bool ReserveUnmount(Action onFinishUnmount = null)
	{
		if (State != AirBalloonState.TakeOff)
		{
			return false;
		}
		State = AirBalloonState.Landing;
		SetAirballoonZoomView(on: false);
		GameSystem<InputSystem>.Instance().MoveLock = true;
		_onFinishUnmount = onFinishUnmount;
		Singleton<BgmManager>.Instance().LandingAirBalloon();
		return true;
	}

	private bool IsProhibitedLandingArea()
	{
		Vector3 position = base.transform.position;
		position.y = 0f;
		Biome tileBiome = Singleton<TerrainBase>.Instance().GetTileBiome(Durango.Terrain.Util.ClientPositionToWorldPosition(position));
		if (tileBiome == Biome.Invalid || Durango.Terrain.Util.IsWater(tileBiome))
		{
			return true;
		}
		CollisionParam param = Collisions.CreateCollisionParam(position, Vector3.zero);
		if (Collisions.TryCapsuleCast(param, out var raycastHit) != 0 && IsCollidable(raycastHit))
		{
			return true;
		}
		EstateInfo estateInfo = EstateSystem.GetEstateInfo(new Point2(Durango.Terrain.Util.ClientPositionToTilePosition(position)));
		return estateInfo != null;
	}

	public void StartInTheAir()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = _maxAltitude;
		base.transform.localPosition = localPosition;
	}

	private bool IsCollidable(RaycastHit hit)
	{
		if (hit.collider.gameObject == base.gameObject)
		{
			return false;
		}
		return base.Driver == null || hit.collider.gameObject != base.Driver.gameObject;
	}

	private void SetAirballoonZoomView(bool on)
	{
		if (base.IsLocalPlayers)
		{
			if (on)
			{
				_prevZoom = Singleton<MainCamera>.Instance().Zoom;
				Singleton<CameraController>.Instance().ZoomRange(_airBalloonZoom, _airBalloonZoom, _takeoffLandingDuration).Zoom(_airBalloonZoom, _takeoffLandingDuration);
			}
			else
			{
				Singleton<CameraController>.Instance().ZoomRange(0.42f, 2.2f, _takeoffLandingDuration).Zoom(_prevZoom, _takeoffLandingDuration);
			}
			Singleton<CustomColorCorrectionEffect>.Instance().SetAtmosphereEffect(on);
		}
	}

	private void SetHoveringMode(bool on)
	{
		if (base.IsLocalPlayers)
		{
			_isHovering = on;
			VisibleController.Hide(VisibleType.HideToHover, on, "AirBalloon", (!on) ? 0.2f : 0f);
			GameSystem<InputSystem>.Instance().Touch.IgnoreTouchPicking = on;
			Singleton<CameraController>.Instance().LockZoomControl(on);
			GameSystem<InteractionSystem>.Instance().ToggleIgnoreInteraction(InteractionSystem.IgnoreInteractionFlags.HoveringAirBalloon, on);
			UIManager.FindScript<PlayerHudGroupBase>().SetAirballoon(on, this);
			UIManager.FindScript<MenuListGroupBase>().SetAirballoonMode(on);
			if (on)
			{
				UIManager.FindScript<ScreenCaptureGroup>().ToggleScreenEffect(ScreenCapture.EffectEnum.Tilt, on: false);
			}
			else
			{
				UIManager.FindScript<ScreenCaptureGroup>().ResetScreenEffects();
			}
		}
	}

	private void Update()
	{
		float num = 0f;
		AirBalloonState state = State;
		if (state == AirBalloonState.TakeOff || state == AirBalloonState.Farewell)
		{
			num = _maxAltitude;
		}
		float num2 = (Mathf.Sin(Time.time) + 1f) * 0.5f;
		num += num2 * _bounceHeight;
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = Mathf.MoveTowards(base.transform.localPosition.y, num, Time.deltaTime * TakeOffSpeed);
		base.transform.localPosition = localPosition;
		float num3 = localPosition.y / _maxAltitude;
		_curCameraPanOffset = Mathf.Lerp(0f, _cameraPanOffset, num3);
		ApplyInertia();
		float num4 = Time.time - _stateBegunTime;
		switch (State)
		{
		case AirBalloonState.Landing:
			if ((num3 < 0.05f || num4 >= _takeoffLandingDuration) && base.IsRidingMe)
			{
				SetHoveringMode(on: false);
				GameSystem<InputSystem>.Instance().MoveLock = false;
				State = AirBalloonState.DoNotReuse;
				base.Driver.Unmount(_onFinishUnmount, immediately: true);
			}
			break;
		case AirBalloonState.Farewell:
			if (num4 >= _takeoffLandingDuration)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			break;
		}
		OnLandingArea = !IsProhibitedLandingArea();
	}

	private void ApplyInertia()
	{
		Vector3 vector = ((!base.IsRidingMe || !(base.Driver != null)) ? Vector3.zero : base.Driver.Owner.CurrentVelocity);
		Vector3 worldUp = -Vector3.right;
		Vector3 worldPosition = base.transform.position - vector - Vector3.up * _gravityForce;
		Quaternion rotation = AxisObjectTransform.rotation;
		AxisObjectTransform.LookAt(worldPosition, worldUp);
		AxisObjectTransform.Rotate(_fixInertiaBoneRotation);
		AxisObjectTransform.rotation = Quaternion.Lerp(rotation, AxisObjectTransform.rotation, _inertiaRatio * Time.deltaTime);
	}

	protected override void AddInteractionMenus(InteractionMenuList menuList)
	{
		menuList.Name = Name;
		if (IsInteractionMenuVisible && !base.IsRidingMe)
		{
			menuList.Add(new InteractionMenuData(Interaction.MountAirBalloon));
		}
	}

	public override Vector3 CalcPosBiasForChunkUpdate()
	{
		Vector3 vector = base.Driver.CameraPanOffset * 1.3f;
		if (base.Driver.Owner.CurrentVelocity.magnitude > 10f)
		{
			_lastPosBiasByMoving = base.Driver.Owner.CurrentVelocity.normalized * 3200f * 0.3f;
		}
		return vector + _lastPosBiasByMoving;
	}

	public override void ContextActionFinder(List<InteractionMenuData> result)
	{
		if (!GameSystem<InputSystem>.Instance().MoveLock && State != AirBalloonState.Landing && State != AirBalloonState.DoNotReuse && State != AirBalloonState.Farewell)
		{
			if (base.IsRidingMe)
			{
				result.Add(Interaction.DismountAirBalloon);
				result.Add(Interaction.CaptureScreenShot);
			}
			else
			{
				result.Add(Interaction.MountAirBalloon);
			}
		}
	}

	public static void Spawn(Vector3 position, Action<VehicleAirBalloon> loaded)
	{
		Singleton<AssetBundleManager>.Instance().RequestAsset("Models/Prop/vehicle/hot_airballoon/hot_airballoon_01.prefab", typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(asset, Maths.Make2D(position), Quaternion.identity) as GameObject;
				if (!(gameObject == null))
				{
					VehicleAirBalloon component = gameObject.GetComponent<VehicleAirBalloon>();
					if (!(component == null))
					{
						loaded(component);
					}
				}
			}
		});
	}
}
