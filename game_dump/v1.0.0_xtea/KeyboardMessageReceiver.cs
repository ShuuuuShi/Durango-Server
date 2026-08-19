using System;
using AndroidKeyboard;
using UnityEngine;

public class KeyboardMessageReceiver : MonoBehaviour
{
	private static KeyboardMessageReceiver m_instance;

	public Action<bool> actionUpdate;

	public Action<bool> actionUpdate2;

	public Action<int, int> actionCursorChanged;

	public Action onKeyboardClosed;

	private char[] separator = new char[1] { '|' };

	public static KeyboardMessageReceiver instance
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if ((Object)(object)m_instance == (Object)null)
			{
				GameObject val = new GameObject();
				((Object)val).name = "KeyboardMessageReceiver";
				m_instance = val.AddComponent<KeyboardMessageReceiver>();
			}
			return m_instance;
		}
	}

	public void create()
	{
	}

	private void OnCompositionChanged(string value)
	{
		Input.compositionString = value;
		if (actionUpdate != null)
		{
			actionUpdate(obj: true);
		}
	}

	private void OnTextChanged(string value)
	{
		if (TouchScreenKeyboard.instance != null)
		{
			if (value.Contains(". "))
			{
				value = value.Replace(". ", " ");
			}
			if (TouchScreenKeyboard.instance != null)
			{
				TouchScreenKeyboard.instance.text = value;
			}
			Input.compositionString = string.Empty;
			Input.inputString = value;
			if (actionUpdate != null)
			{
				actionUpdate(obj: true);
			}
		}
	}

	public void onTextCompleted(string byBackButton)
	{
		if (TouchScreenKeyboard.instance != null)
		{
			if (byBackButton == "true")
			{
				TouchScreenKeyboard.instance.wasCanceled = true;
			}
			Input.compositionString = string.Empty;
			if (actionUpdate != null)
			{
				actionUpdate(obj: true);
			}
			if (!AdditionalOptions.keepKeyboardOn || byBackButton == "true")
			{
				TouchScreenKeyboard.instance.active = false;
			}
			else
			{
				TouchScreenKeyboard.instance.OnReturnKey = true;
			}
		}
	}

	public void onFullTextComplete(string value)
	{
		if (TouchScreenKeyboard.instance != null)
		{
			TouchScreenKeyboard.instance.text = value;
			Input.compositionString = string.Empty;
			if (actionUpdate2 != null)
			{
				actionUpdate2(obj: true);
			}
			if (!AdditionalOptions.keepKeyboardOn)
			{
				TouchScreenKeyboard.instance.active = false;
			}
			else
			{
				TouchScreenKeyboard.instance.OnReturnKey = true;
			}
			if (actionUpdate != null)
			{
				actionUpdate(obj: true);
			}
		}
	}

	public void OnKeyboardClosed(string value)
	{
		if (onKeyboardClosed != null)
		{
			onKeyboardClosed();
		}
	}

	public void UpdateKeyboardHeight(string value)
	{
		if (TouchScreenKeyboard.instance != null)
		{
			int result = 0;
			int.TryParse(value, out result);
			if (TouchScreenKeyboard.instance != null)
			{
				TouchScreenKeyboard.instance.Height = result;
			}
		}
	}

	public void onTouch(string value)
	{
		string[] array = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			int pointerIndex = int.Parse(array3[0]);
			int fingerId = int.Parse(array3[1]);
			int tapCount = int.Parse(array3[2]);
			int phase = int.Parse(array3[3]);
			float x = float.Parse(array3[4]);
			float y = float.Parse(array3[5]);
			AndroidTouch.instance.OnTouch(pointerIndex, fingerId, tapCount, phase, x, y);
		}
	}

	public void OnCursorChanged(string value)
	{
		string[] array = value.Split(',');
		int result = 0;
		int.TryParse(array[0], out result);
		int result2 = 0;
		int.TryParse(array[1], out result2);
		TouchScreenKeyboard.CursorPositionStart = result;
		TouchScreenKeyboard.CursorPositionEnd = result2;
		if (actionCursorChanged != null)
		{
			actionCursorChanged(result, result2);
		}
	}

	private void LateUpdate()
	{
		AndroidTouch.instance.LateUpdate();
	}
}
