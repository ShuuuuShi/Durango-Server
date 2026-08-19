using System.Collections;

namespace Sanford.Multimedia.Midi;

public class SysCommonMessageBuilder : IMessageBuilder
{
	private static Hashtable messageCache = Hashtable.Synchronized(new Hashtable());

	private int message;

	private SysCommonMessage result;

	public static int Count => messageCache.Count;

	public SysCommonMessage Result => result;

	internal int Message
	{
		get
		{
			return message;
		}
		set
		{
			message = value;
		}
	}

	public SysCommonType Type
	{
		get
		{
			return (SysCommonType)ShortMessage.UnpackStatus(message);
		}
		set
		{
			message = ShortMessage.PackStatus(message, (int)value);
		}
	}

	public int Data1
	{
		get
		{
			return ShortMessage.UnpackData1(message);
		}
		set
		{
			message = ShortMessage.PackData1(message, value);
		}
	}

	public int Data2
	{
		get
		{
			return ShortMessage.UnpackData2(message);
		}
		set
		{
			message = ShortMessage.PackData2(message, value);
		}
	}

	public SysCommonMessageBuilder()
	{
		Type = SysCommonType.TuneRequest;
	}

	public SysCommonMessageBuilder(SysCommonMessage message)
	{
		Initialize(message);
	}

	public void Initialize(SysCommonMessage message)
	{
		this.message = message.Message;
	}

	public static void Clear()
	{
		messageCache.Clear();
	}

	public void Build()
	{
		result = (SysCommonMessage)messageCache[message];
		if (result == null)
		{
			result = new SysCommonMessage(message);
			messageCache.Add(message, result);
		}
	}
}
