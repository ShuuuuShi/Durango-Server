namespace Durango.Logic.Notification;

public sealed class Countable : Notification
{
	private int _count;

	public override int Count
	{
		get
		{
			return _count;
		}
		set
		{
			if (_count != value)
			{
				_count = value;
				OnChanged();
			}
		}
	}

	public override bool On
	{
		get
		{
			return Count > 0;
		}
		set
		{
		}
	}

	public Countable(Type type, ViewType viewType = ViewType.Toggle)
	{
		Type = type;
		base.ViewType = viewType;
	}
}
