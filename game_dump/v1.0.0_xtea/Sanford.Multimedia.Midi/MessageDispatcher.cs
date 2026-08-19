using System;

namespace Sanford.Multimedia.Midi;

public class MessageDispatcher
{
	public event EventHandler<ChannelMessageEventArgs> ChannelMessageDispatched;

	public event EventHandler<SysExMessageEventArgs> SysExMessageDispatched;

	public event EventHandler<SysCommonMessageEventArgs> SysCommonMessageDispatched;

	public event EventHandler<SysRealtimeMessageEventArgs> SysRealtimeMessageDispatched;

	public event EventHandler<MetaMessageEventArgs> MetaMessageDispatched;

	public void Dispatch(IMidiMessage message)
	{
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		switch (message.MessageType)
		{
		case MessageType.Channel:
			OnChannelMessageDispatched(new ChannelMessageEventArgs((ChannelMessage)message));
			break;
		case MessageType.SystemExclusive:
			OnSysExMessageDispatched(new SysExMessageEventArgs((SysExMessage)message));
			break;
		case MessageType.Meta:
			OnMetaMessageDispatched(new MetaMessageEventArgs((MetaMessage)message));
			break;
		case MessageType.SystemCommon:
			OnSysCommonMessageDispatched(new SysCommonMessageEventArgs((SysCommonMessage)message));
			break;
		case MessageType.SystemRealtime:
			switch (((SysRealtimeMessage)message).SysRealtimeType)
			{
			case SysRealtimeType.ActiveSense:
				OnSysRealtimeMessageDispatched(SysRealtimeMessageEventArgs.ActiveSense);
				break;
			case SysRealtimeType.Clock:
				OnSysRealtimeMessageDispatched(SysRealtimeMessageEventArgs.Clock);
				break;
			case SysRealtimeType.Continue:
				OnSysRealtimeMessageDispatched(SysRealtimeMessageEventArgs.Continue);
				break;
			case SysRealtimeType.Reset:
				OnSysRealtimeMessageDispatched(SysRealtimeMessageEventArgs.Reset);
				break;
			case SysRealtimeType.Start:
				OnSysRealtimeMessageDispatched(SysRealtimeMessageEventArgs.Start);
				break;
			case SysRealtimeType.Stop:
				OnSysRealtimeMessageDispatched(SysRealtimeMessageEventArgs.Stop);
				break;
			case SysRealtimeType.Tick:
				OnSysRealtimeMessageDispatched(SysRealtimeMessageEventArgs.Tick);
				break;
			case (SysRealtimeType)253:
				break;
			}
			break;
		}
	}

	protected virtual void OnChannelMessageDispatched(ChannelMessageEventArgs e)
	{
		this.ChannelMessageDispatched?.Invoke(this, e);
	}

	protected virtual void OnSysExMessageDispatched(SysExMessageEventArgs e)
	{
		this.SysExMessageDispatched?.Invoke(this, e);
	}

	protected virtual void OnSysCommonMessageDispatched(SysCommonMessageEventArgs e)
	{
		this.SysCommonMessageDispatched?.Invoke(this, e);
	}

	protected virtual void OnSysRealtimeMessageDispatched(SysRealtimeMessageEventArgs e)
	{
		this.SysRealtimeMessageDispatched?.Invoke(this, e);
	}

	protected virtual void OnMetaMessageDispatched(MetaMessageEventArgs e)
	{
		this.MetaMessageDispatched?.Invoke(this, e);
	}
}
