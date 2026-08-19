using System;
using JetBrains.Annotations;
using UnityEngine;

public class CharacterCostume
{
	public enum CostumeType
	{
		Body,
		Head,
		Skin,
		Hair,
		Beard,
		Eye,
		Lip,
		Equipment,
		TotalCount
	}

	public enum SkinDirty
	{
		Clean,
		Dirty,
		VeryDirty
	}

	private const int TextureCount = 6;

	private static readonly Texture[] SkinTexture = (Texture[])(object)new Texture[6];

	private static readonly string[] SkinTexturePath = new string[6] { "m_body_nude01_clean", "m_body_nude01_dirty", "m_body_nude01_very_dirty", "f_body_nude01_clean", "f_body_nude01_dirty", "f_body_nude01_very_dirty" };

	private static Texture _dirtyTexture;

	private SkinDirty _skinDirtyLevel;

	private Transform _transform;

	private Transform _rootBone;

	private int _costumeLoadQueueCount = -1;

	private string _currentHairName = string.Empty;

	private bool _isHoodyMode;

	private bool _activeHairByHeadIndex = true;

	private int _currentHeadIndex = -1;

	private readonly bool[] _costumeVisible;

	private readonly bool _isMale;

	public ItemColor[] CostumeColors { get; private set; }

	public bool IsCostumeLoaded => _costumeLoadQueueCount == 0;

	private bool IsHoodyMode
	{
		get
		{
			return _isHoodyMode;
		}
		set
		{
			if (_isHoodyMode != value)
			{
				_isHoodyMode = value;
				UpdateHairAndHeadActivation();
			}
		}
	}

	private int CurrentHeadIndex
	{
		get
		{
			return _currentHeadIndex;
		}
		set
		{
			if (_currentHeadIndex != value)
			{
				_currentHeadIndex = value;
				ChangeHairByHeadIndex();
			}
		}
	}

	public event Action ModelChanged;

	public event Action<CostumeType, ItemColor> ColorChanged;

	public CharacterCostume(bool isMale)
	{
		CostumeColors = new ItemColor[8];
		_costumeVisible = new bool[5];
		for (int i = 0; i < _costumeVisible.Length; i++)
		{
			_costumeVisible[i] = true;
		}
		_isMale = isMale;
	}

	public void Init(GameObject rootGameObject, ItemColor[] initCostumeColors = null)
	{
		this.ModelChanged = null;
		this.ColorChanged = null;
		_transform = rootGameObject.transform;
		_rootBone = rootGameObject.transform.FindChild("Bip001");
		if (initCostumeColors != null)
		{
			CostumeColors = initCostumeColors;
		}
	}

	public void SyncCostumeProperty(string hairPath, string headPath, string bodyPath, string beardPath)
	{
		SetCostumeVisible(CostumeType.Hair, !string.IsNullOrEmpty(hairPath));
		SetCostumeVisible(CostumeType.Head, !string.IsNullOrEmpty(headPath));
		SetCostumeVisible(CostumeType.Beard, !string.IsNullOrEmpty(beardPath));
		_currentHairName = hairPath;
		_currentHeadIndex = GetHeadIndex(headPath);
		UpdateHoodyMode(bodyPath);
	}

	private static int GetHeadIndex(string headName)
	{
		if (headName.Length <= 6)
		{
			return -1;
		}
		string source = headName.Substring(headName.Length - 6, 2);
		return source.ToInt();
	}

	private void UpdateHoodyMode(string bodyName)
	{
		IsHoodyMode = bodyName.Contains("hoody") && !bodyName.Contains("torn");
	}

	public static string GetPartName(CostumeType type)
	{
		switch (type)
		{
		case CostumeType.Body:
		case CostumeType.Skin:
			return "Body";
		case CostumeType.Hair:
			return "Hair";
		case CostumeType.Beard:
			return "Beard";
		case CostumeType.Equipment:
			return "Equipment";
		default:
			return "Head";
		}
	}

	public void UpdateHairAndHeadActivation()
	{
		UpdateCostumeActivation(CostumeType.Hair);
		UpdateCostumeActivation(CostumeType.Head);
	}

	public void ChangeHairByHeadIndex()
	{
		if (!(_currentHairName == string.Empty))
		{
			_activeHairByHeadIndex = CurrentHeadIndex != 0;
			UpdateHairAndHeadActivation();
			string text;
			switch (CurrentHeadIndex)
			{
			case -1:
				text = _currentHairName + "_01";
				break;
			case 0:
				return;
			default:
				text = _currentHairName + "_" + CurrentHeadIndex.ToString("D2");
				break;
			}
			text += ".FBX";
			ChangeCostume(text);
		}
	}

