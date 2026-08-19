using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BestHTTP;
using ICSharpCode.SharpZipLib.GZip;
using SharpRaven;
using SharpRaven.Data;
using SharpRaven.Utilities;
using UnityEngine;

public static class RavenClient
{
	private static readonly DSN CurrentDsn = new DSN("https://9185afede3904860a74da6d256791ca1:d05527cbcf80452cb38a56f87c49dbfe@app.getsentry.com/48217");

	public static void CaptureUntiyLog(string log, string stack, LogType logType, Dictionary<string, string> tags = null, object extra = null)
	{
		Send(new JsonPacket(CurrentDsn.ProjectID, log, stack, logType)
		{
			Level = ErrorLevel.error,
			Tags = tags,
			Extra = extra
		}, CurrentDsn);
	}

	private static byte[] GZipCompress(string payload)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using GZipOutputStream gZipOutputStream = new GZipOutputStream(memoryStream);
		byte[] bytes = Encoding.UTF8.GetBytes(payload);
		gZipOutputStream.Write(bytes, 0, bytes.Length);
		gZipOutputStream.Finish();
		return memoryStream.ToArray();
	}

	private static void Send(JsonPacket packet, DSN dsn)
	{
		packet.Logger = "root";
		Dictionary<string, string> obj = new Dictionary<string, string>
		{
			["Accept"] = "application/json",
			["Content-Type"] = "application/octet-stream",
			["Content-Encoding"] = "gzip",
			["X-Sentry-Auth"] = PacketBuilder.CreateAuthenticationHeader(dsn),
			["UserAgent"] = "RavenSharp/1.0"
		};
		string payload = packet.Serialize();
		HTTPRequest hTTPRequest = new HTTPRequest(new Uri(dsn.SentryURI), HTTPMethods.Post)
		{
			RawData = GZipCompress(payload)
		};
		foreach (KeyValuePair<string, string> item in obj)
		{
			hTTPRequest.AddHeader(item.Key, item.Value);
		}
		hTTPRequest.Send();
	}
}
