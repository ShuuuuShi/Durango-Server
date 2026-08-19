using System;
using L10N;
using UnityEngine;

public class ChattingInputControl : MonoBehaviour
{
	public Action<string> Submitted;

	[SerializeField]
	private DefaultSelectableButton _sendButton;

	[SerializeField]
	private UIInput _inputLabel;

	[SerializeField]
	private int _buttonPadding;

	[SerializeField]
	private int _buttonMargin;

	public void Init()
	{
		EventDelegate.Add(_inputLabel.onSubmit, OnSubmit);
		_sendButton.Clicked = OnSubmit;
		OnLocalize();
	}

	private void OnDisable()
	{
		_inputLabel.RemoveFocus();
	}

	private void OnSubmit()
	{
		if (Submitted != null)
		{
			Submitted(_inputLabel.value);
		}
		_inputLabel.value = string.Empty;
	}

	public void FocusInputText(bool hasFocus)
	{
		_inputLabel.isSelected = hasFocus;
	}

	private void OnLocalize()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		string text = T._("전송");
		UILabel textLabel = _sendButton.TextLabel;
		textLabel.UpdateNGUIText();
		NGUIText.regionWidth = 1280;
		int num = (int)NGUIText.CalculatePrintedSize(text).x + _buttonPadding;
		_sendButton.Widget.leftAnchor.absolute = _sendButton.Widget.rightAnchor.absolute - num;
		textLabel.text = text;
		NGUITools.UpdateWidgetCollider(((Component)_sendButton).gameObject);
		_inputLabel.label.rightAnchor.absolute = _sendButton.Widget.leftAnchor.absolute - _buttonMargin;
		_inputLabel.defaultText = T._("대화 내용을 입력하세요.");
	}
}
