using Durango.Utils.Extensions;

namespace Durango.System.Config;

public class DropdownSetting : ValueSetting
{
	public string[] Options;

	public bool ButtonClickClose;

	public bool Custom;

	public bool Contains(string value)
	{
		if (Options != null)
		{
			return Options.ContainsIgnoreCase(value);
		}
		return false;
	}
}
