public struct Gettext
{
	public static Gettext Empty = string.Empty;

	private readonly string _text;

	public Gettext(string text)
	{
		_text = text;
	}

	public override string ToString()
	{
		return _text;
	}

	public static bool IsEmpty(Gettext gettext)
	{
		return string.IsNullOrEmpty(gettext._text);
	}

	public static implicit operator string(Gettext g)
	{
		return g._text;
	}

	public static implicit operator Gettext(string s)
	{
		return new Gettext(s);
	}
}
