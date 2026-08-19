using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class EditPlayerNamePage : MonoBehaviour, IEditPlayerDisplayPage
{
	[SerializeField]
	private Transform _modelViewer;

	[SerializeField]
	private UIInput _textInput;

	[SerializeField]
	private SelectableButton _finishButton;

	[SerializeField]
	private UILabel _nameRoleHelpLabel;

	private AnimationWidget _animWidget;

	public string Name { get; private set; }

	public event Action Confirmed;

	public void Initialize(EditPlayerDisplayProxy display)
	{
		_animWidget = AnimationWidget.Get(base.gameObject, 0.3f, 0f, deactiveWhenFadeout: true);
		_nameRoleHelpLabel.text = $"[icon=icon_question_big] {LocalizeUtil.GetNameRoleHelpText()}";
		UIEventListener.Get(_nameRoleHelpLabel.gameObject).onClick = OnClickExplainDetail;
		_finishButton.Text = T._("완료");
		_finishButton.SetClickSound(UISound.ClickType.ButtonHighlight);
		_finishButton.Clicked = delegate
		{
			Name = _textInput.value;
			if (string.IsNullOrEmpty(Name))
			{
				if (!Debug.isDebugBuild)
				{
					return;
				}
				int num = UnityEngine.Random.Range(10, 15);
				char[] array = new char[num];
				for (int i = 0; i < num; i++)
				{
					if (UnityEngine.Random.Range(0, 2) == 0)
					{
						array[i] = (char)UnityEngine.Random.Range(97, 122);
					}
					else
					{
						array[i] = (char)UnityEngine.Random.Range(65, 90);
					}
				}
				Name = new string(array);
				_textInput.value = Name;
			}
			if (this.Confirmed != null)
			{
				this.Confirmed();
			}
		};
		EventDelegate.Set(_textInput.onChange, NameChanged);
	}

	public void Show(bool instant)
	{
		base.gameObject.SetActive(value: true);
		_animWidget.SetAlpha(1f, !instant);
		WaitForLoading(loading: false);
	}

	public void Hide(bool instant)
	{
		_animWidget.SetAlpha(0f, !instant);
	}

	public Transform GetModelPosition()
	{
		return _modelViewer;
	}

	public void SetConfirmText(string text)
	{
		_finishButton.Text = text;
	}

	private void NameChanged()
	{
		string text = Regex.Replace(Regex.Replace(_textInput.value, "[\\[|\\]]", string.Empty), "\\s\\s+", " ");
		if (IsNotAlphabet(text) && text.Length > 10)
		{
			_textInput.value = text.Substring(0, 10);
		}
		else if (text.Length != _textInput.value.Length)
		{
			_textInput.value = text;
		}
	}

	private static bool IsNotAlphabet(string value)
	{
		int length = value.Length;
		for (int i = 0; i < length; i++)
		{
			if (char.GetUnicodeCategory(value[i]) == UnicodeCategory.OtherLetter)
			{
				return true;
			}
		}
		return false;
	}

	private void OnClickExplainDetail(GameObject go)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, LocalizeUtil.GetNameRoleDescription());
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Show(10f);
	}

	public void WaitForLoading(bool loading)
	{
		UIManager.ShowLoadingIcon(loading);
		_finishButton.Disabled = loading;
	}
}
