using System;
using System.Text;

namespace Sanford.Multimedia.Midi;

public class MetaTextBuilder : IMessageBuilder
{
	private string text;

	private MetaType type = MetaType.Text;

	private MetaMessage result;

	private bool changed = true;

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			if (value != null)
			{
				text = value;
			}
			else
			{
				text = string.Empty;
			}
			changed = true;
		}
	}

	public MetaType Type
	{
		get
		{
			return type;
		}
		set
		{
			if (!IsTextType(value))
			{
				throw new ArgumentException("Not text based meta message type.", "message");
			}
			type = value;
			changed = true;
		}
	}

	public MetaMessage Result => result;

	public MetaTextBuilder()
	{
		text = string.Empty;
	}

	public MetaTextBuilder(MetaType type)
	{
		if (!IsTextType(type))
		{
			throw new ArgumentException("Not text based meta message type.", "message");
		}
		text = string.Empty;
	}

	public MetaTextBuilder(MetaType type, string text)
	{
		if (!IsTextType(type))
		{
			throw new ArgumentException("Not text based meta message type.", "message");
		}
		this.type = type;
		if (text != null)
		{
			this.text = text;
		}
		else
		{
			this.text = string.Empty;
		}
	}

	public MetaTextBuilder(MetaMessage message)
	{
		Initialize(message);
	}

	public void Initialize(MetaMessage message)
	{
		if (!IsTextType(message.MetaType))
		{
			throw new ArgumentException("Not text based meta message.", "message");
		}
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		text = aSCIIEncoding.GetString(message.GetBytes());
		type = message.MetaType;
	}

	private bool IsTextType(MetaType type)
	{
		if (type == MetaType.Copyright || type == MetaType.CuePoint || type == MetaType.DeviceName || type == MetaType.InstrumentName || type == MetaType.Lyric || type == MetaType.Marker || type == MetaType.ProgramName || type == MetaType.Text || type == MetaType.TrackName)
		{
			return true;
		}
		return false;
	}

	public void Build()
	{
		if (changed)
		{
			ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
			byte[] bytes = aSCIIEncoding.GetBytes(text);
			result = new MetaMessage(Type, bytes);
			changed = false;
		}
	}
}
