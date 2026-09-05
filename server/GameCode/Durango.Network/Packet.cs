using System;
using System.IO;
using MsgPack;
using Snappier;

namespace Durango.Network;

public struct Packet
{
	public PacketHeader Header;

	public byte[] Payload;

	public int PayloadOffset;

	public const int HeaderSize = 24;

	/// <summary>ขนาด payload สูงสุดที่ยอมรับ/ส่งใน connection หนึ่ง frame</summary>
	public const int MaxPayloadSize = 524288 - HeaderSize;

	public static T DeserializeMsg<T>(byte[] payload, int payloadOffset, int payloadSize, byte[] decompressingBuffer, MessagePacking packer)
	{
		int count = Snappy.Decompress(payload.AsSpan(payloadOffset, payloadSize), decompressingBuffer);
		using MemoryStream stream = new MemoryStream(decompressingBuffer, 0, count);
		using Unpacker unpacker = Unpacker.Create(stream);
		unpacker.Read();
		return packer.Unpack<T>(unpacker);
	}

	public static int SerializeMsg<T>(double time, uint seq, uint replyOf, T msg, byte[] dstBuffer, int dstOffset, byte[] packingBuffer, byte[] compressingBuffer, MessagePacking messagePacker)
	{
		using MemoryStream memoryStream = new MemoryStream(packingBuffer);
		using Packer packer = Packer.Create(memoryStream);
		if (!messagePacker.Pack(msg, packer, out var typeCode))
		{
			Debug.LogError("Not registered message " + msg);
			return 0;
		}
		int num = Snappy.Compress(packingBuffer.AsSpan(0, (int)memoryStream.Position), compressingBuffer);
		if (num > MaxPayloadSize || dstBuffer == null || dstOffset < 0 || dstOffset > dstBuffer.Length - HeaderSize - num)
		{
			throw new InvalidDataException($"Packet payload is too large: {num} bytes");
		}
		int num2 = WritePacketHeader(time, seq, replyOf, typeCode, num, dstBuffer, dstOffset);
		Buffer.BlockCopy(compressingBuffer, 0, dstBuffer, dstOffset + num2, num);
		return num2 + num;
	}

	public static int WritePacketHeader(double time, uint seq, uint replyOf, uint type, int payloadSize, byte[] buffer, int offset)
	{
		WriteUlong((ulong)(time * 1000.0), buffer, offset);
		WriteUint(seq, buffer, offset + 8);
		WriteUint(replyOf, buffer, offset + 12);
		WriteUint(type, buffer, offset + 16);
		WriteUint((uint)payloadSize, buffer, offset + 20);
		return 24;
	}

	public static bool TryReadPacketHeader(byte[] bytes, int remainBytes, int offset, out PacketHeader result, out bool incomplete)
	{
		result = new PacketHeader { Size = HeaderSize };
		incomplete = false;
		if (bytes == null || offset < 0 || remainBytes < HeaderSize)
		{
			incomplete = true;
			result.Size = 0;
			return false;
		}
		if (offset > bytes.Length - HeaderSize || remainBytes > bytes.Length - offset)
		{
			result.Size = 0;
			return false;
		}
		result.Time = (double)BitConverter.ToUInt64(bytes, offset) / 1000.0;
		result.Seq = BitConverter.ToUInt32(bytes, offset + 8);
		result.ReplyOf = BitConverter.ToUInt32(bytes, offset + 12);
		result.TypeCode = BitConverter.ToUInt32(bytes, offset + 16);
		uint payloadSize = BitConverter.ToUInt32(bytes, offset + 20);
		if (payloadSize > MaxPayloadSize)
		{
			result.Size = 0;
			return false;
		}
		if (payloadSize > (uint)(remainBytes - HeaderSize))
		{
			incomplete = true;
			result.Size = 0;
			return false;
		}
		result.PayloadSize = (int)payloadSize;
		return true;
	}

	public static PacketHeader ReadPacketHeader(byte[] bytes, int remainBytes, int offset)
	{
		TryReadPacketHeader(bytes, remainBytes, offset, out PacketHeader result, out _);
		return result;
	}

	private static void WriteUlong(ulong value, byte[] bytes, int offset)
	{
		for (int i = 0; i < 8; i++)
		{
			bytes[offset + i] = (byte)(value >> i * 8);
		}
	}

	private static void WriteUint(uint value, byte[] bytes, int offset)
	{
		for (int i = 0; i < 4; i++)
		{
			bytes[offset + i] = (byte)(value >> i * 8);
		}
	}

	public static bool IsSuccess(Packet packet)
	{
		switch (packet.Header.TypeCode)
		{
		case 1022u:
		case 1024u:
		case 3650u:
			return false;
		default:
			return true;
		}
	}
}
