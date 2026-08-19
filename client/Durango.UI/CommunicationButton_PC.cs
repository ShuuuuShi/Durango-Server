using UnityEngine;

namespace Durango.UI;

public class CommunicationButton_PC : CommunicationButtonBase
{
	[SerializeField]
	private Color _toggleOnColor = PresetColor.UIYellow;

	private bool _toggleOn;

	private float _lastToggleTime;

	public override bool ToggleOn
	{
		get
		{
			return _toggleOn;
		}
		set
		{
			if (_toggleOn != value)
			{
				_toggleOn = value;
				_sprite.color = ((!_toggleOn) ? Color.white : _toggleOnColor);
				_lastToggleTime = Time.time;
			}
		}
	}

	private void OnClick()
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		ToggleOn = !ToggleOn;
		if (_clicked != null)
		{
			_clicked();
		}
	}
}
