using System.Collections.Generic;
using Durango.Model;
using Durango.Prologue;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Shared.Battle;
using UnityEngine;

public class CostumeActorBehavior : AnimalBehavior, IBubbleTalkable, ICostumable
{
	public const string NoneName = "_None_";

	private static ulong _nextNPCEntityId = 50000uL;

	[SerializeField]
	private string _motionName;

	[SerializeField]
	private bool _playAnimationOnStart = true;

	[SerializeField]
	private int _pauseAtFrame = -1;

	[SerializeField]
	private float _animationStartDelay = -1f;

	[SerializeField]
	private string _npcDisplayName = string.Empty;

	[SerializeField]
	private GameObject _referenceModelPrefab;

	[Tooltip("지정 색상 적용")]
	[SerializeField]
	private bool _applyStoredColorsOnStart;

	[SerializeField]
	private bool _isMale;

	[SerializeField]
	private GameObject _equipmentObj;

	[SerializeField]
	private bool _randomCostumeColor = true;

	[SerializeField]
	private List<string> _costumeNameKeys = new List<string>();

	[SerializeField]
	private List<string> _costumeNameValues = new List<string>();

	[SerializeField]
	private bool _loadPlayerClips;

	private Transform _weaponTipTransform;

	private readonly CharacterCostume _costume = new CharacterCostume();

	public override Transform WeaponTipTransform => _weaponTipTransform;

	public ItemColor[] CostumeColors => _costume.CostumeColors;

	public string SkinEffect
	{
		get
		{
			return _costume.GetSkinEffect();
		}
		set
		{
			_costume.SetSkinEffect(value);
		}
	}

	public bool IsMale
	{
		get
		{
			return _isMale;
		}
		set
		{
			_isMale = value;
		}
	}

	protected override ChatableBase CreateChatableBase()
	{
		return new ChatableNPC(this);
	}

	public bool IsTalkerVisible()
	{
		return WillBeRendered;
	}

	public Transform GetTalkBubbleTransform()
	{
		return GetBodyPartTransform(BodyPart.Head);
	}

	public string GetDisplayName()
	{
		return _npcDisplayName;
	}

	public string[] GetAnimPaths()
	{
		string arg = ((!IsMale) ? "Female" : "Male");
		return new string[1] { $"Assets/Models/Prologue/NPC/{arg}/_Anim/" };
	}

	private void TryPlayDefaultMotion()
	{
		if (string.IsNullOrEmpty(_motionName) || !_playAnimationOnStart)
		{
			return;
		}
		if (_pauseAtFrame >= 0)
		{
			float num = (float)_pauseAtFrame / 30f;
			string motionName = _motionName;
			float beginTime = num;
			Play(motionName, loop: true, beginTime, 0f);
			Anim.Sample();
		}
		else
		{
			if (_animationStartDelay < 0f)
			{
				_animationStartDelay = Random.Range(0f, 2f);
			}
			KUtility.DelayedCall(this, delegate
			{
				Play(_motionName);
			}, _animationStartDelay);
		}
	}

	public void ChangeCostume(CharacterCostume.CostumeType type, string fileName)
	{
		SetCostumeKeyValue(type.ToString(), fileName);
		_costume.ChangeCostume(type, fileName);
	}

	public string GetCostumeName(CharacterCostume.CostumeType type)
	{
		return GetCostumeKeyValue(type.ToString());
	}

