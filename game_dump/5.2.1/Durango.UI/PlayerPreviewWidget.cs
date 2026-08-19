using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class PlayerPreviewWidget : MonoBehaviour
{
	private const float DampingRatio = 1f;

	private const float Frequency = 6.5f;

	[SerializeField]
	private UIWidget _previewContainer;

	[SerializeField]
	private UITexture _previewTexture;

	[CanBeNull]
	[SerializeField]
	private UILabel _selectCostumeDescription;

	private UIModelRender _uiModelRender;

	private PlayerBehavior _previewModel;

	private float _velocity;

	private string _reservedMotion;

	private void Start()
	{
		_previewContainer.AddOnChange(UpdateTextureSize);
	}

	private void OnDisable()
	{
		DestoryPreviewModel();
		_reservedMotion = null;
	}

	private void Update()
	{
		if (!(_uiModelRender == null) && !(_previewModel == null))
		{
			Vector3 position = _previewModel.Bip001Transform.position;
			float target = (0f - _uiModelRender.ModelTransform.InverseTransformPoint(position).x) * _uiModelRender.ModelTransform.localScale.x;
			float x = Maths.CalculateSpring(_uiModelRender.ModelTransform.localPosition.x, target, ref _velocity, 1f, 6.5f, Time.deltaTime);
			_uiModelRender.ModelTransform.localPosition = _uiModelRender.ModelTransform.localPosition.WithX(x);
		}
	}

	public void SetModelVisibility(bool isShow)
	{
		_previewTexture.enabled = isShow;
		if (_selectCostumeDescription != null)
		{
			_selectCostumeDescription.gameObject.SetActive(!isShow);
		}
	}

	public void Set(float scale)
	{
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(GameManager.PlayerId, delegate(PlayerInfo info)
		{
			if (info.Valid)
			{
				MakePreviewModel(info, scale);
			}
		});
	}

	private void MakePreviewModel(PlayerInfo info, float scale)
	{
		if (!(_previewModel != null))
		{
			_previewModel = Singleton<PlayerManager>.Instance().MakePreview(info.IsMale, info.Display);
			_uiModelRender = UIModelRenderBuilder.Make();
			_uiModelRender.SetModel(_previewModel.gameObject, 35f, scale);
			_previewModel.UpdateBodyScale();
			UpdateTextureSize();
			if (!string.IsNullOrEmpty(_reservedMotion))
			{
				string reservedMotion = _reservedMotion;
				_reservedMotion = null;
				PlayMotion(reservedMotion);
			}
		}
	}

	private void UpdateTextureSize()
	{
		if (!(_uiModelRender == null))
		{
			_previewTexture.SetDimensions(Mathf.Min(_previewContainer.width, (int)((float)_previewContainer.height * 1.4f)), _previewContainer.height);
			_uiModelRender.FillTexture(_previewTexture);
		}
	}

	private void DestoryPreviewModel()
	{
		_previewTexture.mainTexture = null;
		UIModelRenderBuilder.Release(_uiModelRender);
		_uiModelRender = null;
	}

	public void PlayMotion(string motionClipName)
	{
		if (_previewModel == null)
		{
			_reservedMotion = motionClipName;
			return;
		}
		_previewModel.PlayMotionsForcely(1f, motionClipName);
	}
}
