using System;
using ItemSystem;
using L10N;
using Messages;
using UnityEngine;

public class DyeResultWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _modelWidget;

	[SerializeField]
	private Transform _modelContainer;

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

	private PlayerBehavior _previewModel;

	private string _nameFormat;

	private string _successRatioFormat;

	private string _durabilityFormat;

	private string _modifiableCountFormat;

	private string _modelPath;

	private string _iconSprite;

	private ItemColor _color;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIEventListener uIEventListener = UIEventListener.Get(((Component)_modelWidget).gameObject);
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
		if ((Object)(object)_previewModel != (Object)null)
		{
			((Component)_previewModel).gameObject.SetActive(false);
		}
		ResetEstimate();
	}

	public void SetUnknownModel()
	{
		Init();
		Reset();
		_nameLabel.text = string.Empty;
		_iconTexture.SetIcon("icon_question");
		((Component)_iconTexture).gameObject.SetActive(true);
	}

	public void SetModel(ItemData item)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		Init();
		string itemModel = Util.GetItemModel(item, PlayerBehavior.LocalPlayer.IsMale);
		_nameLabel.text = T.Format(_nameFormat, item.Name, item.Level);
		if (string.IsNullOrEmpty(itemModel))
		{
			if (!string.Equals(_iconSprite, item.Icon))
			{
				Reset();
				_iconSprite = item.Icon;
				((Component)_iconTexture).gameObject.SetActive(true);
			}
		}
		else if (!string.Equals(_modelPath, itemModel))
		{
			Reset();
			_modelPath = itemModel;
			((Component)_iconTexture).gameObject.SetActive(false);
			if ((Object)(object)_previewModel == (Object)null)
			{
				_previewModel = KSingleton<PlayerManager>.Instance().MakePlayerObject(PlayerBehavior.LocalPlayer.IsMale, Vector3.zero, 0uL, isPreview: true);
				((Component)_previewModel).transform.SetParent(_modelContainer);
				((Component)_previewModel).transform.localPosition = Vector3.zero;
				((Component)_previewModel).transform.localScale = Vector3.one;
				((Component)_previewModel).transform.localRotation = Quaternion.Euler(0f, 200f, 0f);
				_previewModel.IsPlaneShadowEnabled = false;
				NGUITools.SetLayer(((Component)_previewModel).gameObject, LayerMask.NameToLayer("NGUI"));
			}
			((Component)_previewModel).gameObject.SetActive(true);
			PlayerDisplay display = PlayerBehavior.LocalPlayer.Display;
			CharacterCostume.CostumeType costumeType = CharacterCostume.GetCostumeType(_modelPath);
			switch (costumeType)
			{
			case CharacterCostume.CostumeType.Equipment:
				display.Equip = itemModel;
				break;
			case CharacterCostume.CostumeType.Head:
				display.Head = itemModel;
				break;
			case CharacterCostume.CostumeType.Body:
				display.Body = itemModel;
				break;
			case CharacterCostume.CostumeType.Beard:
				display.Beard = itemModel;
				break;
			case CharacterCostume.CostumeType.Hair:
				display.Hair = itemModel;
				break;
			}
			if (costumeType != CharacterCostume.CostumeType.Equipment)
			{
				display.Equip = null;
			}
			PlayerManager.SetCostume(_previewModel, display);
		}
	}

	public void SetColor(ItemColor col)
	{
		_color = col;
		UpdateColor();
	}

	public void SetEstimate(CraftEstimation estimation)
	{
		_successRatioLabel.text = string.Format(_successRatioFormat, estimation.SuccessRate);
		_durabilityLabel.text = string.Format(_durabilityFormat, estimation.Durability.x, estimation.Durability.y);
		_modifiableCountLabel.text = string.Format(_modifiableCountFormat, estimation.ModifiableCount);
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
			CharacterCostume.CostumeType costumeType = CharacterCostume.GetCostumeType(_modelPath);
			_previewModel.ChangeCostumeColor(costumeType, _color);
		}
		if (!string.IsNullOrEmpty(_iconSprite))
		{
			_iconTexture.SetIcon(_iconSprite, _color);
		}
	}

	private void OnDragModelWidget(GameObject obj, Vector2 vec)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_previewModel != (Object)null && ((Component)_previewModel).gameObject.activeSelf)
		{
			Vector3 localEulerAngles = ((Component)_previewModel).transform.localEulerAngles;
			localEulerAngles.y += vec.x;
			((Component)_previewModel).transform.localEulerAngles = localEulerAngles;
		}
	}

	private void OnDisable()
	{
		_modelPath = null;
		_iconSprite = null;
		if ((Object)(object)_previewModel != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_previewModel).gameObject);
			_previewModel = null;
		}
	}
}
