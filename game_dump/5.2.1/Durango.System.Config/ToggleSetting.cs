using Durango.Utils.Extensions;

namespace Durango.System.Config;

public class ToggleSetting : ValueSetting
{
	public string[] Options;

	public bool Contains(string value)
	{
		if (Options != null)
		{
			return Options.ContainsIgnoreCase(value);
		}
		return false;
	}
}
