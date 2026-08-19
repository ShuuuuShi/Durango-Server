using Durango.System.Config;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class BannerWidget : SelectableWidget
{
	[SerializeField]
	private UILabel _textMain;

	[SerializeField]
	private UILabel _textSub;

	public ValueSetting ValueSetting { get; private set; }

	public void SetValueSetting(ValueSetting valueSetting)
	{
		_textMain.text = LocalizeSystem.Get("#config_" + valueSetting.Key);
		_textSub.text = LocalizeSystem.Get("#config_" + valueSetting.Default);
		ValueSetting = valueSetting;
	}
}
