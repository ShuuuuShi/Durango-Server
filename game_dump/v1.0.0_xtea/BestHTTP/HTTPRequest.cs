using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BestHTTP.Authentication;
using BestHTTP.Extensions;
using BestHTTP.Forms;
using BestHTTP.Logger;
using UnityEngine;

namespace BestHTTP;

public sealed class HTTPRequest : IDisposable, IEnumerator, IEnumerator<HTTPRequest>
{
	internal static readonly byte[] EOL = new byte[2] { 13, 10 };

	internal static readonly string[] MethodNames = new string[6]
	{
		HTTPMethods.Get.ToString().ToUpper(),
		HTTPMethods.Head.ToString().ToUpper(),
		HTTPMethods.Post.ToString().ToUpper(),
		HTTPMethods.Put.ToString().ToUpper(),
		HTTPMethods.Delete.ToString().ToUpper(),
		HTTPMethods.Patch.ToString().ToUpper()
	};

	public static int UploadChunkSize = 1024;

	public OnUploadProgressDelegate OnUploadProgress;

	public OnDownloadProgressDelegate OnProgress;

	public OnRequestFinishedDelegate OnUpgraded;

	private OnBeforeRedirectionDelegate onBeforeRedirection;

	private bool isKeepAlive;

	private bool disableCache;

	private int streamFragmentSize;

	private bool useStreaming;

	private HTTPFormBase FieldCollector;

	private HTTPFormBase FormImpl;

	HTTPRequest IEnumerator<HTTPRequest>.Current => this;

	public Uri Uri { get; private set; }

	public HTTPMethods MethodType { get; set; }

	public byte[] RawData { get; set; }

	public Stream UploadStream { get; set; }

	public bool DisposeUploadStream { get; set; }

	public bool UseUploadStreamLength { get; set; }

	public bool IsKeepAlive
	{
		get
		{
			return isKeepAlive;
		}
		set
		{
			if (State == HTTPRequestStates.Processing)
			{
				throw new NotSupportedException("Changing the IsKeepAlive property while processing the request is not supported.");
			}
			isKeepAlive = value;
		}
	}

	public bool DisableCache
	{
		get
		{
			return disableCache;
		}
		set
		{
			if (State == HTTPRequestStates.Processing)
			{
				throw new NotSupportedException("Changing the DisableCache property while processing the request is not supported.");
			}
			disableCache = value;
		}
	}

	public bool UseStreaming
	{
		get
		{
			return useStreaming;
		}
		set
		{
			if (State == HTTPRequestStates.Processing)
			{
				throw new NotSupportedException("Changing the UseStreaming property while processing the request is not supported.");
			}
			useStreaming = value;
		}
	}

	public int StreamFragmentSize
	{
		get
		{
			return streamFragmentSize;
		}
		set
		{
			if (State == HTTPRequestStates.Processing)
			{
				throw new NotSupportedException("Changing the StreamFragmentSize property while processing the request is not supported.");
			}
			if (value < 1)
			{
				throw new ArgumentException("StreamFragmentSize must be at least 1.");
			}
			streamFragmentSize = value;
		}
	}

	public OnRequestFinishedDelegate Callback { get; set; }

	public bool DisableRetry { get; set; }

	public bool IsRedirected { get; internal set; }

	public Uri RedirectUri { get; internal set; }

	public Uri CurrentUri => (!IsRedirected) ? Uri : RedirectUri;

	public HTTPResponse Response { get; internal set; }

	public HTTPResponse ProxyResponse { get; internal set; }

	public Exception Exception { get; internal set; }

	public object Tag { get; set; }

	public Credentials Credentials { get; set; }

	public bool HasProxy => Proxy != null;

	public HTTPProxy Proxy { get; set; }

	public int MaxRedirects { get; set; }

	public HTTPFormUsage FormUsage { get; set; }

	public HTTPRequestStates State { get; internal set; }

	public int RedirectCount { get; internal set; }

	public TimeSpan ConnectTimeout { get; set; }

	public TimeSpan Timeout { get; set; }

	public bool EnableTimoutForStreaming { get; set; }

	public int Priority { get; set; }

