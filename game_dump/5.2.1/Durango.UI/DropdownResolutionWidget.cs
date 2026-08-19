using System.Linq;
using Durango.System.Config;

namespace Durango.UI;

public class DropdownResolutionWidget : DropdownWidget
{
	public static readonly string ResolutionKey = "resolution_pc";

	private bool _init;

	protected override void OnEnable()
	{
		base.OnEnable();
		SetValue(ScreenInfo.GetCurrentScreenSize().ToString());
	}

	public override void Init(ValueSetting setting, string[] options, bool isCloseOnClick)
	{
		if (_init)
		{
			UIUtility.UpdateAnchors(base.transform);
			Open(isOpen: false);
		}
		else
		{
			if (ResolutionKey != setting.Key)
			{
				return;
			}
			base.Setting = setting;
			IsCloseOnClick = isCloseOnClick;
			Options = ScreenInfo.GetAvailableScreenSizes().Select(delegate(ScreenSize x)
			{
				ScreenSize screenSize = x;
				return screenSize.ToString();
			}).ToArray();
			if (Options != null)
			{
				base.Pool.Set(Options.Length);
				for (int i = 0; i < Options.Length; i++)
				{
					DropdownButton dropdownButton = base.Pool.Get<DropdownButton>(i);
					if (!(dropdownButton == null))
					{
						dropdownButton.Set(Options[i], i);
						dropdownButton.ButtonClicked = base.OnClickButton;
					}
				}
			}
			SetValue(ScreenInfo.GetCurrentScreenSize().ToString());
			UIUtility.UpdateAnchors(base.transform);
			Open(isOpen: false);
			_init = true;
		}
	}

	public override void SetValue(string value)
	{
		if (base.Setting != null)
		{
			base.Setting.Value = value;
			SetTitle(value);
		}
	}
}
