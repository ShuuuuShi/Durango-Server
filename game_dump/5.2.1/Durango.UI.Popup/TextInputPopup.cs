using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class TextInputPopup : TooltipBase
{
	private Action<string> _onSubmit;

	[SerializeField]
	private UIInput _input;

	[SerializeField]
	private SelectableButton _confirmBtn;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private UIWidget _textInputWidget;

	private string _comment = string.Empty;

	private string _buttonText = string.Empty;

	private int _inputHeight;

	protected override void OnAwake()
	{
		SoundType = UISound.GroupType.NoSound;
		EventDelegate.Add(_input.onSubmit, OnSubmit);
		EventDelegate.Add(_input.onChange, UpdateInputHeight);
		_confirmBtn.Clicked = OnSubmit;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_input.isSelected = true;
	}

	protected override void FillData()
	{
		if (string.IsNullOrEmpty(_comment))
		{
			_commentLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_commentLabel.gameObject.SetActive(value: true);
			_commentLabel.text = _comment;
		}
		_confirmBtn.Text = ((!string.IsNullOrEmpty(_buttonText)) ? _buttonText : T._("확인"));
	}

	protected override void UpdateLayout()
	{
		Vector3 localPosition = _commentLabel.transform.localPosition;
		if (!string.IsNullOrEmpty(_comment))
		{
			localPosition.y -= _commentLabel.height + 30;
		}
		_textInputWidget.transform.localPosition = localPosition;
		UpdateInputHeight();
		if (!base.gameObject.activeSelf)
		{
			Vector3 vector = (float)UIManager.SafeHeight * 0.5f * Vector3.up;
			base.transform.localPosition = vector + Vector3.up * base.Widget.height;
			TweenPosition.Begin(base.gameObject, 0.3f, vector);
		}
		else
		{
			UIUtility.UpdateAnchors(base.transform);
		}
	}

	private void UpdateInputHeight()
	{
		int height = _input.label.height;
		if (height != _inputHeight)
		{
			_inputHeight = height;
			_textInputWidget.height = _inputHeight + 20;
			base.Widget.SetDimensions(UIManager.ScreenWidth, (int)Mathf.Abs(_textInputWidget.GetPosition(0f, 0f).y) + 60);
			UIUtility.UpdateAnchors(base.transform);
		}
	}

	private void OnSubmit()
	{
		if (_onSubmit != null)
		{
			_onSubmit(_input.value);
		}
		Hide();
	}

	public void Show(Action<string> onSubmit, string comment = null, string defaultValue = null, bool isMultiline = false, string buttonText = null, int limitTextCount = 140)
	{
		_onSubmit = onSubmit;
		_comment = ((comment == null) ? string.Empty : comment);
		_input.onReturnKey = ((!isMultiline) ? UIInput.OnReturnKey.Submit : UIInput.OnReturnKey.NewLine);
		_input.label.multiLine = isMultiline;
		_input.label.overflowMethod = ((!isMultiline) ? UILabel.Overflow.ClampContent : UILabel.Overflow.ResizeHeight);
		_input.characterLimit = limitTextCount;
		if (!isMultiline)
		{
			_input.label.height = _input.label.fontSize;
		}
		_input.value = defaultValue;
		_buttonText = buttonText;
		Show();
	}

	protected override void OnTryConfirmOnModal()
	{
		OnSubmit();
	}
}
