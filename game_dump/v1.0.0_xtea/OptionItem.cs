using OptionData;
using UnityEngine;

public class OptionItem
{
	public global::OptionData.OptionData Option;

	public GameObject GameObj;

	public UIWidget Widget;

	public UISpriteLabel Label;

	public UISprite Background;

	public object Value;

	public object Contents;

	public bool IsValid;

	public OptionType Type => Option.Type;

	public string Key => Option.Key;

	public string StringValue => Value as string;

	public float FloatValue => (float)Value;

	public void Dispose()
	{
		Object.Destroy((Object)(object)GameObj);
	}
}
