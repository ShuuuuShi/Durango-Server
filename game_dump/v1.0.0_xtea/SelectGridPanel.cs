using System;
using Building_;
using L10N;
using UnityEngine;

public class SelectGridPanel : MonoBehaviour
{
	[SerializeField]
	private UIWidget _commentWidget;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private Transform _buttonContainer;

	[SerializeField]
	private SelectableWidget _confirmGridSelectionButton;

	[SerializeField]
	private SelectableWidget _cancelGridSelectionButton;

	[SerializeField]
	private SelectableWidget _rotatePreviewButton;

	private Blueprint _blueprint;

	public event Action<Blueprint> ConfirmedBlueprint;

	public event Action Canceled;

	private void Awake()
	{
		_confirmGridSelectionButton.Clicked = ConfirmGridSelection_OnClick;
		_cancelGridSelectionButton.Clicked = CancelGridSelection_OnClick;
		_rotatePreviewButton.Clicked = RotatePreview_OnClick;
	}

	private void OnEnable()
	{
		if (KSingleton<BuildManager>.Exist())
		{
			KSingleton<BuildManager>.Instance().PreviewPositionUpdated += BuildManager_PreviewPositionUpdated;
		}
	}

	private void OnDisable()
	{
		if (KSingleton<BuildManager>.HasInstance())
		{
			KSingleton<BuildManager>.Instance().PreviewPositionUpdated -= BuildManager_PreviewPositionUpdated;
		}
	}

	private void ConfirmGridSelection_OnClick()
	{
		if (_confirmGridSelectionButton.Disable)
		{
			return;
		}
		if (BuildManager.CurrentGridMaxState == BuildManager.BuildGridState.Estate && BuildManager.CurrentGridMinState < BuildManager.CurrentGridMaxState)
		{
			UIManager.MessageBox.Show(T._("<alert>주의</alert>\n건물의 일부가 사유지 밖으로 나가므로 완전한 권리를 보호받을 수 없게 됩니다.\n이대로 진행하시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					OnConfirm();
				}
			});
		}
		else
		{
			OnConfirm();
		}
	}

	private void OnConfirm()
	{
		Hide();
		if (this.ConfirmedBlueprint != null)
		{
			this.ConfirmedBlueprint(_blueprint);
		}
	}

	private void CancelGridSelection_OnClick()
	{
		Hide();
		if (this.Canceled != null)
		{
			this.Canceled();
		}
	}

	private void RotatePreview_OnClick()
	{
		if (!_rotatePreviewButton.Disable)
		{
			KSingleton<BuildManager>.Instance().RotatePreview();
		}
	}

	public void Show(Blueprint blueprint, Point2 size)
	{
		Show();
		_blueprint = blueprint;
		if (_blueprint.IsClanEstateFlag)
		{
			SetComment(T._("영토 확장은 기존의 영토와 인접한 곳에 할 수 있습니다"));
		}
		else
		{
			SetComment(null);
		}
		_rotatePreviewButton.Widget.alpha = ((!_blueprint.RotationDisabled) ? 1f : 0.5f);
		KSingleton<BuildManager>.Instance().SetArtifactBuildingMode(blueprint, size);
	}

	private void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
		KSingleton<BuildManager>.Instance().ResetBuildingMode();
	}

	private void SetComment(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			((Component)_commentWidget).gameObject.SetActive(false);
		}
		((Component)_commentWidget).gameObject.SetActive(true);
		_commentLabel.text = text;
	}

	private void BuildManager_PreviewPositionUpdated(Vector3 position, Point2 size)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		_confirmGridSelectionButton.Widget.alpha = ((BuildManager.CurrentGridMinState != 0) ? 1f : 0.5f);
		Vector3 localPosition = MainCamera.WorldToNGUIPos(position + (Vector3.left + Vector3.back) * 0.5f * 200f);
		float num = MainCamera.NGUIScale();
		localPosition.x = Mathf.Clamp(localPosition.x, (float)(-Screen.width) * num * 0.5f + 150f, (float)Screen.width * num * 0.5f - 150f);
		localPosition.y = Mathf.Clamp(localPosition.y, (float)(-Screen.height) * num * 0.5f + 150f, (float)Screen.height * num * 0.5f - 150f);
		_buttonContainer.localPosition = localPosition;
	}
}
