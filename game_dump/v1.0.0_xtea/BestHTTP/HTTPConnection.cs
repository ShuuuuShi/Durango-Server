using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using BestHTTP.Authentication;
using BestHTTP.Caching;
using BestHTTP.Extensions;
using BestHTTP.Logger;
using BestHTTP.PlatformSupport.TcpClient.General;

namespace BestHTTP;

internal sealed class HTTPConnection : ConnectionBase
{
	private enum RetryCauses
	{
		None,
		Reconnect,
		Authenticate,
		ProxyAuthenticate
	}

	private TcpClient Client;

	private Stream Stream;

	internal HTTPConnection(string serverAddress)
		: base(serverAddress)
	{
	}

	protected override void ThreadFunc(object param)
	{
		bool flag = false;
		bool flag2 = false;
		RetryCauses retryCauses = RetryCauses.None;
		try
		{
			if (!base.HasProxy && base.CurrentRequest.HasProxy)
			{
				base.Proxy = base.CurrentRequest.Proxy;
			}
			if (TryLoadAllFromCache())
			{
				return;
			}
			if (Client != null && !Client.IsConnected())
			{
				Close();
			}
			do
			{
				if (retryCauses == RetryCauses.Reconnect)
				{
					Close();
					Thread.Sleep(100);
				}
				base.LastProcessedUri = base.CurrentRequest.CurrentUri;
				retryCauses = RetryCauses.None;
				Connect();
				if (base.State == HTTPConnectionStates.AbortRequested)
				{
					throw new Exception("AbortRequested");
				}
				if (!base.CurrentRequest.DisableCache)
				{
					HTTPCacheService.SetHeaders(base.CurrentRequest);
				}
				bool flag3 = false;
				try
				{
					base.CurrentRequest.SendOutTo(Stream);
					flag3 = true;
				}
				catch (Exception ex)
				{
					Close();
					if (base.State == HTTPConnectionStates.TimedOut)
					{
						throw new Exception("AbortRequested");
					}
					if (flag || base.CurrentRequest.DisableRetry)
					{
						throw ex;
					}
					flag = true;
					retryCauses = RetryCauses.Reconnect;
				}
				if (!flag3)
				{
					continue;
				}
				bool flag4 = Receive();
				if (base.State == HTTPConnectionStates.TimedOut)
				{
					throw new Exception("AbortRequested");
				}
				if (!flag4 && !flag && !base.CurrentRequest.DisableRetry)
				{
					flag = true;
					retryCauses = RetryCauses.Reconnect;
				}
				if (base.CurrentRequest.Response == null)
				{
					continue;
				}
				switch (base.CurrentRequest.Response.StatusCode)
				{
				case 401:
				{
					string text2 = DigestStore.FindBest(base.CurrentRequest.Response.GetHeaderValues("www-authenticate"));
					if (!string.IsNullOrEmpty(text2))
					{
						Digest orCreate2 = DigestStore.GetOrCreate(base.CurrentRequest.CurrentUri);
						orCreate2.ParseChallange(text2);
						if (base.CurrentRequest.Credentials != null && orCreate2.IsUriProtected(base.CurrentRequest.CurrentUri) && (!base.CurrentRequest.HasHeader("Authorization") || orCreate2.Stale))
						{
							retryCauses = RetryCauses.Authenticate;
						}
					}
					break;
				}
				case 407:
				{
					if (!base.CurrentRequest.HasProxy)
					{
						break;
					}
					string text = DigestStore.FindBest(base.CurrentRequest.Response.GetHeaderValues("proxy-authenticate"));
					if (!string.IsNullOrEmpty(text))
					{
						Digest orCreate = DigestStore.GetOrCreate(base.CurrentRequest.Proxy.Address);
						orCreate.ParseChallange(text);
						if (base.CurrentRequest.Proxy.Credentials != null && orCreate.IsUriProtected(base.CurrentRequest.Proxy.Address) && (!base.CurrentRequest.HasHeader("Proxy-Authorization") || orCreate.Stale))
						{
							retryCauses = RetryCauses.ProxyAuthenticate;
						}
					}
					break;
				}
				case 301:
				case 302:
				case 307:
				case 308:
					if (base.CurrentRequest.RedirectCount < base.CurrentRequest.MaxRedirects)
					{
						base.CurrentRequest.RedirectCount++;
						string firstHeaderValue = base.CurrentRequest.Response.GetFirstHeaderValue("location");
						if (string.IsNullOrEmpty(firstHeaderValue))
						{
							throw new MissingFieldException($"Got redirect status({base.CurrentRequest.Response.StatusCode.ToString()}) without 'location' header!");
						}
						Uri redirectUri = GetRedirectUri(firstHeaderValue);
						if (!base.CurrentRequest.CallOnBeforeRedirection(redirectUri))
						{
							HTTPManager.Logger.Information("HTTPConnection", "OnBeforeRedirection returned False");
							break;
						}
						base.CurrentRequest.RemoveHeader("Host");
						base.CurrentRequest.SetHeader("Referer", base.CurrentRequest.CurrentUri.ToString());
						base.CurrentRequest.RedirectUri = redirectUri;
						base.CurrentRequest.Response = null;
						bool flag5 = true;
						base.CurrentRequest.IsRedirected = flag5;
						flag2 = flag5;
					}
					break;
				}
				TryStoreInCache();
				if (base.CurrentRequest.Response == null || (!base.CurrentRequest.Response.IsClosedManually && base.CurrentRequest.Response.HasHeaderWithValue("connection", "close")))
				{
					Close();
				}
			}
			while (retryCauses != 0);
		}
		catch (TimeoutException exception)
		{
			base.CurrentRequest.Response = null;
			base.CurrentRequest.Exception = exception;
			base.CurrentRequest.State = HTTPRequestStates.ConnectionTimedOut;
			Close();
		}
		catch (Exception exception2)
		{
			if (base.CurrentRequest != null)
			{
				if (base.CurrentRequest.UseStreaming)
				{
					HTTPCacheService.DeleteEntity(base.CurrentRequest.CurrentUri);
				}
				base.CurrentRequest.Response = null;
				switch (base.State)
				{
				case HTTPConnectionStates.AbortRequested:
				case HTTPConnectionStates.Closed:
					base.CurrentRequest.State = HTTPRequestStates.Aborted;
					break;
				case HTTPConnectionStates.TimedOut:
					base.CurrentRequest.State = HTTPRequestStates.TimedOut;
					break;
				default:
					base.CurrentRequest.Exception = exception2;
					base.CurrentRequest.State = HTTPRequestStates.Error;
					break;
				}
			}
			Close();
		}
		finally
		{
			if (base.CurrentRequest != null)
			{
				lock (HTTPManager.Locker)
				{
					if (base.CurrentRequest != null && base.CurrentRequest.Response != null && base.CurrentRequest.Response.IsUpgraded)
					{
						base.State = HTTPConnectionStates.Upgraded;
					}
					else
					{
						base.State = (flag2 ? HTTPConnectionStates.Redirected : ((Client != null) ? HTTPConnectionStates.WaitForRecycle : HTTPConnectionStates.Closed));
					}
					if (base.CurrentRequest.State == HTTPRequestStates.Processing && (base.State == HTTPConnectionStates.Closed || base.State == HTTPConnectionStates.WaitForRecycle))
					{
						if (base.CurrentRequest.Response != null)
						{
							base.CurrentRequest.State = HTTPRequestStates.Finished;
						}
						else
						{
							base.CurrentRequest.State = HTTPRequestStates.Error;
						}
					}
					if (base.CurrentRequest.State == HTTPRequestStates.ConnectionTimedOut)
					{
						base.State = HTTPConnectionStates.Closed;
					}
					LastProcessTime = DateTime.UtcNow;
					if (OnConnectionRecycled != null)
					{
						RecycleNow();
					}
				}
				HTTPCacheService.SaveLibrary();
			}
		}
	}

