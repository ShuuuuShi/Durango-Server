using System;
using System.IO;
using MsgPack;
using Snappy;

namespace Durango.Network;

public struct Packet
{
	public PacketHeader Header;

	public byte[] Payload;

	public int PayloadOffset;

	public const int HeaderSize = 24;

	public static T DeserializeMsg<T>(byte[] payload, int payloadOffset, int payloadSize, byte[] decompressingBuffer, MessagePacking packer)
	{
		int count = SnappyCodec.Uncompress(payload, payloadOffset, payloadSize, decompressingBuffer, 0);
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
			T val = msg;
			Debug.LogError("Not registered message " + val);
			return 0;
		}
		int num = SnappyCodec.Compress(packingBuffer, 0, (int)memoryStream.Position, compressingBuffer, 0);
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

	public static PacketHeader ReadPacketHeader(byte[] bytes, int remainBytes, int offset)
	{
		PacketHeader result = default(PacketHeader);
		result.Size = 24;
		if (remainBytes < result.Size)
		{
			return result;
		}
		result.Time = (double)BitConverter.ToUInt64(bytes, offset) / 1000.0;
		result.Seq = BitConverter.ToUInt32(bytes, offset + 8);
		result.ReplyOf = BitConverter.ToUInt32(bytes, offset + 12);
		result.TypeCode = BitConverter.ToUInt32(bytes, offset + 16);
		result.PayloadSize = (int)BitConverter.ToUInt32(bytes, offset + 20);
		if (bytes.Length - result.Size < result.PayloadSize)
		{
			result.Size = 0;
		}
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
		uint typeCode = packet.Header.TypeCode;
		if (typeCode == 1022 || typeCode == 1024 || typeCode == 3650)
		{
			return false;
		}
		return true;
	}
}
