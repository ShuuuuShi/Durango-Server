using Durango.Logic;
using UnityEngine;

namespace Durango.UI;

public class MenuWidget_PC : MenuWidget
{
	[SerializeField]
	private UILabel _shortcutLabel;

	[SerializeField]
	private UISprite _shortcutBg;

	[SerializeField]
	private int _shortcutBgVPadding;

	[SerializeField]
	private int _minWidth;

	private InputCommand _command;

	public void SetShortcutLabel(MenuType menuType)
	{
		if (!(_shortcutLabel == null))
		{
			_command = GameSystem<InputSystem>.Instance().Keyboard.GetMenuCommand(menuType);
			string keyCaption = GameSystem<InputSystem>.Instance().Keyboard.GetKeyCaption(_command);
			if (string.IsNullOrEmpty(keyCaption))
			{
				_shortcutLabel.gameObject.SetActive(value: false);
				_shortcutLabel.text = string.Empty;
			}
			else
			{
				_shortcutLabel.gameObject.SetActive(value: true);
				_shortcutLabel.text = keyCaption;
			}
			int num = _shortcutLabel.width + _shortcutBgVPadding;
			num = ((num <= _minWidth) ? _minWidth : num);
			_shortcutBg.width = num;
		}
	}
}