	public SupportedProtocols ProtocolHandler { get; set; }

	internal int Downloaded { get; set; }

	internal int DownloadLength { get; set; }

	internal bool DownloadProgressChanged { get; set; }

	internal long UploadStreamLength
	{
		get
		{
			if (UploadStream == null || !UseUploadStreamLength)
			{
				return -1L;
			}
			try
			{
				return UploadStream.Length;
			}
			catch
			{
				return -1L;
			}
		}
	}

	internal long Uploaded { get; set; }

	internal long UploadLength { get; set; }

	internal bool UploadProgressChanged { get; set; }

	private Dictionary<string, List<string>> Headers { get; set; }

	public object Current => null;

	public event Func<HTTPRequest, X509Certificate, X509Chain, bool> CustomCertificationValidator;

	public event OnBeforeRedirectionDelegate OnBeforeRedirection
	{
		add
		{
			onBeforeRedirection = (OnBeforeRedirectionDelegate)Delegate.Combine(onBeforeRedirection, value);
		}
		remove
		{
			onBeforeRedirection = (OnBeforeRedirectionDelegate)Delegate.Remove(onBeforeRedirection, value);
		}
	}

	public HTTPRequest(Uri uri)
		: this(uri, HTTPMethods.Get, HTTPManager.KeepAliveDefaultValue, HTTPManager.IsCachingDisabled, null)
	{
	}

	public HTTPRequest(Uri uri, OnRequestFinishedDelegate callback)
		: this(uri, HTTPMethods.Get, HTTPManager.KeepAliveDefaultValue, HTTPManager.IsCachingDisabled, callback)
	{
	}

	public HTTPRequest(Uri uri, bool isKeepAlive, OnRequestFinishedDelegate callback)
		: this(uri, HTTPMethods.Get, isKeepAlive, HTTPManager.IsCachingDisabled, callback)
	{
	}

	public HTTPRequest(Uri uri, bool isKeepAlive, bool disableCache, OnRequestFinishedDelegate callback)
		: this(uri, HTTPMethods.Get, isKeepAlive, disableCache, callback)
	{
	}

	public HTTPRequest(Uri uri, HTTPMethods methodType)
		: this(uri, methodType, HTTPManager.KeepAliveDefaultValue, HTTPManager.IsCachingDisabled || methodType != HTTPMethods.Get, null)
	{
	}

	public HTTPRequest(Uri uri, HTTPMethods methodType, OnRequestFinishedDelegate callback)
		: this(uri, methodType, HTTPManager.KeepAliveDefaultValue, HTTPManager.IsCachingDisabled || methodType != HTTPMethods.Get, callback)
	{
	}

	public HTTPRequest(Uri uri, HTTPMethods methodType, bool isKeepAlive, OnRequestFinishedDelegate callback)
		: this(uri, methodType, isKeepAlive, HTTPManager.IsCachingDisabled || methodType != HTTPMethods.Get, callback)
	{
	}

	public HTTPRequest(Uri uri, HTTPMethods methodType, bool isKeepAlive, bool disableCache, OnRequestFinishedDelegate callback)
	{
		Uri = uri;
		MethodType = methodType;
		IsKeepAlive = isKeepAlive;
		DisableCache = disableCache;
		Callback = callback;
		StreamFragmentSize = 4096;
		DisableRetry = methodType == HTTPMethods.Post;
		MaxRedirects = int.MaxValue;
		RedirectCount = 0;
		Downloaded = (DownloadLength = 0);
		DownloadProgressChanged = false;
		State = HTTPRequestStates.Initial;
		ConnectTimeout = HTTPManager.ConnectTimeout;
		Timeout = HTTPManager.RequestTimeout;
		EnableTimoutForStreaming = false;
		Proxy = HTTPManager.Proxy;
		UseUploadStreamLength = true;
		DisposeUploadStream = true;
	}

	public void AddField(string fieldName, string value)
	{
		AddField(fieldName, value, Encoding.UTF8);
	}

	public void AddField(string fieldName, string value, Encoding e)
	{
		if (FieldCollector == null)
		{
			FieldCollector = new HTTPFormBase();
		}
		FieldCollector.AddField(fieldName, value, e);
	}

