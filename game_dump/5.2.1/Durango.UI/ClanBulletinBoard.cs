using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ClanBulletinBoard : MonoBehaviour
{
	[SerializeField]
	private UIWidget _contentsContainer;

	[SerializeField]
	private UIWidget _mainTextContainer;

	[SerializeField]
	private UILabel _mainTextLabel;

	[SerializeField]
	private GameObject _defaultTextLabel;

	[SerializeField]
	private UIWidget _editorContainer;

	[SerializeField]
	private UIInput _editorInput;

	[SerializeField]
	private UILabel _wordLimitLabel;

	[SerializeField]
	private UIWidget _editorButton;

	[SerializeField]
	private SelectableButton _sendButton;

	private bool _isEditing;

	private bool _isLineBreakable;

	private Action _sendButtonClicked;

	private int _wordLimit;

	public string Text
	{
		get
		{
			return _mainTextLabel.text;
		}
		set
		{
			_mainTextLabel.text = ((!_isLineBreakable) ? value.Replace('\n', ' ').Replace('\r', ' ') : value);
			bool flag = string.IsNullOrEmpty(value);
			_mainTextContainer.gameObject.SetActive(!flag);
			_defaultTextLabel.gameObject.SetActive(flag);
		}
	}

	public void Init(string defaultText, int wordLimit, Action sendButtonClicked, bool isLineBreakable = true)
	{
		UIEventListener uIEventListener = UIEventListener.Get(_editorButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(EditButton_Clicked));
		EventDelegate.Add(_editorInput.onChange, UpdateWordLimitLabel);
		_sendButton.Clicked = SendButton_Clicked;
		_editorInput.defaultText = defaultText;
		_sendButtonClicked = sendButtonClicked;
		_isLineBreakable = isLineBreakable;
		_wordLimit = wordLimit;
		SetEditMode(editMode: false);
	}

	public bool Back()
	{
		if (!_isEditing)
		{
			return true;
		}
		SetEditMode(editMode: false);
		return false;
	}

	public void SetEditMode(bool editMode)
	{
		if (editMode)
		{
			_editorInput.value = _mainTextLabel.text;
			UpdateWordLimitLabel();
		}
		_isEditing = editMode;
		_wordLimitLabel.gameObject.SetActive(editMode);
		_contentsContainer.gameObject.SetActive(!editMode);
		_editorContainer.gameObject.SetActive(editMode);
	}

	private void UpdateWordLimitLabel()
	{
		string value = _editorInput.value;
		int num = ((!string.IsNullOrEmpty(value)) ? value.Length : 0);
		if (num == 0)
		{
			_wordLimitLabel.text = $"{num}/{_wordLimit}";
		}
		else if (num > _wordLimit)
		{
			_wordLimitLabel.text = $"<alert>{num}</alert>/{_wordLimit}";
		}
		else
		{
			_wordLimitLabel.text = $"<em>{num}</em>/{_wordLimit}";
		}
	}

	private void EditButton_Clicked(GameObject obj)
	{
		SetEditMode(!_isEditing);
	}

	private void SendButton_Clicked()
	{
		if (_isEditing && (string.IsNullOrEmpty(_editorInput.value) || _editorInput.value.Length <= _wordLimit))
		{
			Text = _editorInput.value;
			if (_sendButtonClicked != null)
			{
				_sendButtonClicked();
			}
		}
	}
}
