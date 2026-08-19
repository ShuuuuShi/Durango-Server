using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MsgPack;
using Shared.Battle;
using UnityEngine;

public class NPCActorBehavior : AnimalBehavior, IBubbleTalkable, ICostumable
{
	[SerializeField]
	public const string NoneName = "_None_";

	private static ulong _nextNPCEntityId = 50000uL;

	[ExposedInEditor(null)]
	public bool _isFakeLocalPlayer;

	[SerializeField]
	public string _npcDisplayName = string.Empty;

	[SerializeField]
	public GameObject _referenceModelPrefab;

	[SerializeField]
	public bool _applyStoredColorsOnStart;

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

	private Transform _weaponTipTransform;

	private CharacterCostume _costume;

	private ChatableNPC _chatable;

	private CharacterCostume Costume
	{
		get
		{
			if (_costume == null)
			{
				_costume = new CharacterCostume(_isMale);
			}
			return _costume;
		}
	}

	public override Transform WeaponTipTransform => _weaponTipTransform;

	public ItemColor[] CostumeColors => Costume.CostumeColors;

	public override ChatableBase ChatableBase
	{
		get
		{
			if ((Object)(object)_chatable == (Object)null)
			{
				_chatable = ((Component)this).GetComponent<ChatableNPC>();
			}
			if ((Object)(object)_chatable == (Object)null)
			{
				_chatable = ((Component)this).gameObject.AddComponent<ChatableNPC>();
			}
			return _chatable;
		}
	}

	public CharacterCostume.SkinDirty SkinDirtyLevel
	{
		get
		{
			return Costume.GetSkinDirtyLevel();
		}
		set
		{
			Costume.SetSkinDirtyLevel(value);
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

	public bool IsTalkerVisible()
	{
		return IsVisible;
	}

	public Transform GetTalkBubbleTransform()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return GetBodyPartTransform(BodyPart.Head);
	}

	public string GetDisplayName()
	{
		return _npcDisplayName;
	}

	public string[] GetAnimPaths()
	{
		string arg = ((!IsMale) ? "Female" : "Male");
		return new string[2]
		{
			$"Assets/Models/Prologue/NPC/{arg}/_Anim/",
			$"Assets/Models/PC/{arg}/_Anim/NPCShared/"
		};
	}

	public void ChangeCostume(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return;
		}
		CharacterCostume.CostumeType costumeType = CharacterCostume.GetCostumeType(fileName);
		if (costumeType == CharacterCostume.CostumeType.Equipment)
		{
			ChangeEquipment(fileName);
			return;
		}
		if (!CostumeColors[(int)costumeType].HasValue)
		{
			UpdateCostumeColorsFromMaterials();
		}
		ChangeCostume(costumeType, fileName);
	}

