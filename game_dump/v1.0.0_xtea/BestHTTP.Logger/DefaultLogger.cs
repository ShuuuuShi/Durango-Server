using System;
using UnityEngine;

namespace BestHTTP.Logger;

public class DefaultLogger : ILogger
{
	public Loglevels Level { get; set; }

	public string FormatVerbose { get; set; }

	public string FormatInfo { get; set; }

	public string FormatWarn { get; set; }

	public string FormatErr { get; set; }

	public string FormatEx { get; set; }

	public DefaultLogger()
	{
		FormatVerbose = "I [{0}]: {1}";
		FormatInfo = "I [{0}]: {1}";
		FormatWarn = "W [{0}]: {1}";
		FormatErr = "Err [{0}]: {1}";
		FormatEx = "Ex [{0}]: {1} - Message: {2}  StackTrace: {3}";
		Level = ((!Debug.isDebugBuild) ? Loglevels.Error : Loglevels.Warning);
	}

	public void Verbose(string division, string verb)
	{
		if (Level <= Loglevels.All)
		{
			try
			{
				Debug.Log((object)string.Format(FormatVerbose, division, verb));
			}
			catch
			{
			}
		}
	}

	public void Information(string division, string info)
	{
		if (Level <= Loglevels.Information)
		{
			try
			{
				Debug.Log((object)string.Format(FormatInfo, division, info));
			}
			catch
			{
			}
		}
	}

	public void Warning(string division, string warn)
	{
		if (Level <= Loglevels.Warning)
		{
			try
			{
				Debug.LogWarning((object)string.Format(FormatWarn, division, warn));
			}
			catch
			{
			}
		}
	}

	public void Error(string division, string err)
	{
		if (Level <= Loglevels.Error)
		{
			try
			{
				Debug.LogError((object)string.Format(FormatErr, division, err));
			}
			catch
			{
			}
		}
	}

	public void Exception(string division, string msg, Exception ex)
	{
		if (Level <= Loglevels.Exception)
		{
			try
			{
				Debug.LogError((object)string.Format(FormatEx, division, msg, (ex == null) ? "null" : ex.Message, (ex == null) ? "null" : ex.StackTrace));
			}
			catch
			{
			}
		}
	}
}