	public void AddBinaryData(string fieldName, byte[] content)
	{
		AddBinaryData(fieldName, content, null, null);
	}

	public void AddBinaryData(string fieldName, byte[] content, string fileName)
	{
		AddBinaryData(fieldName, content, fileName, null);
	}

	public void AddBinaryData(string fieldName, byte[] content, string fileName, string mimeType)
	{
		if (FieldCollector == null)
		{
			FieldCollector = new HTTPFormBase();
		}
		FieldCollector.AddBinaryData(fieldName, content, fileName, mimeType);
	}

	public void SetFields(WWWForm wwwForm)
	{
	}

	public void SetForm(HTTPFormBase form)
	{
		FormImpl = form;
	}

	public void ClearForm()
	{
		FormImpl = null;
		FieldCollector = null;
	}

	private HTTPFormBase SelectFormImplementation()
	{
		if (FormImpl != null)
		{
			return FormImpl;
		}
		if (FieldCollector == null)
		{
			return null;
		}
		switch (FormUsage)
		{
		case HTTPFormUsage.Automatic:
			if (FieldCollector.HasBinary || FieldCollector.HasLongValue)
			{
				goto case HTTPFormUsage.Multipart;
			}
			goto case HTTPFormUsage.UrlEncoded;
		case HTTPFormUsage.UrlEncoded:
			FormImpl = new HTTPUrlEncodedForm();
			break;
		case HTTPFormUsage.Multipart:
			FormImpl = new HTTPMultiPartForm();
			break;
		}
		FormImpl.CopyFrom(FieldCollector);
		return FormImpl;
	}

	public void AddHeader(string name, string value)
	{
		if (Headers == null)
		{
			Headers = new Dictionary<string, List<string>>();
		}
		if (!Headers.TryGetValue(name, out var value2))
		{
			Headers.Add(name, value2 = new List<string>(1));
		}
		value2.Add(value);
	}

	public void SetHeader(string name, string value)
	{
		if (Headers == null)
		{
			Headers = new Dictionary<string, List<string>>();
		}
		if (!Headers.TryGetValue(name, out var value2))
		{
			Headers.Add(name, value2 = new List<string>(1));
		}
		value2.Clear();
		value2.Add(value);
	}

	public bool RemoveHeader(string name)
	{
		if (Headers == null)
		{
			return false;
		}
		return Headers.Remove(name);
	}

	public bool HasHeader(string name)
	{
		return Headers != null && Headers.ContainsKey(name);
	}

	public string GetFirstHeaderValue(string name)
	{
		if (Headers == null)
		{
			return null;
		}
		List<string> value = null;
		if (Headers.TryGetValue(name, out value) && value.Count > 0)
		{
			return value[0];
		}
		return null;
	}

	public List<string> GetHeaderValues(string name)
	{
		if (Headers == null)
		{
			return null;
		}
		List<string> value = null;
		if (Headers.TryGetValue(name, out value) && value.Count > 0)
		{
			return value;
		}
		return null;
	}

	public void RemoveHeaders()
	{
		if (Headers != null)
		{
			Headers.Clear();
		}
	}

	public void SetRangeHeader(int firstBytePos)
	{
		SetHeader("Range", $"bytes={firstBytePos}-");
	}

	public void SetRangeHeader(int firstBytePos, int lastBytePos)
	{
		SetHeader("Range", $"bytes={firstBytePos}-{lastBytePos}");
	}

