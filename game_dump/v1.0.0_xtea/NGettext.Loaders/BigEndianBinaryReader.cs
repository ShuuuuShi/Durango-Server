using System;
using System.IO;
using System.Text;

namespace NGettext.Loaders;

public class BigEndianBinaryReader : BinaryReader
{
	private byte[] _Buffer = new byte[16];

	public BigEndianBinaryReader(Stream input)
		: base(input)
	{
	}

	public BigEndianBinaryReader(Stream input, Encoding encoding)
		: base(input, encoding)
	{
	}

	public override short ReadInt16()
	{
		_FillBuffer(2);
		return (short)(_Buffer[1] | (_Buffer[0] << 8));
	}

	public override ushort ReadUInt16()
	{
		_FillBuffer(2);
		return (ushort)(_Buffer[1] | (_Buffer[0] << 8));
	}

	public override int ReadInt32()
	{
		_FillBuffer(4);
		return _Buffer[3] | (_Buffer[2] << 8) | (_Buffer[1] << 16) | (_Buffer[0] << 24);
	}

	public override uint ReadUInt32()
	{
		_FillBuffer(4);
		return (uint)(_Buffer[3] | (_Buffer[2] << 8) | (_Buffer[1] << 16) | (_Buffer[0] << 24));
	}

	public override long ReadInt64()
	{
		_FillBuffer(8);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(_Buffer, 0, 8);
		}
		return BitConverter.ToInt64(_Buffer, 0);
	}

	public override ulong ReadUInt64()
	{
		_FillBuffer(8);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(_Buffer, 0, 8);
		}
		return BitConverter.ToUInt64(_Buffer, 0);
	}

	public override float ReadSingle()
	{
		_FillBuffer(4);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(_Buffer, 0, 4);
		}
		return BitConverter.ToSingle(_Buffer, 0);
	}

	public override double ReadDouble()
	{
		_FillBuffer(8);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(_Buffer, 0, 8);
		}
		return BitConverter.ToDouble(_Buffer, 0);
	}

	private void _FillBuffer(int numBytes)
	{
		if (numBytes < 2 || numBytes > _Buffer.Length)
		{
			throw new ArgumentOutOfRangeException("numBytes");
		}
		int num = 0;
		int num2 = 0;
		Stream baseStream = BaseStream;
		if (baseStream == null)
		{
			throw new ObjectDisposedException("Base stream closed.");
		}
		do
		{
			num2 = baseStream.Read(_Buffer, num, numBytes - num);
			if (num2 == 0)
			{
				throw new EndOfStreamException("Unexpected End Of File.");
			}
			num += num2;
		}
		while (num < numBytes);
	}
}
