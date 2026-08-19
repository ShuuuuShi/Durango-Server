namespace BestHTTP.Extensions;

public sealed class KeyValuePair
{
	public string Key { get; set; }

	public string Value { get; set; }

	public KeyValuePair(string key)
	{
		Key = key;
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(Value))
		{
			return Key + '=' + Value;
		}
		return Key;
	}
}
