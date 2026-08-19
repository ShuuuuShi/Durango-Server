using MsgPack;

namespace Messages;

public struct Message_
{
	public string EntityId;

	public double Time;

	public object Body;

	public RadioId? Speaker;

	public static void Pack(Packer packer, Message_ val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.Time);
		if (val.Body == null)
		{
			packer.PackNull();
		}
		else if (val.Body is TranslatedRadioTalk)
		{
			TranslatedRadioTalk.Pack(packer, (TranslatedRadioTalk)val.Body, hint: true);
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
		else if (val.Body is RadioAlert)
		{
			RadioAlert.Pack(packer, (RadioAlert)val.Body, hint: true);
		}
		else if (val.Body is RadioLink)
		{
			RadioLink.Pack(packer, (RadioLink)val.Body, hint: true);
		}
		if (!val.Speaker.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RadioId.Pack(packer, val.Speaker.Value);
		}
	}

	public static Message_ Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Message_ result = default(Message_);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Time = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Body = null;
		if (unpacker.ReadUInt32(out var result2))
		{
			switch (result2)
			{
			case 32978u:
				result.Body = TranslatedRadioTalk.Unpack(unpacker);
				break;
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
			case 2610u:
				result.Body = RadioAlert.Unpack(unpacker);
				break;
			case 2611u:
				result.Body = RadioLink.Unpack(unpacker);
				break;
			default:
				Debug.LogError("Unexpected type code: " + result2);
				break;
			}
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Speaker = null;
		}
		else
		{
			RadioId value = RadioId.Unpack(unpacker);
			result.Speaker = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Message_ EntityId={EntityId} Time={Time} Body={Body} Speaker={Speaker}>";
	}
}
