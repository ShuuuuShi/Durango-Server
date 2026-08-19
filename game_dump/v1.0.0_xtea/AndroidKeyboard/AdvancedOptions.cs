namespace AndroidKeyboard;

public static class AdvancedOptions
{
	private static TYPE_TEXT_FLAG m_textTypeFlag;

	private static TYPE_NUMBER_FLAG m_numberTypeFlag;

	private static TYPE_TEXT_VARIATION m_typeTextVariation;

	private static TYPE_DATETIME_VARIATION m_typeDatetimeVariation;

	public static TYPE_TEXT_FLAG textTypeFlag
	{
		get
		{
			return m_textTypeFlag;
		}
		set
		{
			if (m_textTypeFlag != value)
			{
				m_textTypeFlag = value;
			}
		}
	}

	public static TYPE_NUMBER_FLAG numberTypeFlag
	{
		get
		{
			return m_numberTypeFlag;
		}
		set
		{
			if (m_numberTypeFlag != value)
			{
				m_numberTypeFlag = value;
			}
		}
	}

	public static TYPE_TEXT_VARIATION typeTextVariation
	{
		get
		{
			return m_typeTextVariation;
		}
		set
		{
			if (m_typeTextVariation != value)
			{
				m_typeTextVariation = value;
			}
		}
	}

	public static TYPE_DATETIME_VARIATION typeDatetimeVariation
	{
		get
		{
			return m_typeDatetimeVariation;
		}
		set
		{
			if (m_typeDatetimeVariation != value)
			{
				m_typeDatetimeVariation = value;
			}
		}
	}

	public static int GetFlags()
	{
		return (int)textTypeFlag | (int)numberTypeFlag | (int)typeTextVariation | (int)typeDatetimeVariation;
	}
}
