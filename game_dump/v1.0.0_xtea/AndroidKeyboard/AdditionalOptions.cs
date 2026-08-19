namespace AndroidKeyboard;

public class AdditionalOptions
{
	private static bool m_fullScreen;

	private static InputAdjustType m_SoftInputMode;

	private static bool m_NoSuggestions;

	private static bool m_selectAllTextOnFocus;

	private static bool m_KeepKeyboardOn;

	public static bool fullScreen
	{
		get
		{
			return m_fullScreen;
		}
		set
		{
			if (m_fullScreen != value)
			{
				m_fullScreen = value;
				AndroidKeyboardManager.SetFullScreen(value);
			}
		}
	}

	public static InputAdjustType softInputMode
	{
		get
		{
			return m_SoftInputMode;
		}
		set
		{
			if (m_SoftInputMode != value)
			{
				m_SoftInputMode = value;
				AndroidKeyboardManager.SetSoftInputMode(value);
			}
		}
	}

	public static bool noSuggestions
	{
		get
		{
			return m_NoSuggestions;
		}
		set
		{
			if (m_NoSuggestions != value)
			{
				m_NoSuggestions = value;
			}
		}
	}

	public static bool selectAllTextOnFocus
	{
		get
		{
			return m_selectAllTextOnFocus;
		}
		set
		{
			if (m_selectAllTextOnFocus != value)
			{
				m_selectAllTextOnFocus = value;
			}
		}
	}

	public static bool keepKeyboardOn
	{
		get
		{
			return m_KeepKeyboardOn;
		}
		set
		{
			AndroidKeyboardManager.KeepKeyboardOn(value);
			m_KeepKeyboardOn = value;
		}
	}

	public static int GetFlags()
	{
		int result = 0;
		if (noSuggestions)
		{
			result = 524288;
		}
		return result;
	}
}
