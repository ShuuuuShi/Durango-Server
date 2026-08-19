using UnityEngine;

namespace AndroidKeyboard;

public class TouchScreenKeyboard
{
	public static TouchScreenKeyboard instance;

	private static bool m_hideInput;

	public bool wasCanceled;

	public bool done;

	public bool m_active;

	private string m_text;

	private bool m_OnReturnKey;

	private int height;

	private static int cursorPositionStart;

	private static int cursorPositionEnd;

	public static bool hideInput
	{
		get
		{
			return m_hideInput;
		}
		set
		{
			if (m_hideInput != value)
			{
				m_hideInput = value;
				AndroidKeyboardManager.SetHideInput(value);
			}
		}
	}

	public bool active
	{
		get
		{
			return m_active;
		}
		set
		{
			if (m_active != value)
			{
				m_active = value;
				if (!value)
				{
					instance = null;
					AndroidKeyboardManager.Close();
					KeyboardMessageReceiver.instance.actionUpdate = null;
				}
			}
		}
	}

	public string text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (m_text != value)
			{
				m_text = value;
			}
		}
	}

	public bool OnReturnKey
	{
		get
		{
			return m_OnReturnKey;
		}
		set
		{
			if (m_OnReturnKey != value)
			{
				m_OnReturnKey = value;
			}
		}
	}

	public int Height
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
		}
	}

	public static int CursorPositionStart
	{
		get
		{
			return cursorPositionStart;
		}
		set
		{
			cursorPositionStart = value;
		}
	}

	public static int CursorPositionEnd
	{
		get
		{
			return cursorPositionEnd;
		}
		set
		{
			cursorPositionEnd = value;
		}
	}

	public static TouchScreenKeyboard Open(string text)
	{
		return Open(text, (TouchScreenKeyboardType)0, autocorrection: false, multiline: false, secure: false, alert: false, string.Empty);
	}

	public static TouchScreenKeyboard Open(string text, TouchScreenKeyboardType keyboardType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Open(text, keyboardType, autocorrection: false, multiline: false, secure: false, alert: false, string.Empty);
	}

	public static TouchScreenKeyboard Open(string text, TouchScreenKeyboardType keyboardType, bool autocorrection)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Open(text, keyboardType, autocorrection, multiline: false, secure: false, alert: false, string.Empty);
	}

	public static TouchScreenKeyboard Open(string text, TouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Open(text, keyboardType, autocorrection, multiline, secure: false, alert: false, string.Empty);
	}

	public static TouchScreenKeyboard Open(string text, TouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline, bool secure)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Open(text, keyboardType, autocorrection, multiline, secure, alert: false, string.Empty);
	}

	public static TouchScreenKeyboard Open(string text, TouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline, bool secure, bool alert)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Open(text, keyboardType, autocorrection, multiline, secure, alert, string.Empty);
	}

	public static TouchScreenKeyboard Open(string text, TouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline, bool secure, bool alert, string textPlaceholder)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		KeyboardMessageReceiver.instance.create();
		TouchScreenKeyboard touchScreenKeyboard = new TouchScreenKeyboard();
		touchScreenKeyboard.m_text = text;
		touchScreenKeyboard.m_active = true;
		AndroidKeyboardManager.Open(text, SumOptionFlags(keyboardType, autocorrection, multiline, secure), alert, textPlaceholder);
		instance = touchScreenKeyboard;
		instance.wasCanceled = false;
		return touchScreenKeyboard;
	}

	private static int SumOptionFlags(TouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline, bool secure)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		int num = GetTextTypeFlags(keyboardType) | GetOptionFlags(autocorrection, multiline, secure);
		num |= AdvancedOptions.GetFlags();
		return num | AdditionalOptions.GetFlags();
	}

	private static int GetOptionFlags(bool autocorrection, bool multiline, bool secure)
	{
		int num = 0;
		if (autocorrection)
		{
			num |= 0x8000;
		}
		if (multiline)
		{
			num |= 0x20000;
		}
		if (secure)
		{
			num |= 0x80;
		}
		return num;
	}

	public static int GetTextTypeFlags(TouchScreenKeyboardType touchScreenKeyboardType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected I4, but got Unknown
		int num = 0;
		switch (touchScreenKeyboardType - 1)
		{
		case 0:
		case 5:
			return 1;
		case 2:
			return 17;
		case 6:
			return 33;
		case 1:
		case 3:
			return 2;
		case 4:
			return 3;
		default:
			return 1;
		}
	}
}
