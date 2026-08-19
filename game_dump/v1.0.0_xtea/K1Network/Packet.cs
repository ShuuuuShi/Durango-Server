using System;
using System.IO;
using MsgPack;
using Snappy;

namespace K1Network;

public class Packet
{
	public PacketHeader Header;

	public byte[] Payload;

	public static T DeserializeMsg<T>(byte[] payload, int payloadSize, byte[] decompressingBuffer, MessagePacking packer)
	{
		int count = SnappyCodec.Uncompress(payload, 0, payloadSize, decompressingBuffer, 0);
		using MemoryStream memoryStream = new MemoryStream(decompressingBuffer, 0, count);
		Unpacker val = Unpacker.Create((Stream)memoryStream);
		try
		{
			val.Read();
			return packer.Unpack<T>(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static int SerializeMsg<T>(double time, ulong seq, ulong replyOf, T msg, byte[] dstBuffer, int dstOffset, byte[] packingBuffer, byte[] compressingBuffer, byte[] utf7Buffer, MessagePacking messagePacker)
	{
		using MemoryStream memoryStream = new MemoryStream(packingBuffer);
		Packer val = Packer.Create((Stream)memoryStream);
		try
		{
			if (!messagePacker.Pack(msg, val, out var typeCode))
			{
				Debug.LogError((object)("Not registered message " + msg));
				return 0;
			}
			int num = SnappyCodec.Compress(packingBuffer, 0, (int)memoryStream.Position, compressingBuffer, 0);
			int num2 = SerializeHeader(time, seq, replyOf, typeCode, num, dstBuffer, dstOffset, utf7Buffer);
			Buffer.BlockCopy(compressingBuffer, 0, dstBuffer, dstOffset + num2, num);
			return num2 + num;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static int SerializeHeader(double time, ulong seq, ulong replyOf, uint type, int payloadSize, byte[] buffer, int offset, byte[] utf7Buffer)
	{
		int num = 1;
		int num2 = UTF7.Encode(utf7Buffer, (ulong)(time * 1000.0));
		Buffer.BlockCopy(utf7Buffer, 0, buffer, offset + num, num2);
		num += num2;
		num2 = UTF7.Encode(utf7Buffer, seq);
		Buffer.BlockCopy(utf7Buffer, 0, buffer, offset + num, num2);
		num += num2;
		num2 = UTF7.Encode(utf7Buffer, replyOf);
		Buffer.BlockCopy(utf7Buffer, 0, buffer, offset + num, num2);
		num += num2;
		num2 = UTF7.Encode(utf7Buffer, type);
		Buffer.BlockCopy(utf7Buffer, 0, buffer, offset + num, num2);
		num += num2;
		num2 = UTF7.Encode(utf7Buffer, (ulong)payloadSize);
		Buffer.BlockCopy(utf7Buffer, 0, buffer, offset + num, num2);
		num += num2;
		buffer[offset] = (byte)num;
		return num;
	}

	public static PacketHeader ReadPacketHeader(byte[] bytes, int remainBytes, int offset, byte[] headerBuffer)
	{
		PacketHeader result = default(PacketHeader);
		int num = offset;
		result.Size = bytes[num];
		num++;
		if (remainBytes < result.Size)
		{
			return result;
		}
		Buffer.BlockCopy(bytes, num, headerBuffer, 0, 16);
		num += UTF7.Decode(headerBuffer, out var num2);
		result.Time = (double)num2 / 1000.0;
		Buffer.BlockCopy(bytes, num, headerBuffer, 0, 16);
		num += UTF7.Decode(headerBuffer, out num2);
		result.Seq = num2;
		Buffer.BlockCopy(bytes, num, headerBuffer, 0, 16);
		num += UTF7.Decode(headerBuffer, out num2);
		result.ReplyOf = num2;
		Buffer.BlockCopy(bytes, num, headerBuffer, 0, 16);
		num += UTF7.Decode(headerBuffer, out num2);
		result.TypeCode = (uint)num2;
		Buffer.BlockCopy(bytes, num, headerBuffer, 0, 16);
		num += UTF7.Decode(headerBuffer, out num2);
		result.PayloadSize = (int)num2;
		if (bytes.Length < result.PayloadSize)
		{
			result.Size = 0;
			return result;
		}
		return result;
	}
}
