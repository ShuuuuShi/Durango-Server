using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class ErrorReporter
{
	private static bool _isFirstTime = true;

	private static Dictionary<string, string> _dictBuffer;

	private static Dictionary<string, string> Dict => (_dictBuffer != null) ? _dictBuffer : (_dictBuffer = new Dictionary<string, string>());

	public static void HandleLog(string log, string stack, DateTime time, LogType type)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		Dict["type"] = ((Enum)type).ToString();
		Dict["playTime"] = Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture);
		Dict["time"] = time.ToString("yyyy-MM-dd HH:mm:ss:fff");
		if (_isFirstTime)
		{
			Dict["Version"] = CurrentBundleVersion.GetClientVersion();
			Dict["ProcessorType"] = SystemInfo.processorType;
			Dict["ProcessorCount"] = SystemInfo.processorCount.ToString();
			Dict["Device-Uid"] = SystemInfo.deviceUniqueIdentifier;
			Dict["Device-Model"] = SystemInfo.deviceModel;
			Dict["Device-Name"] = SystemInfo.deviceName;
			Dict["OS"] = SystemInfo.operatingSystem;
			Dict["MemorySize"] = SystemInfo.systemMemorySize.ToString();
			Dict["GPU-Memory"] = SystemInfo.graphicsMemorySize.ToString();
			Dict["GPU-Name"] = SystemInfo.graphicsDeviceName;
			Dict["GPU-Vendor"] = SystemInfo.graphicsDeviceVendor;
			Dict["GPU-VendorID"] = SystemInfo.graphicsDeviceVendorID.ToString();
			Dict["GPU-id"] = SystemInfo.graphicsDeviceID.ToString();
			Dict["GPU-Version"] = SystemInfo.graphicsDeviceVersion;
			Dict["GPU-ShaderLevel"] = SystemInfo.graphicsShaderLevel.ToString();
			_isFirstTime = false;
		}
		KSingleton<KRavenClient>.Instance().CaptureUntiyLog(log, stack, type, Dict, stack);
	}

	public static void Initialize()
	{
		CheckCrashReports();
	}

	private static void CheckCrashReports()
	{
		CrashReport[] reports = CrashReport.reports;
		if (reports == null)
		{
			return;
		}
		for (int i = 0; i < reports.Length; i++)
		{
			if (reports[i] != null)
			{
				HandleLog("iOS Crash Report", reports[i].text, reports[i].time, (LogType)4);
			}
		}
		CrashReport.RemoveAll();
	}
}
