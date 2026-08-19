using MsgPack;
using MsgPack.Serialization;

namespace Messages;

public struct Relay
{
	public const uint TypeCode = 502u;

	public string Method;

	public MessagePackObjectDictionary Data;

	public static void Pack(Packer packer, Relay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(502u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Method == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Method);
		}
		MessagePackSerializer<MessagePackObjectDictionary> val2 = MessagePackSerializer.Get<MessagePackObjectDictionary>();
		val2.PackTo(packer, val.Data);
	}

	public static Relay Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Relay result = default(Relay);
		result.Method = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackSerializer<MessagePackObjectDictionary> serializer = SerializationContext.Default.GetSerializer<MessagePackObjectDictionary>();
		result.Data = serializer.UnpackFrom(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Relay Method={Method} Data={Data}>";
	}
}
