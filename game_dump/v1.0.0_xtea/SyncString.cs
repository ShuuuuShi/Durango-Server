public struct SyncString
{
	public delegate void UpdateDelegate(out string text, out float period);

	private UpdateDelegate _func;

	private string _text;

	public SyncString(string value)
	{
		_func = null;
		_text = value;
	}

	public SyncString(UpdateDelegate value)
	{
		_func = value;
		_text = null;
	}

	public bool HasText()
	{
		return _func != null || !string.IsNullOrEmpty(_text);
	}

	public string Get(out float period)
	{
		if (_func == null)
		{
			period = 0f;
			return _text;
		}
		_func(out var text, out period);
		return text;
	}

	public static implicit operator SyncString(string value)
	{
		return new SyncString(value);
	}
}
