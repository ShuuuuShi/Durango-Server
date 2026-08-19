using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PlayerSearchInput : MonoBehaviour
{
	[SerializeField]
	private UIInput _nameInput;

	[SerializeField]
	private UIInput _freqInput;

	[SerializeField]
	private GameObject _nameClearButton;

	[SerializeField]
	private GameObject _freqClearButton;

	public event Action<string, string> Submitted;

	[UsedImplicitly]
	private void OnInitialize()
	{
		EventDelegate.Add(_nameInput.onSubmit, Input_Submitted);
		EventDelegate.Add(_freqInput.onSubmit, Input_Submitted);
		EventDelegate.Add(_nameInput.onChange, NameInput_Changed);
		EventDelegate.Add(_freqInput.onChange, FreqInput_Changed);
		UIEventListener.Get(_nameClearButton).onClick = NameClearButton_Clicked;
		UIEventListener.Get(_freqClearButton).onClick = FreqClearButton_Clicked;
		_nameInput.defaultText = T._("검색");
		_freqInput.defaultText = T._("# 주파수");
	}

	private void Input_Submitted()
	{
		string value = _nameInput.value;
		if (!string.IsNullOrEmpty(value))
		{
			string[] array = value.Split('#');
			if (array.Length > 1)
			{
				SetInput(array[0], array[1]);
			}
		}
		if (this.Submitted != null)
		{
			this.Submitted(_nameInput.value, _freqInput.value);
		}
	}

	private void NameInput_Changed()
	{
		bool flag = string.IsNullOrEmpty(_nameInput.value);
		_nameClearButton.SetActive(!flag);
	}

	private void FreqInput_Changed()
	{
		bool flag = string.IsNullOrEmpty(_freqInput.value);
		_freqClearButton.SetActive(!flag);
	}

	private void NameClearButton_Clicked(GameObject go)
	{
		_nameInput.value = string.Empty;
		Input_Submitted();
	}

	private void FreqClearButton_Clicked(GameObject go)
	{
		_freqInput.value = string.Empty;
		Input_Submitted();
	}

	public void SetInput(string key, string freq)
	{
		_nameInput.Set(key, notify: false);
		_freqInput.Set(freq, notify: false);
		_nameClearButton.SetActive(!string.IsNullOrEmpty(key));
		_freqClearButton.SetActive(!string.IsNullOrEmpty(freq));
	}

	public KeyValuePair<string, string> GetInput()
	{
		return new KeyValuePair<string, string>(_nameInput.value, _freqInput.value);
	}
}