	public void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color)
	{
		_costume.ChangeCostumeColor(type, color);
	}

	public ItemColor GetCostumeColor(CharacterCostume.CostumeType type)
	{
		return _costume.CostumeColors[(int)type];
	}

	public void ChangeEquipment(string path)
	{
		if (path.Length == 0)
		{
			ResetEquipment();
			return;
		}
		SetCostumeKeyValue("Equipment", path);
		Singleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), delegate(Object asset)
		{
			if (!(asset == null))
			{
				ResetEquipment();
				_equipmentObj = (GameObject)Object.Instantiate(asset);
				BoneMergeable.AttachBoneMerge(_equipmentObj);
				GameObject gameObject = KUtility.FindObjectByName(_equipmentObj, "Weapon_Tip");
				if (gameObject != null)
				{
					_weaponTipTransform = gameObject.transform;
				}
				else
				{
					Transform transform = KUtility.FindTransformByName(_equipmentObj, "Attachment_RH");
					if ((bool)transform)
					{
						gameObject = new GameObject("Weapon_Tip");
						gameObject.transform.parent = transform;
						gameObject.transform.localPosition = new Vector3(-100f, 0f, 0f);
						_weaponTipTransform = gameObject.transform;
					}
					else
					{
						_weaponTipTransform = base.transform;
					}
				}
				BoneMergeable.UpdateBoneMergeSet();
				ApplyEquipmentColor(GetStoredThreeColors("Equipment_color"));
			}
		});
	}

	public string GetEquipmentName()
	{
		return GetCostumeKeyValue("Equipment");
	}

	public void ChangeEquipmentColor(ItemColor color)
	{
		StoreCostumeColor("Equipment", color);
		ApplyEquipmentColor(color);
	}

	public ItemColor GetEquipmentColor()
	{
		return GetStoredThreeColors("Equipment");
	}

	public void ChangeAccessory(string bone, string path)
	{
		_costume.SetAccessoryModel(bone, path);
	}

	private void ApplyEquipmentColor(ItemColor color)
	{
		if (!(_equipmentObj == null))
		{
			SkinnedMeshRenderer componentInChildren = _equipmentObj.GetComponentInChildren<SkinnedMeshRenderer>();
			if (!(componentInChildren == null))
			{
				CharacterCostume.ApplyColorToRenderer(color, componentInChildren);
			}
		}
	}

	public override void LoadAnimationClips()
	{
		base.LoadAnimationClips();
		if (!_loadPlayerClips)
		{
			return;
		}
		foreach (KeyValuePair<string, AnimationClip> playerClip in PlayerManager.GetPlayerClips(_isMale))
		{
			Anim.AddClip(playerClip.Value, playerClip.Key);
		}
	}

	public override void ClearAnimationClips()
	{
		base.ClearAnimationClips();
		if (!_loadPlayerClips)
		{
			return;
		}
		foreach (KeyValuePair<string, AnimationClip> playerClip in PlayerManager.GetPlayerClips(_isMale))
		{
			if ((bool)Anim.GetClip(playerClip.Key))
			{
				Anim.RemoveClip(playerClip.Key);
			}
		}
	}

	protected new void Awake()
	{
		base.Awake();
		base.EntityId = _nextNPCEntityId.ToString();
		_nextNPCEntityId++;
		Init();
		SetActivateRootMotion(active: false);
	}

	protected new void Start()
	{
		base.Start();
		TryPlayDefaultMotion();
	}

	public void Init()
	{
		if (base.MeshObject.transform.Find("Head") == null && _referenceModelPrefab != null)
		{
			AttachHead();
		}
		_costume.Init(_isMale, base.MeshObject, GetComponent<JiggleBonesController>(), Shadows, MakeRestoreCostumeColors());
		_costume.ModelChanged += Costume_ModelChanged;
		_costume.ColorChanged += Costume_ColorChanged;
		_costume.SyncCostumeProperty(GetCostumeKeyValue("Hair"), GetCostumeKeyValue("Head"), GetCostumeKeyValue("Body"), GetCostumeKeyValue("Beard"));
		RefixBoneMerge();
		if (string.IsNullOrEmpty(_npcDisplayName))
		{
			_npcDisplayName = ((!IsMale) ? "Female" : "Male") + Random.Range(0, 99);
		}
		if (Application.isPlaying)
		{
			if (_applyStoredColorsOnStart)
			{
				UpdateStoredCostumeColors();
			}
			else if (_randomCostumeColor)
			{
				RandomCostumeColors(GetCostumeName(CharacterCostume.CostumeType.Body), GetCostumeName(CharacterCostume.CostumeType.Head));
			}
		}
		if (GameManager.IsPrologueMode)
		{
			SkipSelectedProcess = true;
			if (Application.isPlaying)
			{
				Singleton<PrologueManager>.Instance().MakeLitSphereOverride(MeshObjectTransform);
			}
		}
	}

	private void Costume_ModelChanged()
	{
		if (Application.isPlaying && Shadows != null)
		{
			Shadows.RefreshModel();
		}
	}

	private void RefixBoneMerge()
	{
		Transform transform = MeshObjectTransform.Find("Equipment");
		if (transform != null)
		{
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				BoneMergeable.AttachBoneMerge(transform.GetChild(i).gameObject);
			}
		}
	}

	[ExposedInEditor("전체 모델 리로드")]
	[UsedImplicitly]
	private void ResetCostumeDict()
	{
		_costumeNameKeys.Clear();
		_costumeNameValues.Clear();
	}

	private void SetCostumeKeyValue(string key, string value)
	{
		int num = _costumeNameKeys.IndexOfIgnoreCase(key);
		if (num >= 0)
		{
			_costumeNameValues[num] = value;
			return;
		}
		_costumeNameKeys.Add(key);
		_costumeNameValues.Add(value);
	}

	private string GetCostumeKeyValue(string key)
	{
		int num = _costumeNameKeys.IndexOfIgnoreCase(key);
		return (num < 0) ? string.Empty : _costumeNameValues[num];
	}

	private void StoreCostumeColor(string type, ItemColor color)
	{
		if (color.IsMultiColor)
		{
			for (int i = 0; i < 3; i++)
			{
				string key = type + "_color_" + i;
				string value = color[i].ToHex();
				SetCostumeKeyValue(key, value);
			}
		}
		else
		{
			string key2 = type + "_color";
			string value2 = color[0].ToHex();
			SetCostumeKeyValue(key2, value2);
		}
	}

	private void Costume_ColorChanged(CharacterCostume.CostumeType type, ItemColor color)
	{
		StoreCostumeColor(type.ToString(), color);
	}

	public ItemColor GetStoredThreeColors(string key)
	{
		ItemColor result = new ItemColor(3);
		for (int i = 0; i < 3; i++)
		{
			int num = _costumeNameKeys.IndexOfIgnoreCase($"{key}_{i}");
			if (num >= 0)
			{
				result[i] = _costumeNameValues[num].ToColor();
				continue;
			}
			return default(ItemColor);
		}
		return result;
	}

	public Color GetStoredColor(string key)
	{
		int num = _costumeNameKeys.IndexOfIgnoreCase(key);
		return (num < 0) ? Color.clear : _costumeNameValues[num].ToColor();
	}

	[ExposedInEditor("저장된 컬러 모델에 적용하기")]
	public void UpdateStoredCostumeColors()
	{
		ItemColor storedThreeColors = GetStoredThreeColors("Body_color");
		if (storedThreeColors.HasValue)
		{
			ChangeCostumeColor(CharacterCostume.CostumeType.Body, storedThreeColors);
		}
		ItemColor storedThreeColors2 = GetStoredThreeColors("Head_color");
		if (storedThreeColors2.HasValue)
		{
			ChangeCostumeColor(CharacterCostume.CostumeType.Head, storedThreeColors2);
		}
		Color storedColor = GetStoredColor("Skin_color");
		if (Color.clear != storedColor)
		{
			ChangeCostumeColor(CharacterCostume.CostumeType.Skin, new ItemColor(storedColor));
		}
		Color storedColor2 = GetStoredColor("Hair_color");
		if (Color.clear != storedColor2)
		{
			ChangeCostumeColor(CharacterCostume.CostumeType.Hair, new ItemColor(storedColor2));
		}
		ItemColor storedThreeColors3 = GetStoredThreeColors("Equipment_color");
		if (storedThreeColors3.HasValue)
		{
			ChangeEquipmentColor(storedThreeColors3);
		}
	}

	public ItemColor[] MakeRestoreCostumeColors()
	{
		return new ItemColor[7]
		{
			GetStoredThreeColors("Body_color"),
			GetStoredThreeColors("Head_color"),
			new ItemColor(GetStoredColor("Skin_color")),
			new ItemColor(GetStoredColor("Hair_color")),
			default(ItemColor),
			default(ItemColor),
			default(ItemColor)
		};
	}

	public void RandomCostumeColors(string bodyPathName, string headPathName)
	{
		if (bodyPathName == null)
		{
			return;
		}
		for (int i = 0; i < 7; i++)
		{
			string costumePathName = null;
			switch (i)
			{
			case 0:
				costumePathName = bodyPathName;
				break;
			case 1:
				costumePathName = headPathName;
				break;
			}
			if (i != 4)
			{
				RandomCostumeColorWithPart((CharacterCostume.CostumeType)i, costumePathName);
			}
		}
	}

	private void RandomCostumeColorWithPart(CharacterCostume.CostumeType type, string costumePathName)
	{
		ItemColor randomCostumePartColor = ColorTableLoader.GetRandomCostumePartColor(type, costumePathName, _isMale, CostumeColors);
		ChangeCostumeColor(type, randomCostumePartColor);
	}

	public void ReloadCostumes()
	{
		ChangeCostume(CharacterCostume.CostumeType.Body, GetCostumeKeyValue("Body"));
		ChangeCostume(CharacterCostume.CostumeType.Head, GetCostumeKeyValue("Head"));
		ChangeCostume(CharacterCostume.CostumeType.Hair, GetCostumeKeyValue("Hair"));
		ChangeCostume(CharacterCostume.CostumeType.Beard, GetCostumeKeyValue("Beard"));
		ChangeEquipment(GetCostumeKeyValue("Equipment"));
		Costume_ModelChanged();
	}

	private void ResetEquipment()
	{
		if (_equipmentObj != null)
		{
			BoneMergeable.DetachBoneMerge(_equipmentObj);
			if (Application.isPlaying)
			{
				Object.Destroy(_equipmentObj);
			}
			else
			{
				Object.DestroyImmediate(_equipmentObj);
			}
			_equipmentObj = null;
		}
		else
		{
			if (MeshObjectTransform == null)
			{
				return;
			}
			Transform transform = MeshObjectTransform.Find("Equipment");
			if (transform == null)
			{
				return;
			}
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(childCount - 1 - i);
				if (Application.isPlaying)
				{
					Object.Destroy(child.gameObject);
				}
				else
				{
					Object.DestroyImmediate(child.gameObject);
				}
			}
		}
	}

	public void SetWeaponVisible(bool visible)
	{
		if ((bool)_equipmentObj)
		{
			_equipmentObj.SetActive(visible);
			if (Shadows != null)
			{
				Shadows.RefreshModel();
			}
		}
	}

	private void AttachHead()
	{
		if (Application.isPlaying || _referenceModelPrefab == null || base.MeshObject == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(_referenceModelPrefab);
		if (gameObject == null)
		{
			return;
		}
		Animation componentInChildren = gameObject.transform.GetComponentInChildren<Animation>();
		if (componentInChildren == null)
		{
			Object.DestroyImmediate(gameObject);
			return;
		}
		Transform transform = componentInChildren.gameObject.transform.Find("Head");
		if (transform == null)
		{
			Object.DestroyImmediate(gameObject);
			return;
		}
		GameObject gameObject2 = Object.Instantiate(transform.gameObject);
		if (!(gameObject2 == null))
		{
			gameObject2.transform.parent = MeshObjectTransform;
			gameObject2.name = transform.name;
			gameObject2.transform.localPosition = transform.localPosition;
			gameObject2.transform.localRotation = transform.localRotation;
			gameObject2.transform.localScale = transform.localScale;
			Object.DestroyImmediate(gameObject);
		}
	}

	public Dictionary<string, string> GetCostumeDictionary()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 0; i != Mathf.Min(_costumeNameKeys.Count, _costumeNameValues.Count); i++)
		{
			dictionary.Add(_costumeNameKeys[i], _costumeNameValues[i]);
		}
		return dictionary;
	}

	public override void Select(bool selected, Color outlineColor = default(Color), float outlineWidth = 0f)
	{
	}

	[UsedImplicitly]
	private void OnAttack()
	{
	}
}
