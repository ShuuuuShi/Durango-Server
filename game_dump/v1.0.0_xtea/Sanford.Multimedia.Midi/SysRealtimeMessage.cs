using System.ComponentModel;

namespace Sanford.Multimedia.Midi;

[ImmutableObject(true)]
public sealed class SysRealtimeMessage : ShortMessage
{
	public static readonly SysRealtimeMessage StartMessage = new SysRealtimeMessage(SysRealtimeType.Start);

	public static readonly SysRealtimeMessage ContinueMessage = new SysRealtimeMessage(SysRealtimeType.Continue);

	public static readonly SysRealtimeMessage StopMessage = new SysRealtimeMessage(SysRealtimeType.Stop);

	public static readonly SysRealtimeMessage ClockMessage = new SysRealtimeMessage(SysRealtimeType.Clock);

	public static readonly SysRealtimeMessage TickMessage = new SysRealtimeMessage(SysRealtimeType.Tick);

	public static readonly SysRealtimeMessage ActiveSenseMessage = new SysRealtimeMessage(SysRealtimeType.ActiveSense);

	public static readonly SysRealtimeMessage ResetMessage = new SysRealtimeMessage(SysRealtimeType.Reset);

	public SysRealtimeType SysRealtimeType => (SysRealtimeType)msg;

	public override MessageType MessageType => MessageType.SystemRealtime;

	private SysRealtimeMessage(SysRealtimeType type)
	{
		msg = (int)type;
	}

	public override int GetHashCode()
	{
		return msg;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SysRealtimeMessage))
		{
			return false;
		}
		SysRealtimeMessage sysRealtimeMessage = (SysRealtimeMessage)obj;
		return msg == sysRealtimeMessage.msg;
	}
}
