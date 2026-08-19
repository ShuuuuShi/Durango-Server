using System;
using Newtonsoft.Json;
using UnityEngine;

namespace SharpRaven.Data;

public class SentryException
{
	[JsonProperty(PropertyName = "type")]
	public string Type;

	[JsonProperty(PropertyName = "value")]
	public string Value;

	[JsonProperty(PropertyName = "module")]
	public string Module;

	public SentryException(Exception e)
	{
		Module = e.Source;
		Type = e.Message;
		Value = e.Message;
	}

	public SentryException(string log, string stack, LogType logType)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Module = log;
		Type = ((Enum)logType).ToString();
		Value = stack;
	}
}
