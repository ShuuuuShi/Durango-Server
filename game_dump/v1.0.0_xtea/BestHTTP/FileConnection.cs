using System;
using System.IO;

namespace BestHTTP;

internal sealed class FileConnection : ConnectionBase
{
	public FileConnection(string serverAddress)
		: base(serverAddress)
	{
	}

	internal override void Abort(HTTPConnectionStates newState)
	{
		base.State = newState;
		HTTPConnectionStates state = base.State;
		if (state == HTTPConnectionStates.TimedOut)
		{
			base.TimedOutStart = DateTime.UtcNow;
		}
		throw new NotImplementedException();
	}

	protected override void ThreadFunc(object param)
	{
		try
		{
			using FileStream fileStream = new FileStream(base.CurrentRequest.CurrentUri.LocalPath, FileMode.Open);
			using StreamList streamList = new StreamList(new MemoryStream(), fileStream);
			streamList.Write("HTTP/1.1 200 Ok\r\n");
			streamList.Write("Content-Type: application/octet-stream\r\n");
			streamList.Write("Content-Length: " + fileStream.Length + "\r\n");
			streamList.Write("\r\n");
			streamList.Seek(0L, SeekOrigin.Begin);
			base.CurrentRequest.Response = new HTTPResponse(base.CurrentRequest, streamList, base.CurrentRequest.UseStreaming, isFromCache: false);
			if (!base.CurrentRequest.Response.Receive())
			{
				base.CurrentRequest.Response = null;
			}
		}
		catch (Exception exception)
		{
			if (base.CurrentRequest != null)
			{
				base.CurrentRequest.Response = null;
				switch (base.State)
				{
				case HTTPConnectionStates.AbortRequested:
					base.CurrentRequest.State = HTTPRequestStates.Aborted;
					break;
				case HTTPConnectionStates.TimedOut:
					base.CurrentRequest.State = HTTPRequestStates.TimedOut;
					break;
				default:
					base.CurrentRequest.Exception = exception;
					base.CurrentRequest.State = HTTPRequestStates.Error;
					break;
				}
			}
		}
		finally
		{
			base.State = HTTPConnectionStates.Closed;
			if (base.CurrentRequest.State == HTTPRequestStates.Processing)
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
		}
	}
}