	public void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color)
	{
		Costume.ChangeCostumeColor(type, color);
	}

	public void OnModelChanged()
	{
		if (Application.isPlaying)
		{
			Outline component = ((Component)this).GetComponent<Outline>();
			if ((Object)(object)component != (Object)null)
			{
				component.RefreshModel();
			}
			PlaneShadows component2 = ((Component)this).GetComponent<PlaneShadows>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.RefreshModel();
			}
		}
	}

	public void SetCostumeVisible(CharacterCostume.CostumeType type, bool isVisible)
	{
		if (!isVisible)
		{
			UpdateCostume(type.ToString(), string.Empty);
		}
		Costume.SetCostumeVisible(type, isVisible);
	}

	public void ChangeEquipment(string path)
	{
		if (path.Length == 0)
		{
			ResetEquipment();
			return;
		}
		UpdateCostume(CharacterCostume.CostumeType.Equipment.ToString(), path);
		KSingleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), delegate(Object asset)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Expected O, but got Unknown
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			if (!(asset == (Object)null))
			{
				ResetEquipment();
				_equipmentObj = (GameObject)Object.Instantiate(asset);
				BoneMergeable.AttachBoneMerge(_equipmentObj);
				GameObject val = KUtility.FindObjectByName(_equipmentObj, "Weapon_Tip");
				if ((Object)(object)val != (Object)null)
				{
					_weaponTipTransform = val.transform;
				}
				else
				{
					Transform val2 = KUtility.FindTransformByName(_equipmentObj, "Attachment_RH");
					if (Object.op_Implicit((Object)(object)val2))
					{
						val = new GameObject("Weapon_Tip");
						val.transform.parent = val2;
						val.transform.localPosition = new Vector3(-100f, 0f, 0f);
						_weaponTipTransform = val.transform;
					}
					else
					{
						_weaponTipTransform = ((Component)this).transform;
					}
				}
				BoneMergeable.UpdateBoneMergeSet();
			}
		});
	}

	private void Awake()
	{
		base.EntityId = _nextNPCEntityId;
		_nextNPCEntityId++;
		Init();
		SetServerSideRootMotionEnable(serverSideRootMotionEnabled: false);
	}

	public void Init()
	{
		if ((Object)(object)base.MeshObject.transform.FindChild("Head") == (Object)null && (Object)(object)_referenceModelPrefab != (Object)null)
		{
			AttachHead();
		}
		Costume.Init(base.MeshObject, MakeRestoreCostumeColors());
		Costume.ModelChanged += OnModelChanged;
		Costume.ColorChanged += Costume_ColorChanged;
		Costume.SyncCostumeProperty(GetCostumePath("Hair"), GetCostumePath("Head"), GetCostumePath("Body"), GetCostumePath("Beard"));
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
				RandomCostumeColors(GetCostumeName("Body"), GetCostumeName("Head"));
			}
		}
		if (GameManager.IsPrologueMode)
		{
			ShowSelectEffect = false;
			if (Application.isPlaying)
			{
				KSingleton<PrologueManager>.Instance().MakeLitSphereOverride(MeshObjectTransform);
			}
		}
	}

	private void RefixBoneMerge()
	{
		Transform val = MeshObjectTransform.FindChild("Equipment");
		if ((Object)(object)val != (Object)null)
		{
			int childCount = val.childCount;
			for (int i = 0; i < childCount; i++)
			{
				BoneMergeable.AttachBoneMerge(((Component)val.GetChild(i)).gameObject);
			}
		}
	}

	public string GetCostumeName(string partName)
	{
		string costumePath = GetCostumePath(partName);
		return (costumePath.Length != 0) ? costumePath.Substring(costumePath.LastIndexOf('/') + 1) : string.Empty;
	}

	public string GetCostumePath(string partName)
	{
		for (int i = 0; i < _costumeNameKeys.Count; i++)
		{
			if (string.Equals(_costumeNameKeys[i], partName, StringComparison.OrdinalIgnoreCase))
			{
				return _costumeNameValues[i];
			}
		}
		return string.Empty;
	}

	public string GetEquipmentModelName()
	{
		if (Object.op_Implicit((Object)(object)_equipmentObj))
		{
			SkinnedMeshRenderer componentInChildren = _equipmentObj.GetComponentInChildren<SkinnedMeshRenderer>(true);
			if (Object.op_Implicit((Object)(object)componentInChildren) && Object.op_Implicit((Object)(object)componentInChildren.sharedMesh))
			{
				return ((Object)componentInChildren.sharedMesh).name;
			}
		}
		return string.Empty;
	}

	public string GetStoredEquipmentModelName()
	{
		return GetCostumeName("Equipment");
	}

	public void ResetCostumeDict()
	{
		_costumeNameKeys.Clear();
		_costumeNameValues.Clear();
	}

	private void UpdateCostume(string key, string filename)
	{
		int num = _costumeNameKeys.IndexOfIgnoreCase(key);
		if (num >= 0)
		{
			_costumeNameValues[num] = filename;
			return;
		}
		_costumeNameKeys.Add(key);
		_costumeNameValues.Add(filename);
	}

	private void StoreCostumeColor(CharacterCostume.CostumeType type, ItemColor color)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (color.IsMultiColor)
		{
			for (int i = 0; i < 3; i++)
			{
				string key = string.Concat(type, "_color_", i);
				string filename = KUtility.ToString(color[i]);
				UpdateCostume(key, filename);
			}
		}
		else
		{
			string key2 = string.Concat(type, "_color");
			string filename2 = KUtility.ToString(color[0]);
			UpdateCostume(key2, filename2);
		}
	}

	public void ChangeCostume(CharacterCostume.CostumeType type, string fileName)
	{
		UpdateCostume(type.ToString(), fileName);
		Costume.ChangeCostume(fileName);
	}

	private void Costume_ColorChanged(CharacterCostume.CostumeType type, ItemColor color)
	{
		StoreCostumeColor(type, color);
	}

	public void UpdateCostumeColorsFromMaterials()
	{
		UpdateBodyColorFromMaterial();
		UpdateHairColorFromMaterial();
		UpdateHeadColorFromMaterial();
		UpdateEquipmentColorFromMaterial();
	}

	private void UpdateBodyColorFromMaterial()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		Transform val = MeshObjectTransform.FindChild("Body");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		Material[] materials = KUtility.GetMaterials((Renderer)(object)((Component)val).GetComponent<SkinnedMeshRenderer>());
		if (materials != null && !((Object)(object)materials[0] == (Object)null) && !((Object)(object)materials[1] == (Object)null))
		{
			Material val2;
			Material val3;
			if (((Object)materials[0].shader).name.Contains("Three"))
			{
				val2 = materials[0];
				val3 = materials[1];
			}
			else
			{
				val2 = materials[1];
				val3 = materials[0];
			}
			Color color = val2.GetColor("_ThreeColor_1");
			Color color2 = val2.GetColor("_ThreeColor_2");
			Color color3 = val2.GetColor("_ThreeColor_3");
			Color color4 = val3.GetColor("_XColor");
			ChangeCostumeColor(CharacterCostume.CostumeType.Body, new ItemColor(color, color2, color3));
			ChangeCostumeColor(CharacterCostume.CostumeType.Skin, new ItemColor(color4));
		}
	}

	private void UpdateHairColorFromMaterial()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Transform val = MeshObjectTransform.FindChild("Hair");
		if (!((Object)(object)val == (Object)null))
		{
			Material material = KUtility.GetMaterial((Renderer)(object)((Component)val).GetComponent<SkinnedMeshRenderer>());
			if (!((Object)(object)material == (Object)null))
			{
				Color color = material.GetColor("_2XColor");
				ChangeCostumeColor(CharacterCostume.CostumeType.Hair, new ItemColor(color));
			}
		}
	}

	private void UpdateHeadColorFromMaterial()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		Transform val = MeshObjectTransform.FindChild("Head");
		if (!((Object)(object)val == (Object)null))
		{
			Material material = KUtility.GetMaterial((Renderer)(object)((Component)val).GetComponent<SkinnedMeshRenderer>());
			if (!((Object)(object)material == (Object)null) && material.HasProperty("_ThreeColor_1"))
			{
				Color color = material.GetColor("_ThreeColor_1");
				Color color2 = material.GetColor("_ThreeColor_2");
				Color color3 = material.GetColor("_ThreeColor_3");
				ChangeCostumeColor(CharacterCostume.CostumeType.Head, new ItemColor(color, color2, color3));
			}
		}
	}

	private void UpdateEquipmentColorFromMaterial()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		Transform val = MeshObjectTransform.FindChild("Equipment");
		if ((Object)(object)val == (Object)null || val.childCount <= 0)
		{
			return;
		}
		Transform child = val.GetChild(0);
		if ((Object)(object)child == (Object)null)
		{
			return;
		}
		SkinnedMeshRenderer val2 = ((Component)child).GetComponent<SkinnedMeshRenderer>();
		if ((Object)(object)val2 == (Object)null)
		{
			val2 = ((Component)child).GetComponentInChildren<SkinnedMeshRenderer>(true);
			if ((Object)(object)val2 == (Object)null)
			{
				return;
			}
		}
		Material val3 = ((!Application.isPlaying) ? ((Renderer)val2).sharedMaterial : ((Renderer)val2).material);
		if (!((Object)(object)val3 == (Object)null) && val3.HasProperty("_ThreeColor_1"))
		{
			Color color = val3.GetColor("_ThreeColor_1");
			Color color2 = val3.GetColor("_ThreeColor_2");
			Color color3 = val3.GetColor("_ThreeColor_3");
			ChangeCostumeColor(CharacterCostume.CostumeType.Equipment, new ItemColor(color, color2, color3));
		}
	}

	public ItemColor GetStoredThreeColors(string key)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		ItemColor result = new ItemColor(3);
		for (int i = 0; i < 3; i++)
		{
			int num = _costumeNameKeys.IndexOfIgnoreCase($"{key}_{i}");
			if (num >= 0)
			{
				result[i] = KUtility.ToColor(_costumeNameValues[num]);
				continue;
			}
			return default(ItemColor);
		}
		return result;
	}

	public Color GetStoredColor(string key)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		int num = _costumeNameKeys.IndexOfIgnoreCase(key);
		return (num < 0) ? Color.clear : KUtility.ToColor(_costumeNameValues[num]);
	}

	public void UpdateStoredCostumeColors()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		ItemColor storedThreeColors = GetStoredThreeColors("body_color");
		if (storedThreeColors.HasValue)
		{
			ChangeCostumeColor(CharacterCostume.CostumeType.Body, storedThreeColors);
		}
		ItemColor storedThreeColors2 = GetStoredThreeColors("head_color");
		if (storedThreeColors2.HasValue)
		{
			ChangeCostumeColor(CharacterCostume.CostumeType.Head, storedThreeColors2);
		}
		Color storedColor = GetStoredColor("skin_color");
		if (Color.clear != storedColor)
		{
			ChangeCostumeColor(CharacterCostume.CostumeType.Skin, new ItemColor(storedColor));
		}
		Color storedColor2 = GetStoredColor("hair_color");
		if (Color.clear != storedColor2)
		{
			ChangeCostumeColor(CharacterCostume.CostumeType.Hair, new ItemColor(storedColor2));
		}
		ItemColor storedThreeColors3 = GetStoredThreeColors("equipment_color");
		if (storedThreeColors3.HasValue)
		{
			ChangeCostumeColor(CharacterCostume.CostumeType.Equipment, storedThreeColors3);
		}
	}

	public ItemColor[] MakeRestoreCostumeColors()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		return new ItemColor[8]
		{
			GetStoredThreeColors("body_color"),
			GetStoredThreeColors("head_color"),
			new ItemColor(GetStoredColor("skin_color")),
			new ItemColor(GetStoredColor("hair_color")),
			default(ItemColor),
			default(ItemColor),
			default(ItemColor),
			GetStoredThreeColors("equipment_color")
		};
	}

	public void RandomCostumeColors(string bodyPathName, string headPathName)
	{
		if (bodyPathName == null)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			string clothPathName = null;
			switch (i)
			{
			case 0:
				clothPathName = bodyPathName;
				break;
			case 1:
				clothPathName = headPathName;
				break;
			}
			if (i != 4)
			{
				RandomCostumeColorWithPart((CharacterCostume.CostumeType)i, clothPathName);
			}
		}
	}

	private void RandomCostumeColorWithPart(CharacterCostume.CostumeType type, string clothPathName)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		int costumeColorCount = CharacterCostume.GetCostumeColorCount(type);
		ItemColor color = new ItemColor(costumeColorCount);
		for (int i = 0; i < costumeColorCount; i++)
		{
			switch (type)
			{
			case CharacterCostume.CostumeType.Skin:
				color[i] = ColorTableLoader.GetRandomSkinColor();
				continue;
			case CharacterCostume.CostumeType.Hair:
				color[i] = ColorTableLoader.GetRandomHairColor();
				continue;
			case CharacterCostume.CostumeType.Eye:
				color[i] = ColorTableLoader.GetRandomEyeColor();
				continue;
			case CharacterCostume.CostumeType.Lip:
				color[i] = ColorTableLoader.GetRandomLipColor(_isMale);
				continue;
			}
			if (string.IsNullOrEmpty(clothPathName))
			{
				if (CostumeColors[(int)type].HasValue)
				{
					color[i] = CostumeColors[(int)type][i];
				}
			}
			else
			{
				color[i] = ColorTableLoader.GetRandomClothColor(clothPathName, i);
			}
		}
		ChangeCostumeColor(type, color);
	}

	public void ReloadCostumes()
	{
		ChangeCostume(GetCostumePath("Body"));
		ChangeCostume(GetCostumePath("Head"));
		ChangeCostume(GetCostumePath("Hair"));
		ChangeEquipment(GetCostumePath("Equipment"));
		OnModelChanged();
	}

	private void ResetEquipment()
	{
		if ((Object)(object)_equipmentObj != (Object)null)
		{
			BoneMergeable.DetachBoneMerge(_equipmentObj);
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_equipmentObj);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_equipmentObj);
			}
			_equipmentObj = null;
		}
		else
		{
			if ((Object)(object)MeshObjectTransform == (Object)null)
			{
				return;
			}
			Transform val = MeshObjectTransform.FindChild("Equipment");
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			int childCount = val.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = val.GetChild(childCount - 1 - i);
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)((Component)child).gameObject);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)((Component)child).gameObject);
				}
			}
		}
	}

	public override void SetWeaponVisible(bool visible)
	{
		if (Object.op_Implicit((Object)(object)_equipmentObj))
		{
			_equipmentObj.SetActive(visible);
			((Component)this).GetComponent<PlaneShadows>().RefreshModel();
		}
	}

	private void AttachHead()
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying || (Object)(object)_referenceModelPrefab == (Object)null || (Object)(object)base.MeshObject == (Object)null)
		{
			return;
		}
		GameObject val = Object.Instantiate<GameObject>(_referenceModelPrefab);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		Animation componentInChildren = ((Component)val.transform).GetComponentInChildren<Animation>();
		if ((Object)(object)componentInChildren == (Object)null)
		{
			Object.DestroyImmediate((Object)(object)val);
			return;
		}
		Transform val2 = ((Component)componentInChildren).gameObject.transform.FindChild("Head");
		if ((Object)(object)val2 == (Object)null)
		{
			Object.DestroyImmediate((Object)(object)val);
			return;
		}
		GameObject val3 = Object.Instantiate<GameObject>(((Component)val2).gameObject);
		if (!((Object)(object)val3 == (Object)null))
		{
			val3.transform.parent = MeshObjectTransform;
			((Object)val3).name = ((Object)val2).name;
			val3.transform.localPosition = val2.localPosition;
			val3.transform.localRotation = val2.localRotation;
			val3.transform.localScale = val2.localScale;
			Object.DestroyImmediate((Object)(object)val);
		}
	}

	public MessagePackObjectDictionary AllocCostumeDict()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		MessagePackObjectDictionary val = new MessagePackObjectDictionary();
		for (int i = 0; i != Mathf.Min(_costumeNameKeys.Count, _costumeNameValues.Count); i++)
		{
			val.Add(MessagePackObject.op_Implicit(_costumeNameKeys[i]), MessagePackObject.op_Implicit(_costumeNameValues[i]));
		}
		if (!val.ContainsKey(MessagePackObject.op_Implicit("eye_color")))
		{
			val.Add(MessagePackObject.op_Implicit("eye_color"), MessagePackObject.op_Implicit(KUtility.ToString(ColorTableLoader.GetRandomEyeColor())));
		}
		if (!val.ContainsKey(MessagePackObject.op_Implicit("lip_color")))
		{
			val.Add(MessagePackObject.op_Implicit("lip_color"), MessagePackObject.op_Implicit(KUtility.ToString(ColorTableLoader.GetRandomLipColor(_isMale))));
		}
		val.Add(MessagePackObject.op_Implicit("body_size"), MessagePackObject.op_Implicit(1));
		return val;
	}

	[UsedImplicitly]
	private void OnAttack()
	{
	}
}
