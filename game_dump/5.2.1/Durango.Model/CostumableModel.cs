using Durango.Render;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Model;

[ExecuteInEditMode]
public class CostumableModel : MonoBehaviour, ICostumable
{
	[SerializeField]
	private GameObject _meshObject;

	[SerializeField]
	private bool _isMale;

	private string _equipmentName;

	private ItemColor _equipmentColor;

	private GameObject _equipmentObj;

	private BoneMergeable _boneMergeable;

	private readonly CharacterCostume _costume = new CharacterCostume();

	public bool IsMale => _isMale;

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

	public Transform WeaponTipTransform { get; private set; }

	[NotNull]
	private BoneMergeable BoneMergeable
	{
		get
		{
			if (_boneMergeable == null)
			{
				CharacterBehavior component = GetComponent<CharacterBehavior>();
				if (component != null)
				{
					_boneMergeable = component.BoneMergeable;
				}
				if (_boneMergeable == null)
				{
					Transform transform = KUtility.FindTransformByName(base.gameObject, "Bip001");
					_boneMergeable = new BoneMergeable(base.gameObject, MeshObjectTransform, (!(transform != null)) ? base.transform : transform);
				}
			}
			return _boneMergeable;
		}
	}

	[CanBeNull]
	private GameObject MeshObject
	{
		get
		{
			if (_meshObject != null)
			{
				return _meshObject;
			}
			Animation componentInChildren = base.gameObject.GetComponentInChildren<Animation>(includeInactive: true);
			if (componentInChildren == null)
			{
				return null;
			}
			_meshObject = componentInChildren.gameObject;
			return _meshObject;
		}
	}

	[CanBeNull]
	private Transform MeshObjectTransform
	{
		get
		{
			if (MeshObject != null)
			{
				return MeshObject.transform;
			}
			return null;
		}
	}

	private void Awake()
	{
		_costume.Init(_isMale, MeshObject, GetComponent<JiggleBonesController>(), GetComponent<PlaneShadows>());
		_costume.ModelChanged += Costume_ModelChanged;
	}

	public void ChangeCostume(CharacterCostume.CostumeType type, string fileName)
	{
		_costume.ChangeCostume(type, fileName);
	}

	public string GetCostumeName(CharacterCostume.CostumeType type)
	{
		return _costume.GetCostumeName(type);
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
		_equipmentName = path;
		if (string.IsNullOrEmpty(path))
		{
			DetachEquipment();
			return;
		}
		Singleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), delegate(Object asset)
		{
			if (!(this == null))
			{
				DetachEquipment();
				if (!(asset == null))
				{
					AttachEquipment((GameObject)Object.Instantiate(asset));
				}
			}
		});
	}

	public string GetEquipmentName()
	{
		return _equipmentName;
	}

	public void ChangeEquipmentColor(ItemColor color)
	{
		_equipmentColor = color;
		ApplyEquipmentColor();
	}

	public ItemColor GetEquipmentColor()
	{
		return _equipmentColor;
	}

	public void ChangeAccessory(string bone, string path)
	{
		_costume.SetAccessoryModel(bone, path);
	}

	public void SetSkinMaterial(Material mat, bool update = false)
	{
		_costume.SetSkinMaterial(mat, update);
	}

	private void AttachEquipment([NotNull] GameObject equipObj)
	{
		_equipmentObj = equipObj;
		BoneMergeable.AttachBoneMerge(_equipmentObj);
		ApplyEquipmentColor();
		UpdateWeaponTip();
	}

	private void DetachEquipment()
	{
		if (!(_equipmentObj == null))
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
			UpdateWeaponTip();
		}
	}

	private void ApplyEquipmentColor()
	{
		if (!(_equipmentObj == null))
		{
			SkinnedMeshRenderer componentInChildren = _equipmentObj.GetComponentInChildren<SkinnedMeshRenderer>();
			if (!(componentInChildren == null))
			{
				CharacterCostume.ApplyColorToRenderer(_equipmentColor, componentInChildren);
			}
		}
	}

	private void UpdateWeaponTip()
	{
		if (_equipmentObj == null)
		{
			WeaponTipTransform = MeshObjectTransform;
			return;
		}
		GameObject gameObject = KUtility.FindObjectByName(_equipmentObj, "Weapon_Tip");
		if (gameObject != null)
		{
			WeaponTipTransform = gameObject.transform;
			return;
		}
		Transform transform = KUtility.FindTransformByName(_equipmentObj, "Attachment_RH");
		if ((bool)transform)
		{
			gameObject = new GameObject("Weapon_Tip");
			gameObject.transform.parent = transform;
			gameObject.transform.localPosition = new Vector3(-100f, 0f, 0f);
			WeaponTipTransform = gameObject.transform;
		}
		else
		{
			WeaponTipTransform = base.transform;
		}
	}

	private void Costume_ModelChanged()
	{
		if (Application.isPlaying)
		{
			PlaneShadows component = GetComponent<PlaneShadows>();
			if (component != null)
			{
				component.RefreshModel();
			}
		}
	}
}