	public void EnumerateHeaders(OnHeaderEnumerationDelegate callback)
	{
		if (!HasHeader("Host"))
		{
			SetHeader("Host", CurrentUri.Authority);
		}
		if (IsRedirected && !HasHeader("Referer"))
		{
			AddHeader("Referer", Uri.ToString());
		}
		if (!HasHeader("Accept-Encoding"))
		{
			AddHeader("Accept-Encoding", "gzip, identity");
		}
		if (HasProxy && !HasHeader("Proxy-Connection"))
		{
			AddHeader("Proxy-Connection", (!IsKeepAlive) ? "Close" : "Keep-Alive");
		}
		if (!HasHeader("Connection"))
		{
			AddHeader("Connection", (!IsKeepAlive) ? "Close, TE" : "Keep-Alive, TE");
		}
		if (!HasHeader("TE"))
		{
			AddHeader("TE", "identity");
		}
		if (!HasHeader("User-Agent"))
		{
			AddHeader("User-Agent", "BestHTTP");
		}
		long num = -1L;
		if (UploadStream == null)
		{
			byte[] entityBody = GetEntityBody();
			num = ((entityBody != null) ? entityBody.Length : 0);
			if (RawData == null && (FormImpl != null || (FieldCollector != null && !FieldCollector.IsEmpty)))
			{
				SelectFormImplementation();
				if (FormImpl != null)
				{
					FormImpl.PrepareRequest(this);
				}
			}
		}
		else
		{
			num = UploadStreamLength;
			if (num == -1)
			{
				SetHeader("Transfer-Encoding", "Chunked");
			}
			if (!HasHeader("Content-Type"))
			{
				SetHeader("Content-Type", "application/octet-stream");
			}
		}
		if (num != -1 && !HasHeader("Content-Length"))
		{
			SetHeader("Content-Length", num.ToString());
		}
		if (HasProxy && Proxy.Credentials != null)
		{
			switch (Proxy.Credentials.Type)
			{
			case AuthenticationTypes.Basic:
				SetHeader("Proxy-Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Proxy.Credentials.UserName + ":" + Proxy.Credentials.Password)));
				break;
			case AuthenticationTypes.Unknown:
			case AuthenticationTypes.Digest:
			{
				Digest digest = DigestStore.Get(Proxy.Address);
				if (digest != null)
				{
					string value = digest.GenerateResponseHeader(this, Proxy.Credentials);
					if (!string.IsNullOrEmpty(value))
					{
						SetHeader("Proxy-Authorization", value);
					}
				}
				break;
			}
			}
		}
		if (Credentials != null)
		{
			switch (Credentials.Type)
			{
			case AuthenticationTypes.Basic:
				SetHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Credentials.UserName + ":" + Credentials.Password)));
				break;
			case AuthenticationTypes.Unknown:
			case AuthenticationTypes.Digest:
			{
				Digest digest2 = DigestStore.Get(CurrentUri);
				if (digest2 != null)
				{
					string value2 = digest2.GenerateResponseHeader(this, Credentials);
					if (!string.IsNullOrEmpty(value2))
					{
						SetHeader("Authorization", value2);
					}
				}
				break;
			}
			}
		}
		if (callback == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<string>> header in Headers)
		{
			callback(header.Key, header.Value);
		}
	}

	private void SendHeaders(BinaryWriter stream)
	{
		EnumerateHeaders(delegate(string header, List<string> values)
		{
			byte[] aSCIIBytes = (header + ": ").GetASCIIBytes();
			for (int i = 0; i < values.Count; i++)
			{
				stream.Write(aSCIIBytes);
				stream.Write(values[i].GetASCIIBytes());
				stream.Write(EOL);
			}
		});
	}

	public string DumpHeaders()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter stream = new BinaryWriter(memoryStream);
		SendHeaders(stream);
		return memoryStream.ToArray().AsciiToString();
	}

	internal byte[] GetEntityBody()
	{
		if (RawData != null)
		{
			return RawData;
		}
		if (FormImpl != null || (FieldCollector != null && !FieldCollector.IsEmpty))
		{
			SelectFormImplementation();
			if (FormImpl != null)
			{
				return FormImpl.GetData();
			}
		}
		return null;
	}

	internal void SendOutTo(Stream stream)
	{
		try
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			string text = $"{MethodNames[(uint)MethodType]} {((!HasProxy || !Proxy.SendWholeUri) ? CurrentUri.PathAndQuery : CurrentUri.OriginalString)} HTTP/1.1";
			if (HTTPManager.Logger.Level <= Loglevels.Information)
			{
				HTTPManager.Logger.Information("HTTPRequest", $"Sending request: {text}");
			}
			binaryWriter.Write(text.GetASCIIBytes());
			binaryWriter.Write(EOL);
			SendHeaders(binaryWriter);
			binaryWriter.Write(EOL);
			binaryWriter.Flush();
			byte[] array = RawData;
			if (array == null && FormImpl != null)
			{
				array = FormImpl.GetData();
			}
			if (array == null && UploadStream == null)
			{
				return;
			}
			Stream stream2 = UploadStream;
			if (stream2 == null)
			{
				stream2 = new MemoryStream(array, 0, array.Length);
				UploadLength = array.Length;
			}
			else
			{
				UploadLength = ((!UseUploadStreamLength) ? (-1) : UploadStreamLength);
			}
			Uploaded = 0L;
			byte[] array2 = new byte[UploadChunkSize];
			int num = 0;
			while ((num = stream2.Read(array2, 0, array2.Length)) > 0)
			{
				if (!UseUploadStreamLength)
				{
					binaryWriter.Write(num.ToString("X").GetASCIIBytes());
					binaryWriter.Write(EOL);
				}
				binaryWriter.Write(array2, 0, num);
				if (!UseUploadStreamLength)
				{
					binaryWriter.Write(EOL);
				}
				binaryWriter.Flush();
				Uploaded += num;
				UploadProgressChanged = true;
			}
			if (!UseUploadStreamLength)
			{
				binaryWriter.Write("0".GetASCIIBytes());
				binaryWriter.Write(EOL);
				binaryWriter.Write(EOL);
			}
			binaryWriter.Flush();
			if (UploadStream == null)
			{
				stream2?.Dispose();
			}
		}
		catch (Exception ex)
		{
			HTTPManager.Logger.Exception("HTTPRequest", "SendOutTo", ex);
			throw ex;
		}
		finally
		{
			if (UploadStream != null && DisposeUploadStream)
			{
				UploadStream.Dispose();
			}
		}
	}

	internal void UpgradeCallback()
	{
		if (Response == null || !Response.IsUpgraded)
		{
			return;
		}
		try
		{
			if (OnUpgraded != null)
			{
				OnUpgraded(this, Response);
			}
		}
		catch (Exception ex)
		{
			HTTPManager.Logger.Exception("HTTPRequest", "UpgradeCallback", ex);
		}
	}

	internal void CallCallback()
	{
		try
		{
			if (Callback != null)
			{
				Callback(this, Response);
			}
		}
		catch (Exception ex)
		{
			HTTPManager.Logger.Exception("HTTPRequest", "CallCallback", ex);
		}
	}

	internal bool CallOnBeforeRedirection(Uri redirectUri)
	{
		if (onBeforeRedirection != null)
		{
			return onBeforeRedirection(this, Response, redirectUri);
		}
		return true;
	}

	internal void FinishStreaming()
	{
		if (Response != null && UseStreaming)
		{
			Response.FinishStreaming();
		}
	}

	internal void Prepare()
	{
	}

	internal bool CallCustomCertificationValidator(X509Certificate cert, X509Chain chain)
	{
		if (this.CustomCertificationValidator != null)
		{
			return this.CustomCertificationValidator(this, cert, chain);
		}
		return true;
	}

	public HTTPRequest Send()
	{
		return HTTPManager.SendRequest(this);
	}

	public void Abort()
	{
		lock (HTTPManager.Locker)
		{
			if (State >= HTTPRequestStates.Finished)
			{
				HTTPManager.Logger.Warning("HTTPRequest", $"Abort - Already in a state({State.ToString()}) where no Abort required!");
				return;
			}
			ConnectionBase connectionWith = HTTPManager.GetConnectionWith(this);
			if (connectionWith == null)
			{
				if (!HTTPManager.RemoveFromQueue(this))
				{
					HTTPManager.Logger.Warning("HTTPRequest", "Abort - No active connection found with this request! (The request may already finished?)");
				}
				State = HTTPRequestStates.Aborted;
			}
			else
			{
				if (Response != null && Response.IsStreamed)
				{
					Response.Dispose();
				}
				connectionWith.Abort(HTTPConnectionStates.AbortRequested);
			}
		}
	}

	public void Clear()
	{
		ClearForm();
		RemoveHeaders();
	}

	public bool MoveNext()
	{
		return State < HTTPRequestStates.Finished;
	}

	public void Reset()
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
	}
}
