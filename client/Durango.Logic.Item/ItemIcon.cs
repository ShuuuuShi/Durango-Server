using Messages;
using Yaml;

namespace Durango.Logic.Item;

public struct ItemIcon
{
	public string Main;

	public string Sub;

	public ItemColor Colors;

	public ItemIcon(Messages.Item item)
	{
		if (string.IsNullOrEmpty(item.Icon))
		{
			Prototype itemPrototype = PrototypeYaml.GetItemPrototype(item.Prototype, item.Level);
			Main = ((itemPrototype != null) ? itemPrototype.Icon : "icon_question");
		}
		else
		{
			Main = item.Icon;
		}
		Sub = item.SubIcon;
		Colors = new ItemColor(item.ColorR, item.ColorG, item.ColorB);
	}

	public ItemIcon(PrototypePreset preset)
	{
		Main = preset.Icon;
		Sub = null;
		Colors = new ItemColor(preset.ColorR, preset.ColorG, preset.ColorB);
	}

	public ItemIcon(string icon)
	{
		Main = icon;
		Sub = null;
		Colors = default(ItemColor);
	}

	public static implicit operator ItemIcon(string icon)
	{
		return new ItemIcon(icon);
	}
}
