using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class ErrorReporter
{
	private static Dictionary<string, string> _tags;

	private static HashSet<string> _logSet;

	private static Dictionary<string, string> Tags
	{
		get
		{
			if (_tags != null)
			{
				return _tags;
			}
			_tags = new Dictionary<string, string>();
			_tags["Version"] = CurrentBundleVersion.GetClientVersion();
			_tags["ProcessorType"] = SystemInfo.processorType;
			_tags["ProcessorCount"] = SystemInfo.processorCount.ToString();
			_tags["Device-Uid"] = SystemInfo.deviceUniqueIdentifier;
			_tags["Device-Model"] = SystemInfo.deviceModel;
			_tags["Device-Name"] = SystemInfo.deviceName;
			_tags["OS"] = SystemInfo.operatingSystem;
			_tags["MemorySize"] = SystemInfo.systemMemorySize.ToString();
			_tags["GPU-Memory"] = SystemInfo.graphicsMemorySize.ToString();
			_tags["GPU-Name"] = SystemInfo.graphicsDeviceName;
			_tags["GPU-Vendor"] = SystemInfo.graphicsDeviceVendor;
			_tags["GPU-VendorID"] = SystemInfo.graphicsDeviceVendorID.ToString();
			_tags["GPU-id"] = SystemInfo.graphicsDeviceID.ToString();
			_tags["GPU-Version"] = SystemInfo.graphicsDeviceVersion;
			_tags["GPU-ShaderLevel"] = SystemInfo.graphicsShaderLevel.ToString();
			_tags["GPU-MaxTextureSize"] = SystemInfo.maxTextureSize.ToString();
			_tags["GPU-2DArrayTextures"] = SystemInfo.supports2DArrayTextures.ToString();
			return _tags;
		}
	}

	private static HashSet<string> LogSet
	{
		get
		{
			if (_logSet == null)
			{
				_logSet = new HashSet<string>();
			}
			return _logSet;
		}
	}

	public static void HandleLog(string log, string stack, DateTime time, LogType type)
	{
		if (!Application.isEditor && !LogSet.Contains(stack))
		{
			LogSet.Add(stack);
			Tags["type"] = type.ToString();
			Tags["playTime"] = Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture);
			Tags["time"] = time.ToString("yyyy-MM-dd HH:mm:ss:fff");
			Tags["PlayerId"] = GameManager.PlayerId;
			RavenClient.CaptureUntiyLog(log, stack, type, Tags, stack);
		}
	}
}
