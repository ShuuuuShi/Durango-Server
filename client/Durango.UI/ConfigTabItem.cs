using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ConfigTabItem : SelectableWidget
{
	[SerializeField]
	private UILabel _nameLabel;

	public string Category { get; private set; }

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}

	public void Set(string category)
	{
		Category = category;
		_nameLabel.text = LocalizeSystem.Get("#config_" + category);
	}
}
