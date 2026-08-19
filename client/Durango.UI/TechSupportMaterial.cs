using UnityEngine;

namespace Durango.UI;

public class TechSupportMaterial : UIWidget
{
	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UILabel _textCount;

	[SerializeField]
	private GameObject _seperator;

	public void SetEmpty()
	{
		_textName.gameObject.SetActive(value: false);
		_textCount.gameObject.SetActive(value: false);
		_seperator.SetActive(value: false);
	}

	public void SetMaterial(string name, int current, int max)
	{
		_textName.gameObject.SetActive(value: true);
		_textCount.gameObject.SetActive(value: true);
		_seperator.SetActive(value: true);
		_textName.text = name;
		_textCount.text = ((current >= max) ? $"<em>{current}</em> / {max}" : $"[c=ui_red]{current}[-] / {max}");
	}
}
