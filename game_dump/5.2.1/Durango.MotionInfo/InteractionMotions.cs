using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Durango.MotionInfo;

public class InteractionMotions
{
	[JsonProperty(PropertyName = "motions")]
	private Dictionary<string, Dictionary<string, List<string>>> _motions;

	[JsonProperty(PropertyName = "default_motions")]
	private List<string> _defaultMotions;

	public string Get(string blueprintId, string attachType)
	{
		if (_motions == null || string.IsNullOrEmpty(blueprintId))
		{
			return GetDefaultMotion();
		}
		if (_motions.TryGetValueWithSubStringKey(blueprintId, out var value))
		{
			if (attachType == null)
			{
				attachType = "default";
			}
			else if (!value.ContainsKey(attachType))
			{
				attachType = "common";
			}
			List<string> list = value.Get(attachType);
			if (list != null && list.Count > 0)
			{
				return list[Random.Range(0, list.Count)];
			}
		}
		return GetDefaultMotion();
	}

	private string GetDefaultMotion()
	{
		if (_defaultMotions == null || _defaultMotions.Count == 0)
		{
			return null;
		}
		return _defaultMotions[Random.Range(0, _defaultMotions.Count)];
	}
}
