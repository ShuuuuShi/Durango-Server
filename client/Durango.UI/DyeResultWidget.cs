using System;
using Durango.Logic.Item;
using Durango.Model;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class DyeResultWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _modelWidget;

	[SerializeField]
	private UITexture _modelTexture;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _successRatioLabel;

	[SerializeField]
	private UILabel _durabilityLabel;

	[SerializeField]
	private UILabel _modifiableCountLabel;

	private UIModelRender _uiModelRender;

	private PlayerBehavior _previewModel;

	private Vector3 _previewModelScale;

	private string _nameFormat;

	private string _successRatioFormat;

	private string _durabilityFormat;

	private string _modifiableCountFormat;

	private string _modelPath;

	private string _iconSprite;

	private bool _isEquipments;

	private PlayerBehavior.WeaponFramework _weaponFramework;

	private CharacterCostume.CostumeType _costumeType;

	private ItemColor _color;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIEventListener uIEventListener = UIEventListener.Get(_modelWidget.gameObject);
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragModelWidget));
			_nameFormat = _nameLabel.text;
			_successRatioFormat = _successRatioLabel.text;
			_durabilityFormat = _durabilityLabel.text;
			_modifiableCountFormat = _modifiableCountLabel.text;
		}
	}

	private void Reset()
	{
		_modelPath = null;
		_iconSprite = null;
		if (_previewModel != null)
		{
			_previewModel.gameObject.SetActive(value: false);
		}
		ResetEstimate();
	}

	public void SetUnknownModel()
	{
		Init();
		Reset();
		_nameLabel.text = string.Empty;
		_iconTexture.SetIcon("icon_question");
		_iconTexture.gameObject.SetActive(value: true);
	}

	public void SetModel(ItemData item)
	{
		Init();
		string model = item.GetModel(PlayerBehavior.LocalPlayer.IsMale);
		_nameLabel.text = T._(_nameFormat, item.Name, item.Level);
		if (string.IsNullOrEmpty(model))
		{
			string main = item.Icon.Main;
			if (!string.Equals(_iconSprite, main))
			{
				Reset();
				_iconSprite = main;
				_iconTexture.gameObject.SetActive(value: true);
			}
		}
		else
		{
			if (string.Equals(_modelPath, model))
			{
				return;
			}
			Reset();
			_modelPath = model;
			_iconTexture.gameObject.SetActive(value: false);
			if (_previewModel == null)
			{
				_uiModelRender = UIModelRenderBuilder.Make();
				_previewModel = Singleton<PlayerManager>.Instance().MakePreview(PlayerBehavior.LocalPlayer.IsMale);
				_uiModelRender.SetModel(_previewModel.gameObject, 35f);
				_previewModelScale = _previewModel.transform.localScale;
				_uiModelRender.FillTexture(_modelTexture);
			}
			_previewModel.gameObject.SetActive(value: true);
			PlayerDisplay display = PlayerBehavior.LocalPlayer.Display;
			string stringAttribute = item.GetStringAttribute("weapon_framework");
			if (string.IsNullOrEmpty(stringAttribute))
			{
				_isEquipments = false;
				_weaponFramework = PlayerBehavior.WeaponFramework.BAREHAND;
				_costumeType = CharacterCostume.GetCostumeType(_modelPath);
				switch (_costumeType)
				{
				case CharacterCostume.CostumeType.Head:
					display.Head = model;
					break;
				case CharacterCostume.CostumeType.Body:
					display.Body = model;
					break;
				case CharacterCostume.CostumeType.Beard:
					display.Beard = model;
					break;
				case CharacterCostume.CostumeType.Hair:
					display.Hair = model;
					break;
				}
				display.Equip = null;
			}
			else
			{
				_isEquipments = true;
				_weaponFramework = stringAttribute.ToEnum(PlayerBehavior.WeaponFramework.BAREHAND);
				display.Equip = _modelPath;
			}
			PlayerManager.SetDisplay(_previewModel, display);
			_previewModel.ChangeWeaponType(_weaponFramework);
			_previewModel.transform.localScale = Vector3.zero;
			KUtility.DelayedCall(this, OnPostUpdatePreview, 0.1f);
		}
	}

	private void OnPostUpdatePreview()
	{
		if (_previewModel != null)
		{
			_previewModel.transform.localScale = _previewModelScale;
		}
	}

	public void SetColor(ItemColor col)
	{
		_color = col;
		UpdateColor();
	}

	public void SetEstimate(CraftEstimation? res)
	{
		if (res.HasValue)
		{
			CraftEstimation value = res.Value;
			_successRatioLabel.text = string.Format(_successRatioFormat, value.SuccessRate);
			_durabilityLabel.text = string.Format(_durabilityFormat, value.Durability.x, value.Durability.y);
			_modifiableCountLabel.text = string.Format(_modifiableCountFormat, value.ModifiableCount);
		}
	}

	public void ResetEstimate()
	{
		_successRatioLabel.text = string.Empty;
		_durabilityLabel.text = string.Empty;
		_modifiableCountLabel.text = string.Empty;
	}

	private void UpdateColor()
	{
		if (!string.IsNullOrEmpty(_modelPath))
		{
			if (_isEquipments)
			{
				_previewModel.ChangeEquipmentColor(_color);
			}
			else
			{
				_previewModel.ChangeCostumeColor(_costumeType, _color);
			}
		}
		if (!string.IsNullOrEmpty(_iconSprite))
		{
			_iconTexture.SetIcon(_iconSprite, _color);
		}
	}

	private void OnDragModelWidget(GameObject obj, Vector2 delta)
	{
		if (_previewModel != null && _previewModel.gameObject.activeSelf)
		{
			Transform transform = _previewModel.transform;
			transform.Rotate(transform.up, 0f - delta.x, Space.World);
		}
	}

	private void OnDisable()
	{
		_modelPath = null;
		_iconSprite = null;
		UIModelRenderBuilder.Release(_uiModelRender);
		_uiModelRender = null;
	}
}
