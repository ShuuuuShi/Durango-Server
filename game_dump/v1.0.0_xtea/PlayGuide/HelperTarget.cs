using System.Collections.Generic;

namespace PlayGuide;

public class HelperTarget
{
	public string type;

	public string id;

	public float duration;

	public string text;

	public string arrow;

	public Dictionary<string, ClickTargetData> click_targets;
}
