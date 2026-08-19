using System.Collections.Generic;
using Yaml.Util;

namespace Yaml;

public class TimelineMessagesYaml : Singleton<TimelineMessagesYaml>
{
	public Dictionary<int, TimelineMessage> messages;
}
