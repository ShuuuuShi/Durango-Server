using System;
using Durango.System.Config;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class ToggleWidget : MonoBehaviour
{
	[SerializeField]
	public UILabel Text;

	[SerializeField]
	public GameObject Left;

	[SerializeField]
	public GameObject Right;

	public Action<string> ValueChanged;

	private string[] _options;

	public int Index
	{
		get
		{
			if (_options != null)
			{
				int num = ((Parent == null) ? _options.IndexOf(Text.text) : _options.IndexOf(Parent.Value as string));
				return (num != -1) ? num : 0;
			}
			return 0;
		}
	}

	public SettingItem Parent { get; set; }

	private void Start()
	{
		UIEventListener.Get(Left).onClick = delegate
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			MoveIndex(-1);
		};
		UIEventListener.Get(Right).onClick = delegate
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			MoveIndex(1);
		};
	}

	public void SetOptions(string[] options)
	{
		if (options != null && options.Length != 0)
		{
			_options = options;
			Text.text = _options[Index];
		}
	}

	public void OnLocalize(SettingType type = SettingType.Toggle)
	{
		if (Parent != null)
		{
			if (type == SettingType.Locale)
			{
				string locale = Parent.Value.ToString();
				Text.text = ((!(Parent.Key == "locale")) ? LocalizeSystem.GetVoiceLocaleName(locale) : LocalizeSystem.GetLocaleName(locale));
				return;
			}
			Text.text = LocalizeSystem.Get("#config_" + Parent.Key + "_" + Parent.Value);
		}
	}

	public void MoveIndex(int offset)
	{
		int num = (Index + offset + _options.Length) % _options.Length;
		string text = _options[num];
		if (Parent != null)
		{
			if (ValueChanged != null)
			{
				ValueChanged(text);
			}
		}
		else
		{
			Text.text = text;
		}
	}
}
