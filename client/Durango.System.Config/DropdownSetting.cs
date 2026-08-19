using Durango.Utils.Extensions;

namespace Durango.System.Config;

public class DropdownSetting : ValueSetting
{
	public string[] Options;

	public bool ButtonClickClose;

	public bool Custom;

	public bool Contains(string value)
	{
		return Options != null && Options.ContainsIgnoreCase(value);
	}
}
