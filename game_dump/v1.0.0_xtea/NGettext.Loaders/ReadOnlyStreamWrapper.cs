using System;
using System.IO;

namespace NGettext.Loaders;

public sealed class ReadOnlyStreamWrapper : Stream
{
	private bool _IsClosed;

	public Stream BaseStream { get; private set; }

	public override bool CanRead => !_IsClosed && BaseStream.CanRead;

	public override bool CanSeek => !_IsClosed && BaseStream.CanSeek;

	public override bool CanWrite => false;

	public override long Length
	{
		get
		{
			throw new InvalidOperationException("Stream is in read-only mode.");
		}
	}

	public override long Position
	{
		get
		{
			_CheckIsClosed();
			return BaseStream.Position;
		}
		set
		{
			_CheckIsClosed();
			BaseStream.Position = value;
		}
	}

	public ReadOnlyStreamWrapper(Stream baseStream)
	{
		if (baseStream == null)
		{
			throw new ArgumentNullException("baseStream");
		}
		BaseStream = baseStream;
	}

	public override void Flush()
	{
		throw new InvalidOperationException("Stream is in read-only mode.");
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		_CheckIsClosed();
		return BaseStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		throw new InvalidOperationException("Stream is in read-only mode.");
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		_CheckIsClosed();
		return BaseStream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new InvalidOperationException("Stream is in read-only mode.");
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		_CheckIsClosed();
		return base.BeginRead(buffer, offset, count, callback, state);
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new InvalidOperationException("Stream is in read-only mode.");
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		_CheckIsClosed();
		return base.EndRead(asyncResult);
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		throw new InvalidOperationException("Stream is in read-only mode.");
	}

	public override int ReadByte()
	{
		_CheckIsClosed();
		return base.ReadByte();
	}

	public override void WriteByte(byte value)
	{
		throw new InvalidOperationException("Stream is in read-only mode.");
	}

	public override void Close()
	{
		if (!_IsClosed)
		{
			_IsClosed = true;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!_IsClosed)
		{
			_IsClosed = true;
		}
	}

	private void _CheckIsClosed()
	{
		if (_IsClosed)
		{
			throw new ObjectDisposedException(null, "Stream closed.");
		}
	}
}
