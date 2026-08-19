using MsgPack;

namespace Messages;

public struct Message_
{
	public ulong EntityId;

	public double Time;

	public object Body;

	public static void Pack(Packer packer, Message_ val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.EntityId);
		packer.Pack(val.Time);
		if (val.Body == null)
		{
			packer.PackNull();
		}
		else if (val.Body is RadioTalk)
		{
			RadioTalk.Pack(packer, (RadioTalk)val.Body, hint: true);
		}
		else if (val.Body is RadioDictation)
		{
			RadioDictation.Pack(packer, (RadioDictation)val.Body, hint: true);
		}
		else if (val.Body is RadioText)
		{
			RadioText.Pack(packer, (RadioText)val.Body, hint: true);
		}
		else if (val.Body is RadioNotice)
		{
			RadioNotice.Pack(packer, (RadioNotice)val.Body, hint: true);
		}
		else if (val.Body is RadioPin)
		{
			RadioPin.Pack(packer, (RadioPin)val.Body, hint: true);
		}
		else if (val.Body is RadioChannelUpdated)
		{
			RadioChannelUpdated.Pack(packer, (RadioChannelUpdated)val.Body, hint: true);
		}
		else if (val.Body is RadioEntered)
		{
			RadioEntered.Pack(packer, (RadioEntered)val.Body, hint: true);
		}
		else if (val.Body is RadioLeft)
		{
			RadioLeft.Pack(packer, (RadioLeft)val.Body, hint: true);
		}
		else if (val.Body is RadioPinWithText)
		{
			RadioPinWithText.Pack(packer, (RadioPinWithText)val.Body, hint: true);
		}
	}

	public static Message_ Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Message_ result = default(Message_);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Time = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		unpacker.Read();
		result.Body = null;
		uint num = default(uint);
		if (unpacker.ReadUInt32(ref num))
		{
			switch (num)
			{
			case 2601u:
				result.Body = RadioTalk.Unpack(unpacker);
				break;
			case 2602u:
				result.Body = RadioDictation.Unpack(unpacker);
				break;
			case 2603u:
				result.Body = RadioText.Unpack(unpacker);
				break;
			case 2604u:
				result.Body = RadioNotice.Unpack(unpacker);
				break;
			case 2605u:
				result.Body = RadioPin.Unpack(unpacker);
				break;
			case 2606u:
				result.Body = RadioChannelUpdated.Unpack(unpacker);
				break;
			case 2607u:
				result.Body = RadioEntered.Unpack(unpacker);
				break;
			case 2608u:
				result.Body = RadioLeft.Unpack(unpacker);
				break;
			case 2609u:
				result.Body = RadioPinWithText.Unpack(unpacker);
				break;
			default:
				Debug.LogError((object)("Unexpected type code: " + num));
				break;
			}
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Message_ EntityId={EntityId} Time={Time} Body={Body}>";
	}
}
