using Durango.Utils.Extensions;

namespace Durango.System.Config;

public class ToggleSetting : ValueSetting
{
	public string[] Options;

	public bool Contains(string value)
	{
		return Options != null && Options.ContainsIgnoreCase(value);
	}
}