	public static CostumeType GetCostumeType(string fileName)
	{
		if (fileName.Contains("_hair_"))
		{
			return CostumeType.Hair;
		}
		if (fileName.Contains("_head_"))
		{
			return CostumeType.Head;
		}
		if (fileName.Contains("_inner_"))
		{
			return CostumeType.Body;
		}
		if (fileName.Contains("_beard_"))
		{
			return CostumeType.Beard;
		}
		if (fileName.Contains("Equipment"))
		{
			return CostumeType.Equipment;
		}
		return CostumeType.Body;
	}

	public void ChangeCostume(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return;
		}
		CostumeType type = GetCostumeType(fileName);
		if (type == CostumeType.Hair && !fileName.EndsWith(".FBX", StringComparison.OrdinalIgnoreCase))
		{
			_currentHairName = fileName;
			ChangeHairByHeadIndex();
			return;
		}
		if (_costumeLoadQueueCount == -1)
		{
			_costumeLoadQueueCount = 1;
		}
		else
		{
			_costumeLoadQueueCount++;
		}
		KSingleton<AssetBundleManager>.Instance().RequestAsset(fileName, typeof(GameObject), delegate(Object asset)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			GameObject val = (GameObject)asset;
			if (!((Object)(object)val == (Object)null) && !((Object)(object)_transform == (Object)null))
			{
				SkinnedMeshRenderer componentInChildren = val.GetComponentInChildren<SkinnedMeshRenderer>(true);
				if (!((Object)(object)componentInChildren == (Object)null))
				{
					string partName = GetPartName(type);
					Transform val2 = _transform.FindChild(partName);
					if (!((Object)(object)val2 == (Object)null))
					{
						SkinnedMeshRenderer component = ((Component)val2).GetComponent<SkinnedMeshRenderer>();
						switch (type)
						{
						case CostumeType.Head:
							CurrentHeadIndex = GetHeadIndex(fileName);
							break;
						case CostumeType.Body:
							UpdateHoodyMode(fileName);
							break;
						}
						component.sharedMesh = componentInChildren.sharedMesh;
						((Renderer)component).sharedMaterials = ((Renderer)componentInChildren).sharedMaterials;
						Transform[] array = AllocAdjustedBones(_rootBone, componentInChildren.bones);
						if (array != null)
						{
							component.bones = array;
						}
						switch (type)
						{
						case CostumeType.Body:
							ChangeCostumeColor(CostumeType.Body, CostumeColors[0]);
							ChangeCostumeColor(CostumeType.Skin, CostumeColors[2]);
							ApplyDirtyTexture();
							break;
						case CostumeType.Hair:
						case CostumeType.Beard:
							ChangeCostumeColor(CostumeType.Hair, CostumeColors[3]);
							break;
						case CostumeType.Equipment:
							ChangeCostumeColor(CostumeType.Equipment, CostumeColors[7]);
							break;
						default:
							ChangeCostumeColor(CostumeType.Head, CostumeColors[1]);
							break;
						}
						if (this.ModelChanged != null)
						{
							this.ModelChanged();
						}
						_costumeLoadQueueCount--;
					}
				}
			}
		});
	}

	private static Transform[] AllocAdjustedBones(Transform rootBone, Transform[] skinBones)
	{
		Transform[] componentsInChildren = ((Component)rootBone).GetComponentsInChildren<Transform>();
		if (componentsInChildren.Length == 0)
		{
			return null;
		}
		Transform[] array = (Transform[])(object)new Transform[skinBones.Length];
		for (int i = 0; i < skinBones.Length; i++)
		{
			Transform val = skinBones[i];
			array[i] = componentsInChildren[0];
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (((Object)componentsInChildren[j]).name == ((Object)val).name)
				{
					array[i] = componentsInChildren[j];
					break;
				}
			}
		}
		return array;
	}

	public bool GetCostumeVisible(CostumeType type)
	{
		return (int)type < _costumeVisible.Length && _costumeVisible[(int)type];
	}

	public void SetCostumeVisible(CostumeType type, bool isVisible)
	{
		if ((int)type < _costumeVisible.Length)
		{
			_costumeVisible[(int)type] = isVisible;
			if (type == CostumeType.Head && !isVisible)
			{
				CurrentHeadIndex = 1;
			}
			UpdateCostumeActivation(type);
		}
	}

	private void UpdateCostumeActivation(CostumeType type)
	{
		bool flag = GetCostumeVisible(type);
		switch (type)
		{
		case CostumeType.Head:
			flag &= !IsHoodyMode;
			break;
		case CostumeType.Hair:
			flag &= !IsHoodyMode && _activeHairByHeadIndex;
			break;
		}
		string partName = GetPartName(type);
		Transform val = _transform.FindChild(partName);
		if ((Object)(object)val != (Object)null)
		{
			((Component)val).gameObject.SetActive(flag);
		}
	}

	public static int GetCostumeColorCount(CostumeType type)
	{
		return (type != 0 && type != CostumeType.Head && type != CostumeType.Equipment) ? 1 : 3;
	}

	public void ChangeCostumeColor(CostumeType type, ItemColor color, int colorKeyIndex = -1)
	{
		ChangeMaterialColor(type, color, colorKeyIndex);
		if (type == CostumeType.Hair && _costumeVisible[4])
		{
			ChangeMaterialColor(CostumeType.Beard, color, colorKeyIndex);
		}
		CostumeColors[(int)type] = color;
		if (this.ColorChanged != null)
		{
			this.ColorChanged(type, color);
		}
	}

	private void ChangeMaterialColor(CostumeType type, ItemColor color, int colorKeyIndex)
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (!color.HasValue)
		{
			return;
		}
		string partName = GetPartName(type);
		GameObject val = KUtility.FindObjectByName(((Component)_transform).gameObject, partName, includeInactive: true);
		if ((Object)(object)val == (Object)null)
		{
			if (!(partName != "Equipment"))
			{
			}
			return;
		}
		SkinnedMeshRenderer val2 = val.GetComponent<SkinnedMeshRenderer>();
		if ((Object)(object)val2 == (Object)null)
		{
			val2 = val.GetComponentInChildren<SkinnedMeshRenderer>(true);
			if ((Object)(object)val2 == (Object)null)
			{
				return;
			}
		}
		Material[] materials = KUtility.GetMaterials((Renderer)(object)val2);
		int num = materials.Length;
		for (int i = 0; i < num; i++)
		{
			Material val3 = materials[i];
			if ((Object)(object)val3 == (Object)null || (Object)(object)val3.shader == (Object)null)
			{
				continue;
			}
			if (color.IsMultiColor)
			{
				if (!((Object)val3.shader).name.Contains("Three"))
				{
					continue;
				}
				for (int j = 0; j < 3; j++)
				{
					if (colorKeyIndex == -1 || colorKeyIndex == j)
					{
						val3.SetColor($"_ThreeColor_{j + 1}", color[j]);
					}
				}
			}
			else
			{
				val3.SetColor("_XColor", color[0]);
				val3.SetColor("_2XColor", color[0]);
			}
		}
	}

	public SkinDirty GetSkinDirtyLevel()
	{
		return _skinDirtyLevel;
	}

	public void SetSkinDirtyLevel(SkinDirty dirtyLevel)
	{
		if (_skinDirtyLevel != dirtyLevel)
		{
			_skinDirtyLevel = dirtyLevel;
			ApplyDirtyTexture();
		}
	}

	private void ApplyDirtyTexture()
	{
		Texture skinTexture = GetSkinTexture(_skinDirtyLevel, _isMale);
		if ((Object)(object)skinTexture == (Object)null)
		{
			return;
		}
		Material bodyMaterial = GetBodyMaterial(isNude: true);
		if ((Object)(object)bodyMaterial != (Object)null)
		{
			bodyMaterial.mainTexture = skinTexture;
		}
		Material bodyMaterial2 = GetBodyMaterial(isNude: false);
		if ((Object)(object)bodyMaterial2 != (Object)null)
		{
			if ((Object)(object)_dirtyTexture == (Object)null)
			{
				_dirtyTexture = (Texture)(object)Resources.Load<Texture2D>("Texture/u_body_dirty");
			}
			bodyMaterial2.DisableKeyword("DIRTY_ON");
			bodyMaterial2.DisableKeyword("VERY_DIRTY_ON");
			switch (_skinDirtyLevel)
			{
			case SkinDirty.Dirty:
				bodyMaterial2.EnableKeyword("DIRTY_ON");
				break;
			case SkinDirty.VeryDirty:
				bodyMaterial2.EnableKeyword("VERY_DIRTY_ON");
				break;
			}
			bodyMaterial2.SetTexture("_DirtyTex", _dirtyTexture);
		}
	}

	private static Texture GetSkinTexture(SkinDirty dirtyLevel, bool isMale)
	{
		int num = ((!isMale) ? 3 : 0);
		num = (int)(num + dirtyLevel);
		if ((Object)(object)SkinTexture[num] == (Object)null)
		{
			SkinTexture[num] = (Texture)(object)Resources.Load<Texture2D>("Texture/" + SkinTexturePath[num]);
		}
		return SkinTexture[num];
	}

	[CanBeNull]
	private Material GetBodyMaterial(bool isNude)
	{
		string partName = GetPartName(CostumeType.Body);
		Transform val = _transform.FindChild(partName);
		SkinnedMeshRenderer component = ((Component)val).GetComponent<SkinnedMeshRenderer>();
		Material[] materials = KUtility.GetMaterials((Renderer)(object)component);
		int num = materials.Length;
		for (int i = 0; i < num; i++)
		{
			Material val2 = materials[i];
			if (!((Object)(object)val2 == (Object)null) && !((Object)(object)val2.mainTexture == (Object)null))
			{
				bool flag = ((Object)val2.mainTexture).name.Contains("body_nude");
				if (flag == isNude)
				{
					return val2;
				}
			}
		}
		return null;
	}
}
