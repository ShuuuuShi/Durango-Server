using System;
using System.Collections.Generic;
using InteractionData;
using Messages;
using Shared.Battle;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
	[SerializeField]
	private string _walkMotion;

	[SerializeField]
	private string _runMotion;

	[SerializeField]
	private string _standMotion;

	[SerializeField]
	private string _idleMotion;

	[SerializeField]
	private string _eatMotion;

	[SerializeField]
	private float _eatDistance = 250f;

	[SerializeField]
	private GameObject _saddlePrefab;

	[SerializeField]
	private GameObject _helmetPrefab;

	[SerializeField]
	private string _attachmentName = "Attachment_Mount_01";

	[SerializeField]
	private float _walkSpeed = 200f;

	[SerializeField]
	private float _rotateSpeed = 200f;

	[SerializeField]
	private float _cameraHeight = 300f;

	private IMotionPlayable _motionPlayable;

	private Driver _driver;

	private GameObject _saddle;

	private GameObject _helmet;

	private bool _costumeVehicle = true;

	private BoneLookAtTarget _lookAtController;

	[ExposedInEditor(null)]
	private float _playbackRate = 1f;

	private static List<GameObject> _bufferList = new List<GameObject>();

	public float MoveSpeed { get; set; }

	public float PlaybackRate
	{
		get
		{
			return _playbackRate;
		}
		set
		{
			_playbackRate = value;
		}
	}

	public float WalkSpeed => _walkSpeed;

	public float RotateSpeed => _rotateSpeed;

	public float CameraHeight => _cameraHeight;

	public IMotionPlayable MotionPlayable
	{
		get
		{
			if (_motionPlayable == null)
			{
				_motionPlayable = KUtility.FindMotionPlayable(((Component)this).gameObject);
			}
			return _motionPlayable;
		}
	}

	public string WalkMotion => _walkMotion;

	public string RunMotion => _runMotion;

	public string StandMotion => _standMotion;

	public string IdleMotion => _idleMotion;

	public string EatMotion => _eatMotion;

	public float EatDistance => _eatDistance;

	public bool HasDriver => (Object)(object)_driver != (Object)null;

	private bool IsRiding => Object.op_Implicit((Object)(object)_driver) && _driver.IsRiding;

	public string Name
	{
		get
		{
			AnimalBehavior component = ((Component)this).GetComponent<AnimalBehavior>();
			if ((Object)(object)component != (Object)null)
			{
				return component.GetName();
			}
			return string.Empty;
		}
	}

	public string OwnerName
	{
		get
		{
			if (Object.op_Implicit((Object)(object)_driver))
			{
				return _driver.DriverName;
			}
			return string.Empty;
		}
	}

	public bool IsLocalPlayers
	{
		get
		{
			if (Object.op_Implicit((Object)(object)_driver))
			{
				return (Object)(object)((Component)_driver).gameObject == (Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject;
			}
			return false;
		}
	}

	public CharacterBehavior.SizeLevel Size
	{
		get
		{
			AnimalBehavior component = ((Component)this).GetComponent<AnimalBehavior>();
			if ((Object)(object)component != (Object)null)
			{
				return component.Size;
			}
			return CharacterBehavior.SizeLevel.Small;
		}
	}

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().InteractionMenuProcessed += delegate(InteractionMenuData menuData)
		{
			if (16 <= menuData.Action && menuData.Action <= 23)
			{
				RequestUnmountIfRiding(immediately: true);
			}
		};
		_lookAtController = ((Component)this).gameObject.GetComponent<BoneLookAtTarget>();
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

	public void SetDriver(Driver driver)
	{
		_driver = driver;
		if (Object.op_Implicit((Object)(object)_lookAtController))
		{
			_lookAtController.SetLookTarget(((Component)driver).gameObject, bFindHead: true);
			_lookAtController.AutoChangeTarget = true;
		}
	}

	public void AttachDriver(Driver driver)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		_driver = driver;
		CharacterBehavior component = ((Component)this).GetComponent<CharacterBehavior>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)("No CharacterBehavior! " + this));
		}
		else
		{
			if (SetupSaddle())
			{
				return;
			}
			Transform val = KUtility.FindTransformByName(_saddle, _attachmentName);
			if ((Object)null == (Object)(object)val)
			{
				Debug.LogError((object)("No " + _attachmentName + " in " + _saddle));
				return;
			}
			Vector3 lossyScale = ((Component)val).transform.parent.lossyScale;
			((Component)val).transform.localScale = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z);
			((Component)driver).transform.position = new Vector3(val.position.x, 0f, val.position.z);
			((Component)driver).transform.rotation = ((Component)this).transform.rotation;
			if (_costumeVehicle)
			{
				((Component)this).transform.parent = ((Component)driver).transform;
				((Component)this).transform.localPosition = Vector3.zero;
				((Component)this).transform.localRotation = Quaternion.identity;
				component.TurnToYaw(0f, bSnap: true);
			}
			if (Object.op_Implicit((Object)(object)_lookAtController))
			{
				_lookAtController.SetLookTarget(null);
				_lookAtController.AutoChangeTarget = false;
			}
			Transform bodyPartTransform = driver.GetBodyPartTransform(BodyPart.Body);
			bodyPartTransform.parent = val;
			bodyPartTransform.localPosition = Vector3.zero;
		}
	}

	public bool SetupSaddle(bool setupSaddle = true, bool setupHelmet = true)
	{
		CharacterBehavior component = ((Component)this).GetComponent<CharacterBehavior>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)("No CharacterBehavior! " + this));
			return true;
		}
		if (component.BoneMergeable == null)
		{
			Debug.LogError((object)("No BoneMergeable in " + this));
			return true;
		}
		if (setupSaddle && (Object)(object)_saddle == (Object)null)
		{
			if ((Object)(object)_saddlePrefab == (Object)null)
			{
				Debug.LogError((object)("No saddle prefab! " + this));
				return true;
			}
			_saddle = Object.Instantiate<GameObject>(_saddlePrefab);
			component.BoneMergeable.AttachBoneMerge(_saddle);
		}
		if (setupHelmet && (Object)(object)_helmet == (Object)null)
		{
			if ((Object)(object)_helmetPrefab == (Object)null)
			{
				Debug.LogError((object)("No helmet prefab! " + this));
			}
			else
			{
				_helmet = Object.Instantiate<GameObject>(_helmetPrefab);
				component.BoneMergeable.AttachBoneMerge(_helmet);
			}
		}
		return false;
	}

	public void DetachDriver(Driver driver)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (_costumeVehicle)
		{
			((Component)this).transform.parent = null;
		}
		Transform bodyPartTransform = _driver.GetBodyPartTransform(BodyPart.Body);
		CharacterBehavior component = ((Component)_driver).GetComponent<CharacterBehavior>();
		bodyPartTransform.parent = ((!Object.op_Implicit((Object)(object)component)) ? ((Component)_driver).transform : component.MeshObjectTransform);
		if (Object.op_Implicit((Object)(object)_lookAtController))
		{
			_lookAtController.SetLookTarget(((Component)driver).gameObject, bFindHead: true);
			_lookAtController.AutoChangeTarget = true;
		}
	}

	public static Vehicle GetNearestVehicle(Vector3 pos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		List<GameObject> bufferList = _bufferList;
		InteractionSystem.GetNearObjectsInternal(bufferList, LayerMask.op_Implicit(LayerHelper.DefaultMask), 2000f, (GameObject o) => (!ObjectIdentifier.IsTargetableEnemy(o, includePets: true)) ? null : o);
		float num = -1f;
		Vehicle result = null;
		int count = bufferList.Count;
		for (int i = 0; i < count; i++)
		{
			Vehicle component = bufferList[i].GetComponent<Vehicle>();
			if (!((Object)(object)component == (Object)null))
			{
				Vector3 val = ((Component)component).transform.position - pos;
				float magnitude = ((Vector3)(ref val)).magnitude;
				if (magnitude < num || num < 0f)
				{
					num = magnitude;
					result = component;
				}
			}
		}
		return result;
	}

	public void UpdateMovingMotion(bool isMoving)
	{
		MotionPlayable.CrossFade(playbackRate: PlaybackRate, motionName: (!isMoving) ? StandMotion : RunMotion);
	}

	public void InteractionTouched()
	{
		KUtility.DelayedCall((MonoBehaviour)(object)this, MakeInteractionMenuList, 0.1f);
	}

	private void MakeInteractionMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		menuList.Add(new InteractionMenuData((!IsRiding) ? Interaction.Mount : Interaction.Unmount));
		menuList.Add(new InteractionMenuData(Interaction.PetInventory));
		menuList.Add(new InteractionMenuData(Interaction.FeedPet));
		menuList.Add(new InteractionMenuData(Interaction.ReturnPet));
		menuList.Add(new InteractionMenuData(Interaction.RenamePet));
		CharacterBehavior component = ((Component)this).GetComponent<CharacterBehavior>();
		if (Object.op_Implicit((Object)(object)component))
		{
			string name = component.GetName();
			menuList.Name = name;
		}
		menuList.Apply();
		KSingleton<CameraController>.Instance().SetCameraTarget(((Component)this).gameObject);
	}

	public void RemoveVehicle()
	{
		AnimalBehavior component = ((Component)this).GetComponent<AnimalBehavior>();
		if (Object.op_Implicit((Object)(object)component) && KSingleton<AnimalManager>.HasInstance())
		{
			KSingleton<AnimalManager>.Instance().RemoveAnimal(component);
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
