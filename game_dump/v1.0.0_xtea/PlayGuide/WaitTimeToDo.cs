namespace PlayGuide;

public class WaitTimeToDo : ToDoBase
{
	private readonly int _timeBegin;

	private readonly int _timeEnd;

	public WaitTimeToDo(int timeBegin, int timeEnd)
	{
		_timeBegin = timeBegin;
		_timeEnd = timeEnd;
	}

	public override void Process()
	{
		if (TimeGauge.CheckTime(_timeBegin, _timeEnd))
		{
			CallComplete();
		}
	}
}
