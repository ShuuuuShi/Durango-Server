using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using K1Network;
using UnityEngine;

public class PacketWatcher
{
	private class PacketRecord
	{
		public int Count;

		public int Size;

		private float AvgSize
		{
			get
			{
				if (Count > 0)
				{
					return (float)Size / (float)Count;
				}
				return Size;
			}
		}

		public PacketRecord(int size)
		{
			Size = size;
			Count = 1;
		}

		public void Add(int size)
		{
			Size += size;
			Count++;
		}

		public override string ToString()
		{
			return $"{Size:N0} bytes ({Count})\tAvg {AvgSize:N} bytes";
		}

		public string ToString(float durationTime)
		{
			return $"{ToString()}\t{(float)Size / durationTime:N} bytes/s";
		}
	}

	public enum PacketType
	{
		Send,
		Receive
	}

	public struct MessageStruct
	{
		public PacketType Type;

		public ulong Seq;

		public object Msg;

		public int Size;
	}

	private static PacketWatcher _inst;

	private Dictionary<uint, Action<MessageStruct>> _watcherFunc;

	private Dictionary<uint, PacketRecord> _recordReceivePacket = new Dictionary<uint, PacketRecord>();

	private Dictionary<uint, PacketRecord> _recordSendPacket = new Dictionary<uint, PacketRecord>();

	private List<Type> _messageTypes;

	private Dictionary<uint, Type> _messageTypesByTypeCode;

	private float _startTime;

	private int _headerSize;

	private float _recordStartTime;

	private float _recordPeriod;

	private bool _isRecording;

	private int _recordIndex;

	private List<KeyValuePair<uint, List<int>>> _recordReceiveList;

	private List<KeyValuePair<uint, List<int>>> _recordSendList;

	private Dictionary<uint, Action<MessageStruct>> WatcherFunc
	{
		get
		{
			if (_watcherFunc == null)
			{
				_watcherFunc = new Dictionary<uint, Action<MessageStruct>>();
			}
			return _watcherFunc;
		}
	}

	public Dictionary<uint, Type> TypeCodeDict => _messageTypesByTypeCode;

	public int TotalSendSize { get; private set; }

	public int TotalReceiveSize { get; private set; }

	public bool IsRecording => _isRecording;

	private PacketWatcher()
	{
		_headerSize = Marshal.SizeOf(typeof(PacketHeader));
		MakeMessagesList();
		Reset();
	}

	public static PacketWatcher Instance()
	{
		if (_inst == null)
		{
			_inst = new PacketWatcher();
		}
		return _inst;
	}

	public void Reset()
	{
		_recordReceivePacket.Clear();
		_recordSendPacket.Clear();
		_startTime = Time.time;
	}

	public void RecordSendPacket<T>(T msg, int size, ulong seq)
	{
		TotalSendSize += size;
	}

	public void RecordReceivePacket(uint typeCode, byte[] buffer, int bufferSize, ulong seq, MessagePacking packer)
	{
		int num = bufferSize + _headerSize;
		TotalReceiveSize += num;
	}

	public string RecordDataToString()
	{
		StringBuilder str = new StringBuilder();
		float num = Time.time - _startTime;
		str.AppendLine("Receive Packet");
		RecordDataToString(ref str, _recordReceivePacket, num);
		str.AppendLine();
		str.AppendLine("Send Packet");
		RecordDataToString(ref str, _recordSendPacket, num);
		str.AppendLine();
		str.Append(num).Append(" sec");
		str.AppendLine();
		return str.ToString();
	}

	public void SetWatcher(Type type, Action<MessageStruct> func)
	{
		if (TryGetTypeCode(type, out var typeCode))
		{
			SetWatcher(typeCode, func);
		}
	}

	public void SetWatcher(uint typeCode, Action<MessageStruct> func)
	{
		if (func == null)
		{
			RemoveWatcher(typeCode);
		}
		else
		{
			WatcherFunc[typeCode] = func;
		}
	}

	public void RemoveWatcher(Type type)
	{
		SetWatcher(type, null);
	}

	public void RemoveWatcher(uint typeCode)
	{
		WatcherFunc.Remove(typeCode);
	}

	public void ClearWatcher()
	{
		WatcherFunc.Clear();
	}

	public bool HasWatcher(uint typeCode)
	{
		return WatcherFunc.ContainsKey(typeCode);
	}

	private void RecordDataToString(ref StringBuilder str, Dictionary<uint, PacketRecord> data, float durationTime)
	{
		List<KeyValuePair<uint, PacketRecord>> list = data.ToList();
		list.Sort((KeyValuePair<uint, PacketRecord> x, KeyValuePair<uint, PacketRecord> y) => y.Value.Size - x.Value.Size);
		int num = 0;
		foreach (KeyValuePair<uint, PacketRecord> item in list)
		{
			Type messageType = GetMessageType(item.Key);
			if ((object)messageType == null)
			{
				str.Append("Unknown : ");
			}
			else
			{
				str.Append(messageType.Name).Append("\t");
			}
			str.Append(item.Value.ToString(durationTime));
			str.AppendLine();
			num += item.Value.Size;
		}
		str.AppendLine();
		str.Append("Total : ").AppendFormat("{0:N0} byte", num);
		str.AppendLine();
	}

