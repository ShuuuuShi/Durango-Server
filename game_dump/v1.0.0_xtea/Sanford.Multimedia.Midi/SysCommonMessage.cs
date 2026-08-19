using System.ComponentModel;

namespace Sanford.Multimedia.Midi;

[ImmutableObject(true)]
public sealed class SysCommonMessage : ShortMessage
{
	public SysCommonType SysCommonType => (SysCommonType)ShortMessage.UnpackStatus(msg);

	public int Data1 => ShortMessage.UnpackData1(msg);

	public int Data2 => ShortMessage.UnpackData2(msg);

	public override MessageType MessageType => MessageType.SystemCommon;

	public SysCommonMessage(SysCommonType type)
	{
		msg = (int)type;
	}

	public SysCommonMessage(SysCommonType type, int data1)
	{
		msg = (int)type;
		msg = ShortMessage.PackData1(msg, data1);
	}

	public SysCommonMessage(SysCommonType type, int data1, int data2)
	{
		msg = (int)type;
		msg = ShortMessage.PackData1(msg, data1);
		msg = ShortMessage.PackData2(msg, data2);
	}

	internal SysCommonMessage(int message)
	{
		msg = message;
	}

	public override int GetHashCode()
	{
		return msg;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SysCommonMessage))
		{
			return false;
		}
		SysCommonMessage sysCommonMessage = (SysCommonMessage)obj;
		return SysCommonType == sysCommonMessage.SysCommonType && Data1 == sysCommonMessage.Data1 && Data2 == sysCommonMessage.Data2;
	}
}
