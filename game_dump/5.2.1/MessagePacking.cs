using System;
using System.Collections.Generic;
using System.Reflection;
using MsgPack;
using MsgPack.Serialization;

public class MessagePacking
{
	public static MessagePackSerializer<MessagePackObjectDictionary> MapSerializer = SerializationContext.Default.GetSerializer<MessagePackObjectDictionary>();

	private Dictionary<Type, PackingBase> _packingsByType = new Dictionary<Type, PackingBase>();

	private Dictionary<uint, PackingBase> _packingsByTypeCode = new Dictionary<uint, PackingBase>();

	[Obsolete]
	public void searchNamespace(string namespaceName = null)
	{
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (namespaceName == null || type.Namespace == namespaceName)
			{
				FieldInfo field = type.GetField("TypeCode", BindingFlags.Static | BindingFlags.Public);
				MethodInfo method = type.GetMethod("Pack", BindingFlags.Static | BindingFlags.Public);
				MethodInfo method2 = type.GetMethod("Unpack", BindingFlags.Static | BindingFlags.Public);
				if (field != null && field.IsLiteral)
				{
					CheckPackType(method);
					CheckUnpackType(method2);
				}
			}
		}
	}

	public void RegisterHandler<T>(Handler<T> handler)
	{
		Type typeFromHandle = typeof(T);
		FieldInfo field = typeFromHandle.GetField("TypeCode", BindingFlags.Static | BindingFlags.Public);
		MethodInfo method = typeFromHandle.GetMethod("Pack", BindingFlags.Static | BindingFlags.Public);
		MethodInfo method2 = typeFromHandle.GetMethod("Unpack", BindingFlags.Static | BindingFlags.Public);
		if (field != null && field.IsLiteral)
		{
			uint num = (uint)field.GetValue(null);
			PackFunc<T> pack = null;
			UnpackFunc<T> unpack = null;
			Packing<T> packing = new Packing<T>(num, pack, unpack, handler);
			if (CheckPackType(method))
			{
				packing._packerInfo = method;
			}
			if (CheckUnpackType(method2))
			{
				packing._unpackerInfo = method2;
			}
			_packingsByType[typeFromHandle] = packing;
			_packingsByTypeCode[num] = packing;
		}
	}

	private bool CheckPackType(MethodInfo pack)
	{
		return true;
	}

	private bool CheckUnpackType(MethodInfo unpack)
	{
		return true;
	}

	public bool Handle(uint typeCode, Unpacker unpacker, out Type type)
	{
		if (!_packingsByTypeCode.TryGetValue(typeCode, out var value))
		{
			type = null;
			return false;
		}
		type = value.GetMsgType();
		return value.HandleMsgPack(unpacker);
	}

	public T Unpack<T>(Unpacker unpacker)
	{
		if (!_packingsByType.TryGetValue(typeof(T), out var value))
		{
			return default(T);
		}
		Packing<T> packing = (Packing<T>)value;
		if (packing._unpack != null)
		{
			return packing._unpack(unpacker);
		}
		return (T)packing._unpackerInfo.Invoke(null, new object[1] { unpacker });
	}

	public bool Pack<T>(T message, Packer packer, out uint typeCode)
	{
		typeCode = 0u;
		if (!_packingsByType.TryGetValue(typeof(T), out var value))
		{
			RegisterHandler<T>(null);
			if (!_packingsByType.TryGetValue(typeof(T), out value))
			{
				Debug.LogError("Cannot pack " + message.ToString());
				return false;
			}
		}
		try
		{
			Packing<T> packing = (Packing<T>)value;
			typeCode = packing.GetTypeCode();
			if (packing._pack != null)
			{
				packing._pack(packer, message, hint: false);
			}
			else
			{
				packing._packerInfo.Invoke(null, new object[3] { packer, message, false });
			}
		}
		catch (Exception message2)
		{
			Debug.LogError(message2);
			return false;
		}
		return true;
	}
}