	private void Connect()
	{
		Uri uri = ((!base.CurrentRequest.HasProxy) ? base.CurrentRequest.CurrentUri : base.CurrentRequest.Proxy.Address);
		if (Client == null)
		{
			Client = new TcpClient();
		}
		if (!Client.Connected)
		{
			Client.ConnectTimeout = base.CurrentRequest.ConnectTimeout;
			Client.Connect(uri.Host, uri.Port);
			if (HTTPManager.Logger.Level <= Loglevels.Information)
			{
				HTTPManager.Logger.Information("HTTPConnection", "Connected to " + uri.Host + ":" + uri.Port);
			}
		}
		else if (HTTPManager.Logger.Level <= Loglevels.Information)
		{
			HTTPManager.Logger.Information("HTTPConnection", "Already connected to " + uri.Host + ":" + uri.Port);
		}
		lock (HTTPManager.Locker)
		{
			base.StartTime = DateTime.UtcNow;
		}
		if (Stream != null)
		{
			return;
		}
		bool flag = HTTPProtocolFactory.IsSecureProtocol(base.CurrentRequest.CurrentUri);
		if (base.HasProxy && (!base.Proxy.IsTransparent || (flag && base.Proxy.NonTransparentForHTTPS)))
		{
			Stream = Client.GetStream();
			BinaryWriter binaryWriter = new BinaryWriter(Stream);
			bool flag2;
			do
			{
				flag2 = false;
				binaryWriter.SendAsASCII($"CONNECT {base.CurrentRequest.CurrentUri.Host}:{base.CurrentRequest.CurrentUri.Port} HTTP/1.1");
				binaryWriter.Write(HTTPRequest.EOL);
				binaryWriter.SendAsASCII("Proxy-Connection: Keep-Alive");
				binaryWriter.Write(HTTPRequest.EOL);
				binaryWriter.SendAsASCII("Connection: Keep-Alive");
				binaryWriter.Write(HTTPRequest.EOL);
				binaryWriter.SendAsASCII($"Host: {base.CurrentRequest.CurrentUri.Host}:{base.CurrentRequest.CurrentUri.Port}");
				binaryWriter.Write(HTTPRequest.EOL);
				if (base.HasProxy && base.Proxy.Credentials != null)
				{
					switch (base.Proxy.Credentials.Type)
					{
					case AuthenticationTypes.Basic:
						binaryWriter.Write(string.Format("Proxy-Authorization: {0}", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(base.Proxy.Credentials.UserName + ":" + base.Proxy.Credentials.Password))).GetASCIIBytes());
						binaryWriter.Write(HTTPRequest.EOL);
						break;
					case AuthenticationTypes.Unknown:
					case AuthenticationTypes.Digest:
					{
						Digest digest = DigestStore.Get(base.Proxy.Address);
						if (digest != null)
						{
							string text = digest.GenerateResponseHeader(base.CurrentRequest, base.Proxy.Credentials);
							if (!string.IsNullOrEmpty(text))
							{
								binaryWriter.Write($"Proxy-Authorization: {text}".GetASCIIBytes());
								binaryWriter.Write(HTTPRequest.EOL);
							}
						}
						break;
					}
					}
				}
				binaryWriter.Write(HTTPRequest.EOL);
				binaryWriter.Flush();
				base.CurrentRequest.ProxyResponse = new HTTPResponse(base.CurrentRequest, Stream, isStreamed: false, isFromCache: false);
				if (!base.CurrentRequest.ProxyResponse.Receive())
				{
					throw new Exception("Connection to the Proxy Server failed!");
				}
				if (HTTPManager.Logger.Level <= Loglevels.Information)
				{
					HTTPManager.Logger.Information("HTTPConnection", "Proxy returned - status code: " + base.CurrentRequest.ProxyResponse.StatusCode + " message: " + base.CurrentRequest.ProxyResponse.Message);
				}
				int statusCode = base.CurrentRequest.ProxyResponse.StatusCode;
				if (statusCode == 407)
				{
					string text2 = DigestStore.FindBest(base.CurrentRequest.ProxyResponse.GetHeaderValues("proxy-authenticate"));
					if (!string.IsNullOrEmpty(text2))
					{
						Digest orCreate = DigestStore.GetOrCreate(base.Proxy.Address);
						orCreate.ParseChallange(text2);
						if (base.Proxy.Credentials != null && orCreate.IsUriProtected(base.Proxy.Address) && (!base.CurrentRequest.HasHeader("Proxy-Authorization") || orCreate.Stale))
						{
							flag2 = true;
						}
					}
				}
				else if (!base.CurrentRequest.ProxyResponse.IsSuccess)
				{
					throw new Exception($"Proxy returned Status Code: \"{base.CurrentRequest.ProxyResponse.StatusCode}\", Message: \"{base.CurrentRequest.ProxyResponse.Message}\" and Response: {base.CurrentRequest.ProxyResponse.DataAsText}");
				}
			}
			while (flag2);
		}
		if (flag)
		{
			SslStream sslStream = new SslStream(Client.GetStream(), leaveInnerStreamOpen: false, (object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors) => base.CurrentRequest.CallCustomCertificationValidator(cert, chain));
			if (!sslStream.IsAuthenticated)
			{
				sslStream.AuthenticateAsClient(base.CurrentRequest.CurrentUri.Host);
			}
			Stream = sslStream;
		}
		else
		{
			Stream = Client.GetStream();
		}
	}

	private bool Receive()
	{
		SupportedProtocols protocol = ((base.CurrentRequest.ProtocolHandler != 0) ? base.CurrentRequest.ProtocolHandler : HTTPProtocolFactory.GetProtocolFromUri(base.CurrentRequest.CurrentUri));
		base.CurrentRequest.Response = HTTPProtocolFactory.Get(protocol, base.CurrentRequest, Stream, base.CurrentRequest.UseStreaming, isFromCache: false);
		if (!base.CurrentRequest.Response.Receive())
		{
			base.CurrentRequest.Response = null;
			return false;
		}
		if (base.CurrentRequest.Response.StatusCode == 304)
		{
			int length;
			using Stream stream = HTTPCacheService.GetBody(base.CurrentRequest.CurrentUri, out length);
			if (!base.CurrentRequest.Response.HasHeader("content-length"))
			{
				base.CurrentRequest.Response.Headers.Add("content-length", new List<string>(1) { length.ToString() });
			}
			base.CurrentRequest.Response.IsFromCache = true;
			base.CurrentRequest.Response.ReadRaw(stream, length);
		}
		return true;
	}

	private bool TryLoadAllFromCache()
	{
		if (base.CurrentRequest.DisableCache || !HTTPCacheService.IsSupported)
		{
			return false;
		}
		try
		{
			if (HTTPCacheService.IsCachedEntityExpiresInTheFuture(base.CurrentRequest))
			{
				base.CurrentRequest.Response = HTTPCacheService.GetFullResponse(base.CurrentRequest);
				if (base.CurrentRequest.Response != null)
				{
					return true;
				}
			}
		}
		catch
		{
			HTTPCacheService.DeleteEntity(base.CurrentRequest.CurrentUri);
		}
		return false;
	}

	private void TryStoreInCache()
	{
		if (!base.CurrentRequest.UseStreaming && !base.CurrentRequest.DisableCache && base.CurrentRequest.Response != null && HTTPCacheService.IsSupported && HTTPCacheService.IsCacheble(base.CurrentRequest.CurrentUri, base.CurrentRequest.MethodType, base.CurrentRequest.Response))
		{
			if (base.CurrentRequest.IsRedirected)
			{
				HTTPCacheService.Store(base.CurrentRequest.Uri, base.CurrentRequest.MethodType, base.CurrentRequest.Response);
			}
			else
			{
				HTTPCacheService.Store(base.CurrentRequest.CurrentUri, base.CurrentRequest.MethodType, base.CurrentRequest.Response);
			}
		}
	}

	private Uri GetRedirectUri(string location)
	{
		Uri uri = null;
		try
		{
			return new Uri(location);
		}
		catch (UriFormatException)
		{
			Uri uri2 = base.CurrentRequest.Uri;
			UriBuilder uriBuilder = new UriBuilder(uri2.Scheme, uri2.Host, uri2.Port, location);
			return uriBuilder.Uri;
		}
	}

	internal override void Abort(HTTPConnectionStates newState)
	{
		base.State = newState;
		HTTPConnectionStates state = base.State;
		if (state == HTTPConnectionStates.TimedOut)
		{
			base.TimedOutStart = DateTime.UtcNow;
		}
		if (Stream != null)
		{
			Stream.Dispose();
		}
	}

	private void Close()
	{
		base.LastProcessedUri = null;
		if (Client == null)
		{
			return;
		}
		try
		{
			Client.Close();
		}
		catch
		{
		}
		finally
		{
			Stream = null;
			Client = null;
		}
	}

	protected override void Dispose(bool disposing)
	{
		Close();
		base.Dispose(disposing);
	}
}
