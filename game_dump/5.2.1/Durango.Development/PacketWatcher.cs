using System;
using System.Collections.Generic;
using System.Reflection;
using Durango.Network;
using Durango.Utils;

namespace Durango.Development;

public class PacketWatcher
{
	public enum PacketType
	{
		Send,
		Receive
	}

	public struct MessageStruct
	{
		public PacketType Type;

		public ulong Seq;

		public ulong ReplyOf;

		public object Msg;

		public int Size;
	}

	public struct SequenceItem
	{
		public const int PrefixSize = 33;

		public const int PostfixSize = 4;

		public PacketType Type;

		public long At;

		public PacketHeader Header;

		public int Start;

		public int TotalLength => 33 + Header.PayloadSize + 4;
	}

	private static PacketWatcher _inst;

	private static Dictionary<uint, Type> _typeCodeDict;

	public static Dictionary<uint, Type> TypeCodeDict
	{
		get
		{
			if (_typeCodeDict == null)
			{
				IEnumerable<Type> allNamespaceMembers = Reflection.GetAllNamespaceMembers("Messages");
				_typeCodeDict = new Dictionary<uint, Type>();
				foreach (Type item in allNamespaceMembers)
				{
					if (TryGetTypeCode(item, out var typeCode))
					{
						_typeCodeDict.Add(typeCode, item);
					}
				}
			}
			return _typeCodeDict;
		}
	}

	public int TotalSendSize { get; private set; }

	public int TotalReceiveSize { get; private set; }

	private PacketWatcher()
	{
	}

	public static PacketWatcher Instance()
	{
		if (_inst == null)
		{
			_inst = new PacketWatcher();
		}
		return _inst;
	}

	public static bool HasInstance()
	{
		return _inst != null;
	}

	public void RecordSendPacket(byte[] buffer, int bufferOffset, int bufferSize, ulong seq)
	{
		TotalSendSize += bufferSize;
	}

	public void RecordReceivePacket(PacketHeader header, byte[] payload, int payloadOffset)
	{
		int num = header.PayloadSize + 24;
		TotalReceiveSize += num;
	}

	public static bool TryGetTypeCode(Type type, out uint typeCode)
	{
		FieldInfo field = type.GetField("TypeCode");
		if (field != null)
		{
			typeCode = (uint)field.GetValue(null);
			return true;
		}
		typeCode = 0u;
		return false;
	}

	public static Type GetMessageType(uint typeCode)
	{
		if (TypeCodeDict.TryGetValue(typeCode, out var value))
		{
			return value;
		}
		return null;
	}
}
