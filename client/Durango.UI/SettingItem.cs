using Durango.System.Config;
using UnityEngine;

namespace Durango.UI;

public class SettingItem
{
	public Setting Setting;

	public UIWidget Widget;

	public UILabel Label;

	public GameObject BgLine;

	public object Value;

	public object Contents;

	public object SubContent;

	public GameObject GameObj => Widget.gameObject;

	public SettingType Type => Setting.Type;

	public string Key => Setting.Key;

	public void ShowBgLine(bool show)
	{
		if (BgLine != null)
		{
			BgLine.SetActive(show);
		}
	}
}
