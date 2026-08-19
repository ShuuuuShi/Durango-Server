using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using SharpRaven;
using SharpRaven.Data;
using SharpRaven.Logging;
using SharpRaven.Utilities;
using UnityEngine;

public class KRavenClient : KSingleton<KRavenClient>
{
	private DSN _currentDSN = new DSN("https://9185afede3904860a74da6d256791ca1:d05527cbcf80452cb38a56f87c49dbfe@app.getsentry.com/48217");

	public IScrubber LogScrubber { get; set; }

	public string Logger { get; set; }

	protected override bool CheckDontDestroyOnLoad()
	{
		return true;
	}

	public int CaptureException(Exception e)
	{
		return CaptureException(e, null, null);
	}

	public int CaptureException(Exception e, Dictionary<string, string> tags)
	{
		return CaptureException(e, tags, null);
	}

	public int CaptureException(Exception e, Dictionary<string, string> tags, object extra = null)
	{
		JsonPacket jsonPacket = new JsonPacket(_currentDSN.ProjectID, e);
		jsonPacket.Level = ErrorLevel.error;
		jsonPacket.Tags = tags;
		jsonPacket.Extra = extra;
		((MonoBehaviour)this).StartCoroutine(coSend(jsonPacket, _currentDSN));
		return 0;
	}

	public int CaptureUntiyLog(string log, string stack, LogType logType, Dictionary<string, string> tags = null, object extra = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		JsonPacket jsonPacket = new JsonPacket(_currentDSN.ProjectID, log, stack, logType);
		jsonPacket.Level = ErrorLevel.error;
		jsonPacket.Tags = tags;
		jsonPacket.Extra = extra;
		((MonoBehaviour)this).StartCoroutine(coSend(jsonPacket, _currentDSN));
		return 0;
	}

	public int CaptureMessage(string message)
	{
		return CaptureMessage(message, ErrorLevel.info, null, null);
	}

	public int CaptureMessage(string message, ErrorLevel level)
	{
		return CaptureMessage(message, level, null, null);
	}

	public int CaptureMessage(string message, ErrorLevel level, Dictionary<string, string> tags)
	{
		return CaptureMessage(message, level, tags, null);
	}

	public int CaptureMessage(string message, ErrorLevel level, Dictionary<string, string> tags, object extra)
	{
		JsonPacket jsonPacket = new JsonPacket(_currentDSN.ProjectID);
		jsonPacket.Message = message;
		jsonPacket.Level = level;
		jsonPacket.Tags = tags;
		jsonPacket.Extra = extra;
		((MonoBehaviour)this).StartCoroutine(coSend(jsonPacket, _currentDSN));
		return 0;
	}

	private byte[] GZipCompress(string payload)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		using MemoryStream memoryStream = new MemoryStream();
		GZipOutputStream val = new GZipOutputStream((Stream)memoryStream);
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(payload);
			val.Write(bytes, 0, bytes.Length);
			val.Finish();
			return memoryStream.ToArray();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private IEnumerator coSend(JsonPacket packet, DSN dsn)
	{
		packet.Logger = "root";
		Dictionary<string, string> dict = new Dictionary<string, string>
		{
			["Accept"] = "application/json",
			["Content-Type"] = "application/octet-stream",
			["Content-Encoding"] = "gzip",
			["X-Sentry-Auth"] = PacketBuilder.CreateAuthenticationHeader(dsn),
			["UserAgent"] = "RavenSharp/1.0"
		};
		string data = packet.Serialize();
		if (LogScrubber != null)
		{
			data = LogScrubber.Scrub(data);
		}
		WWW www = new WWW(dsn.SentryURI, GZipCompress(data), dict);
		yield return www;
		try
		{
			if (string.IsNullOrEmpty(www.error))
			{
			}
			www.Dispose();
		}
		catch (Exception)
		{
		}
	}
}
