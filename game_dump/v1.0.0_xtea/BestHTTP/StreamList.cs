using System;
using System.IO;
using BestHTTP.Extensions;

namespace BestHTTP;

internal sealed class StreamList : Stream
{
	private Stream[] Streams;

	private int CurrentIdx;

	public override bool CanRead
	{
		get
		{
			if (CurrentIdx >= Streams.Length)
			{
				return false;
			}
			return Streams[CurrentIdx].CanRead;
		}
	}

	public override bool CanSeek => false;

	public override bool CanWrite
	{
		get
		{
			if (CurrentIdx >= Streams.Length)
			{
				return false;
			}
			return Streams[CurrentIdx].CanWrite;
		}
	}

	public override long Length
	{
		get
		{
			if (CurrentIdx >= Streams.Length)
			{
				return 0L;
			}
			long num = 0L;
			for (int i = 0; i < Streams.Length; i++)
			{
				num += Streams[i].Length;
			}
			return num;
		}
	}

	public override long Position
	{
		get
		{
			throw new NotImplementedException("Position get");
		}
		set
		{
			throw new NotImplementedException("Position set");
		}
	}

	public StreamList(params Stream[] streams)
	{
		Streams = streams;
		CurrentIdx = 0;
	}

	public override void Flush()
	{
		if (CurrentIdx < Streams.Length)
		{
			for (int i = 0; i <= CurrentIdx; i++)
			{
				Streams[i].Flush();
			}
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (CurrentIdx >= Streams.Length)
		{
			return -1;
		}
		int i;
		for (i = Streams[CurrentIdx].Read(buffer, offset, count); i < count; i += Streams[CurrentIdx].Read(buffer, offset + i, count - i))
		{
			if (CurrentIdx++ >= Streams.Length)
			{
				break;
			}
		}
		return i;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (CurrentIdx < Streams.Length)
		{
			Streams[CurrentIdx].Write(buffer, offset, count);
		}
	}

	public void Write(string str)
	{
		byte[] aSCIIBytes = str.GetASCIIBytes();
		Write(aSCIIBytes, 0, aSCIIBytes.Length);
	}

	protected override void Dispose(bool disposing)
	{
		for (int i = 0; i < Streams.Length; i++)
		{
			try
			{
				Streams[i].Dispose();
			}
			catch (Exception ex)
			{
				HTTPManager.Logger.Exception("StreamList", "Dispose", ex);
			}
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (CurrentIdx >= Streams.Length)
		{
			return 0L;
		}
		return Streams[CurrentIdx].Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException("SetLength");
	}
}