	private void MakeMessagesList()
	{
		_messageTypes = GetAllNamespaceMembers("Messages");
		_messageTypesByTypeCode = new Dictionary<uint, Type>();
		int i = 0;
		for (int count = _messageTypes.Count; i < count; i++)
		{
			if (TryGetTypeCode(_messageTypes[i], out var typeCode))
			{
				_messageTypesByTypeCode.Add(typeCode, _messageTypes[i]);
			}
		}
	}

	private bool TryGetTypeCode(Type type, out uint typeCode)
	{
		FieldInfo field = type.GetField("TypeCode");
		if ((object)field != null)
		{
			typeCode = (uint)field.GetValue(null);
			return true;
		}
		typeCode = 0u;
		return false;
	}

	public Type GetMessageType(uint typeCode)
	{
		if (_messageTypesByTypeCode.TryGetValue(typeCode, out var value))
		{
			return value;
		}
		return null;
	}

	public void RecordEnable(float period)
	{
		_isRecording = true;
		_recordStartTime = Time.time;
		_recordPeriod = Mathf.Max(period, 1f);
		_recordIndex = 0;
		if (_recordReceiveList == null)
		{
			_recordReceiveList = new List<KeyValuePair<uint, List<int>>>();
		}
		else
		{
			_recordReceiveList.Clear();
		}
		if (_recordSendList == null)
		{
			_recordSendList = new List<KeyValuePair<uint, List<int>>>();
		}
		else
		{
			_recordSendList.Clear();
		}
	}

	public void RecordDisable()
	{
		_isRecording = false;
	}

	public string RecordListToString()
	{
		StringBuilder str = new StringBuilder();
		str.AppendLine("Recive Packet");
		RecordListToString(ref str, _recordReceiveList);
		str.AppendLine();
		str.AppendLine("Send Packet");
		RecordListToString(ref str, _recordSendList);
		return str.ToString();
	}

	private void RecordListToString(ref StringBuilder str, List<KeyValuePair<uint, List<int>>> list)
	{
		int i = 0;
		for (int num = list?.Count ?? 0; i < num; i++)
		{
			RecordLineToString(ref str, list[i]);
			str.AppendLine();
		}
	}

	private void RecordLineToString(ref StringBuilder str, KeyValuePair<uint, List<int>> line)
	{
		Type messageType = GetMessageType(line.Key);
		if ((object)messageType == null)
		{
			str.Append("Unknown\t");
		}
		else
		{
			str.Append(messageType.Name).Append('\t');
		}
		int i = 0;
		for (int count = line.Value.Count; i < count; i++)
		{
			str.Append(line.Value[i]);
			if (i < count - 1)
			{
				str.Append('\t');
			}
		}
	}

	private void UpdateRecord()
	{
		if (_isRecording)
		{
			float num = Time.time - _recordStartTime;
			int num2 = Mathf.FloorToInt(num / _recordPeriod);
			if (num2 != _recordIndex)
			{
				UpdateRecord(_recordIndex, num2, _recordReceivePacket, _recordReceiveList);
				UpdateRecord(_recordIndex, num2, _recordSendPacket, _recordSendList);
				_recordIndex = num2;
			}
		}
	}

	private void UpdateRecord(int recordIndex, int currentIndex, Dictionary<uint, PacketRecord> data, List<KeyValuePair<uint, List<int>>> list)
	{
		foreach (KeyValuePair<uint, PacketRecord> datum in data)
		{
			int num = -1;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (list[i].Key == datum.Key)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				num = list.Count;
				KeyValuePair<uint, List<int>> item = new KeyValuePair<uint, List<int>>(datum.Key, new List<int>());
				int[] collection = new int[recordIndex];
				item.Value.AddRange(collection);
				list.Add(item);
			}
			for (int j = recordIndex; j < currentIndex; j++)
			{
				if (list[num].Value.Count > j)
				{
					list[num].Value[j] = datum.Value.Size;
				}
				else
				{
					list[num].Value.Add(datum.Value.Size);
				}
			}
		}
	}

	public static List<Type> GetAllNamespaceMembers(string @namespace)
	{
		IEnumerable<Type> source = from t in Assembly.GetExecutingAssembly().GetTypes()
			where t.Namespace == @namespace
			select t;
		List<Type> list = source.ToList();
		list.Sort((Type t1, Type t2) => string.Compare(t1.Name, t2.Name));
		return list;
	}
}
