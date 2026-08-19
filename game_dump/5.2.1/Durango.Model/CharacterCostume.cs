using System;
using System.Collections.Generic;
using Durango.Render;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Model;

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
		TotalCount
	}

	public const string FlagAttachBone = "Attachment_Flag";

	public const string AccessoryKey = "Accessory";

	private static readonly int ThreeColor1 = Shader.PropertyToID("_ThreeColor_1");

	private static readonly int ThreeColor2 = Shader.PropertyToID("_ThreeColor_2");

	private static readonly int ThreeColor3 = Shader.PropertyToID("_ThreeColor_3");

	private static readonly int SubColor = Shader.PropertyToID("_SubColor");

	private static readonly int DirtyTex = Shader.PropertyToID("_DirtyTex");

	private static readonly Dictionary<string, Texture> SkinTextures = new Dictionary<string, Texture>();

	private static Texture _dirtyTexture;

	private string _skinEffect;

	private Material _skinMaterial;

	private Transform _transform;

	private Transform _rootBone;

	private PlaneShadows _shadows;

	private int _costumeLoadQueueCount = -1;

	private string _currentHairName = string.Empty;

	private string _currentHeadName = string.Empty;

	private int _cachedHeadIndex = -1;

	private bool _isHoodyMode;

	private bool _isMale;

	private string _accessoryModelPath;

	private AttachBoneHelper _attachBoneHelper;

	private JiggleBonesController _jiggleController;

	private bool _visible = true;

	private bool _accessoryVisible = true;

	private readonly bool[] _costumeVisible = new bool[5];

	private readonly string[] _costumeNames = new string[5];

	private List<GameObject>[] _particles;

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

	public event Action ModelChanged;

	public event Action<CostumeType, ItemColor> ColorChanged;

	public void Init(bool isMale, GameObject rootGameObject, JiggleBonesController controller, PlaneShadows shadows, ItemColor[] initCostumeColors = null)
	{
		_isMale = isMale;
		_transform = rootGameObject.transform;
		_rootBone = rootGameObject.transform.Find("Bip001");
		if (_rootBone == null)
		{
			_rootBone = rootGameObject.transform.Find("Dummy_root");
		}
		_shadows = shadows;
		if (initCostumeColors == null)
		{
			CostumeColors = new ItemColor[7];
		}
		else
		{
			CostumeColors = initCostumeColors;
		}
		_attachBoneHelper = new AttachBoneHelper(_rootBone);
		_attachBoneHelper.VisibleChanged += AttachBoneHelper_VisibledChanged;
		_jiggleController = controller;
		this.ModelChanged = null;
		this.ColorChanged = null;
		_costumeVisible.SetAll(value: true);
	}

	public void SyncCostumeProperty(string hairPath, string headPath, string bodyPath, string beardPath)
	{
		SetCostumeVisible(CostumeType.Hair, !string.IsNullOrEmpty(hairPath));
		SetCostumeVisible(CostumeType.Head, !string.IsNullOrEmpty(headPath));
		SetCostumeVisible(CostumeType.Beard, !string.IsNullOrEmpty(beardPath));
		_currentHairName = hairPath;
		_currentHeadName = headPath;
		UpdateHoodyMode(bodyPath);
	}

	private void UpdateHead(string headName)
	{
		_currentHeadName = headName;
		_cachedHeadIndex = -1;
		ChangeHairByHeadIndex();
	}

	private int GetHeadIndex()
	{
		if (_currentHeadName.Length <= 6 || !GetCostumeVisible(CostumeType.Head))
		{
			return 1;
		}
		if (_cachedHeadIndex == -1)
		{
			int num = 6;
			if (_currentHeadName.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
			{
				num = 9;
			}
			string source = _currentHeadName.Substring(_currentHeadName.Length - num, 2);
			_cachedHeadIndex = source.ToInt();
		}
		return _cachedHeadIndex;
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
		case CostumeType.Head:
			return "Head";
		default:
			return null;
		}
	}

	private void UpdateHairAndHeadActivation()
	{
		UpdateCostumeVisible(CostumeType.Hair);
		UpdateCostumeVisible(CostumeType.Head);
	}

	private void ChangeHairByHeadIndex()
	{
		if (!(_currentHairName == string.Empty))
		{
			UpdateHairAndHeadActivation();
			int headIndex = GetHeadIndex();
			string text;
			switch (headIndex)
			{
			case -1:
				text = _currentHairName + "_01";
				break;
			case 0:
				return;
			default:
				text = _currentHairName + "_" + headIndex.ToString("D2");
				break;
			}
			text += ".prefab";
			ChangeCostume(CostumeType.Hair, text);
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
		return CostumeType.Body;
	}

	private bool IsNotSuitableType(CostumeType type)
	{
		if (!_isMale)
		{
			return type == CostumeType.Beard;
		}
		return false;
	}

	public void ChangeCostume(CostumeType type, string fileName)
	{
		if (IsNotSuitableType(type))
		{
			return;
		}
		if (string.IsNullOrEmpty(fileName) || fileName == "none")
		{
			if (type != 0)
			{
				RemoveCostume(type);
			}
		}
		else if (type == CostumeType.Hair && !fileName.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
		{
			_currentHairName = fileName;
			ChangeHairByHeadIndex();
		}
		else
		{
			if (_costumeNames[(int)type] == fileName)
			{
				return;
			}
			_costumeNames[(int)type] = fileName;
			SetCostumeVisible(type, isVisible: true);
			if (_costumeLoadQueueCount == -1)
			{
				_costumeLoadQueueCount = 1;
			}
			else
			{
				_costumeLoadQueueCount++;
			}
			Singleton<AssetBundleManager>.Instance().RequestAsset(fileName, typeof(GameObject), delegate(UnityEngine.Object asset)
			{
				_costumeLoadQueueCount--;
				GameObject gameObject = (GameObject)asset;
				if (!(gameObject == null) && !(_transform == null) && !(_rootBone == null))
				{
					SkinnedMeshRenderer componentInChildren = gameObject.GetComponentInChildren<SkinnedMeshRenderer>(includeInactive: true);
					if (!(componentInChildren == null))
					{
						string partName = GetPartName(type);
						if (!string.IsNullOrEmpty(partName))
						{
							Transform transform = _transform.Find(partName);
							if (!(transform == null))
							{
								SkinnedMeshRenderer component = transform.GetComponent<SkinnedMeshRenderer>();
								component.sharedMesh = componentInChildren.sharedMesh;
								component.sharedMaterials = componentInChildren.sharedMaterials;
								UpdateJiggleBones(type, gameObject);
								UpdateParticles(type, gameObject);
								Transform[] array = Prefabs.MappingBones(_rootBone.GetComponentsInChildren<Transform>(), componentInChildren.bones);
								if (array != null)
								{
									component.bones = array;
								}
								switch (type)
								{
								case CostumeType.Body:
									ApplyColorToMaterial(CostumeType.Body, CostumeColors[0]);
									ApplyColorToMaterial(CostumeType.Skin, CostumeColors[2]);
									UpdateSkinEffect();
									UpdateSkinMaterial();
									_attachBoneHelper.SetModelLink(gameObject.transform);
									break;
								case CostumeType.Head:
									ApplyColorToMaterial(CostumeType.Head, CostumeColors[1]);
									break;
								case CostumeType.Hair:
									ApplyColorToMaterial(CostumeType.Hair, CostumeColors[3]);
									break;
								case CostumeType.Beard:
									ApplyColorToMaterial(CostumeType.Beard, CostumeColors[3]);
									break;
								}
								if (type != 0)
								{
									if (type == CostumeType.Head)
									{
										UpdateHead(fileName);
									}
								}
								else
								{
									UpdateHoodyMode(fileName);
								}
								if (this.ModelChanged != null)
								{
									this.ModelChanged();
								}
							}
						}
					}
				}
			});
		}
	}

	private void RemoveCostume(CostumeType type)
	{
		SetCostumeVisible(type, isVisible: false);
		if (_jiggleController != null)
		{
			_jiggleController.Remove(type);
		}
		ClearParticles(type);
		_costumeNames[(int)type] = null;
		if (this.ModelChanged != null)
		{
			this.ModelChanged();
		}
	}

	public void SetAccessoryModel(string bone, string path)
	{
		if (_accessoryModelPath == path)
		{
			return;
		}
		_accessoryModelPath = path;
		_attachBoneHelper.RemoveAttach("Accessory");
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		Singleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(_accessoryModelPath != path) && !(asset == null))
			{
				_attachBoneHelper.AddAttach("Accessory", bone, (GameObject)asset);
			}
		});
	}

	public void SetAccessoryVisible(bool visible)
	{
		_accessoryVisible = visible;
		UpdateAccessoryVisible();
	}

	private void UpdateAccessoryVisible()
	{
		_attachBoneHelper.SetVisible("Accessory", _accessoryVisible && _visible);
	}

	public void SetVisible(bool visible)
	{
		if (_visible != visible)
		{
			_visible = visible;
			UpdateCostumeVisible(CostumeType.Body);
			UpdateCostumeVisible(CostumeType.Head);
			UpdateCostumeVisible(CostumeType.Hair);
			UpdateCostumeVisible(CostumeType.Beard);
			UpdateAccessoryVisible();
		}
	}

	public bool GetCostumeVisible(CostumeType type)
	{
		if ((int)type < _costumeVisible.Length)
		{
			return _costumeVisible[(int)type];
		}
		return false;
	}

	public void SetCostumeVisible(CostumeType type, bool isVisible)
	{
		if ((int)type < _costumeVisible.Length)
		{
			bool flag = _costumeVisible[(int)type] != isVisible;
			_costumeVisible[(int)type] = isVisible;
			if (type == CostumeType.Head && flag)
			{
				ChangeHairByHeadIndex();
			}
			UpdateCostumeVisible(type);
		}
	}

	private void UpdateCostumeVisible(CostumeType type)
	{
		bool flag = GetCostumeVisible(type) && _visible;
		switch (type)
		{
		case CostumeType.Head:
			flag &= !IsHoodyMode;
			break;
		case CostumeType.Hair:
			flag &= !IsHoodyMode && GetHeadIndex() != 0;
			break;
		}
		string partName = GetPartName(type);
		if (!string.IsNullOrEmpty(partName))
		{
			Transform transform = _transform.Find(partName);
			if (transform != null)
			{
				transform.gameObject.SetActive(flag);
			}
		}
		List<GameObject> particles = GetParticles(type);
		if (particles == null)
		{
			return;
		}
		foreach (GameObject item in particles)
		{
			item.SetActive(flag);
		}
	}

	public void ChangeCostumeColor(CostumeType type, ItemColor color)
	{
		ApplyColorToMaterial(type, color);
		if (type == CostumeType.Hair)
		{
			ApplyColorToMaterial(CostumeType.Beard, color);
		}
		CostumeColors[(int)type] = color;
		if (this.ColorChanged != null)
		{
			this.ColorChanged(type, color);
		}
	}

	private void ApplyColorToMaterial(CostumeType type, ItemColor color)
	{
		if (!color.HasValue || IsNotSuitableType(type))
		{
			return;
		}
		string partName = GetPartName(type);
		if (string.IsNullOrEmpty(partName))
		{
			return;
		}
		GameObject gameObject = KUtility.FindObjectByName(_transform.gameObject, partName, includeInactive: true);
		if (gameObject == null)
		{
			_ = 4;
			return;
		}
		SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
		if (!(component == null))
		{
			bool? threeColor = null;
			switch (type)
			{
			case CostumeType.Skin:
				threeColor = false;
				break;
			case CostumeType.Hair:
				threeColor = false;
				break;
			case CostumeType.Beard:
				threeColor = false;
				break;
			case CostumeType.Body:
				threeColor = true;
				break;
			}
			ApplyColorToRenderer(color, component, threeColor);
		}
	}

	public static void ApplyColorToRenderer(ItemColor color, Renderer renderer, bool? threeColor = null)
	{
		Material[] materials = Prefabs.GetMaterials(renderer);
		int num = materials.Length;
		for (int i = 0; i < num; i++)
		{
			Material material = materials[i];
			if (material == null || material.shader == null)
			{
				continue;
			}
			if (material.HasProperty(ThreeColor1))
			{
				if (!threeColor.HasValue || threeColor.Value)
				{
					material.SetColor(ThreeColor1, color[0]);
					material.SetColor(ThreeColor2, color[1]);
					material.SetColor(ThreeColor3, color[2]);
				}
			}
			else if (!threeColor.HasValue || !threeColor.Value)
			{
				material.SetColor(SubColor, color[0]);
			}
		}
	}

	public string GetCostumeName(CostumeType type)
	{
		return _costumeNames[(int)type];
	}

	public string GetSkinEffect()
	{
		return _skinEffect;
	}

	public void SetSkinEffect(string skinEffect)
	{
		if (!(_skinEffect == skinEffect))
		{
			_skinEffect = skinEffect;
			UpdateSkinEffect();
		}
	}

	private void UpdateSkinEffect()
	{
		if (_skinMaterial != null)
		{
			return;
		}
		Texture skinTexture = GetSkinTexture(_skinEffect, _isMale);
		if (skinTexture == null)
		{
			return;
		}
		Material bodyMaterial = GetBodyMaterial(isNude: true);
		if (bodyMaterial != null)
		{
			bodyMaterial.mainTexture = skinTexture;
		}
		Material bodyMaterial2 = GetBodyMaterial(isNude: false);
		if (!(bodyMaterial2 != null))
		{
			return;
		}
		if (_dirtyTexture == null)
		{
			_dirtyTexture = Resources.Load<Texture2D>("Costume/u_body_dirty");
		}
		bodyMaterial2.DisableKeyword("DIRTY_ON");
		bodyMaterial2.DisableKeyword("VERY_DIRTY_ON");
		string skinEffect = _skinEffect;
		if (!(skinEffect == "dirty"))
		{
			if (skinEffect == "very_dirty")
			{
				bodyMaterial2.EnableKeyword("VERY_DIRTY_ON");
			}
		}
		else
		{
			bodyMaterial2.EnableKeyword("DIRTY_ON");
		}
		bodyMaterial2.SetTexture(DirtyTex, _dirtyTexture);
	}

	public void SetSkinMaterial(Material mat, bool update)
	{
		_skinMaterial = mat;
		if (update)
		{
			UpdateSkinMaterial();
		}
	}

	private void UpdateSkinMaterial()
	{
		if (_skinMaterial == null)
		{
			return;
		}
		string partName = GetPartName(CostumeType.Body);
		if (string.IsNullOrEmpty(partName))
		{
			return;
		}
		Transform transform = _transform.Find(partName);
		if (transform == null)
		{
			return;
		}
		SkinnedMeshRenderer component = transform.GetComponent<SkinnedMeshRenderer>();
		Material[] materials = Prefabs.GetMaterials(component);
		int num = materials.Length;
		for (int i = 0; i < num; i++)
		{
			Material material = materials[i];
			if (!(material == null) && !(material.mainTexture == null) && material.mainTexture.name.Contains("body_nude"))
			{
				materials[i] = _skinMaterial;
				component.sharedMaterials = materials;
				break;
			}
		}
	}

	private static Texture GetSkinTexture(string effect, bool isMale)
	{
		string text = ((!isMale) ? "f_body_nude01_" : "m_body_nude01_");
		text = ((!string.IsNullOrEmpty(effect)) ? (text + effect) : (text + "clean"));
		Texture texture = SkinTextures.Get(text);
		if (texture == null)
		{
			texture = Resources.Load<Texture2D>("Costume/" + text);
			SkinTextures[text] = texture;
		}
		return texture;
	}

	[CanBeNull]
	private Material GetBodyMaterial(bool isNude)
	{
		string partName = GetPartName(CostumeType.Body);
		if (string.IsNullOrEmpty(partName))
		{
			return null;
		}
		Transform transform = _transform.Find(partName);
		if (transform == null)
		{
			return null;
		}
		Material[] materials = Prefabs.GetMaterials(transform.GetComponent<SkinnedMeshRenderer>());
		int num = materials.Length;
		for (int i = 0; i < num; i++)
		{
			Material material = materials[i];
			if (!(material == null) && !(material.mainTexture == null) && material.mainTexture.name.Contains("body_nude") == isNude)
			{
				return material;
			}
		}
		return null;
	}

	private void AttachBoneHelper_VisibledChanged(GameObject obj, bool visible)
	{
		if (_shadows != null)
		{
			if (visible)
			{
				_shadows.Add(obj.GetComponentsInChildren<SkinnedMeshRenderer>());
			}
			else
			{
				_shadows.Remove(obj.GetComponentsInChildren<SkinnedMeshRenderer>());
			}
		}
	}

	private void UpdateJiggleBones(CostumeType type, GameObject obj)
	{
		if (_jiggleController != null)
		{
			_jiggleController.Remove(type);
			JiggleBonesController component = obj.GetComponent<JiggleBonesController>();
			if (component != null)
			{
				_jiggleController.Add(component, _rootBone.GetComponentsInChildren<Transform>(), type);
			}
		}
	}

	private void UpdateParticles(CostumeType type, GameObject obj)
	{
		if (type != 0 && type != CostumeType.Head)
		{
			return;
		}
		ClearParticles(type);
		using Reusable<List<Transform>> reusable = ReusableList<Transform>.Pop();
		List<Transform> value = reusable.Value;
		obj.GetComponentsInChildren(includeInactive: true, value);
		foreach (Transform item in value)
		{
			if (item.name.StartsWith("fx_control", StringComparison.OrdinalIgnoreCase))
			{
				AddParticle(type, item);
			}
		}
	}

	private void AddParticle(CostumeType type, Transform src)
	{
		List<GameObject> particles = GetParticles(type, create: true);
		if (particles != null)
		{
			string name = src.parent.name;
			Transform transform = KUtility.FindTransformByName(_rootBone.gameObject, name, includeInactive: true);
			if (transform == null)
			{
				transform = _transform;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(src.gameObject, transform);
			gameObject.transform.localPosition = src.localPosition;
			gameObject.transform.localRotation = src.localRotation;
			gameObject.transform.localScale = src.localScale;
			gameObject.SetActive(value: true);
			particles.Add(gameObject);
		}
	}

	[CanBeNull]
	private List<GameObject> GetParticles(CostumeType type, bool create = false)
	{
		if (type != 0 && type != CostumeType.Head)
		{
			return null;
		}
		if (create)
		{
			if (_particles == null)
			{
				_particles = new List<GameObject>[2];
			}
			List<GameObject> list = _particles[(int)type];
			if (list == null)
			{
				list = new List<GameObject>();
				_particles[(int)type] = list;
			}
			return list;
		}
		if (_particles != null)
		{
			return _particles[(int)type];
		}
		return null;
	}

	private void ClearParticles(CostumeType type)
	{
		List<GameObject> particles = GetParticles(type);
		if (particles == null)
		{
			return;
		}
		foreach (GameObject item in particles)
		{
			UnityEngine.Object.Destroy(item);
		}
		particles.Clear();
	}
}
