using OptionData;
using UnityEngine;

public class ToggleWidget : MonoBehaviour
{
	[SerializeField]
	public UILabel Text;

	[SerializeField]
	public GameObject Left;

	[SerializeField]
	public GameObject Right;

	public OptionItem Parent { get; set; }

	public string[] Optons { get; set; }

	public void OnLocalize(OptionType type = OptionType.Toggle)
	{
		if (type == OptionType.Locale)
		{
			string key = Parent.Value.ToString();
			if (!LocalizeSystem.LocaleNames.ContainsKey(key))
			{
				key = "en_US";
			}
			Text.text = LocalizeSystem.LocaleNames[key];
		}
		else
		{
			Text.text = LocalizeSystem.Get("#option_" + Parent.Key + "_" + Parent.Value);
		}
	}
}
