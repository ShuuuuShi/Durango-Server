using System;
using JetBrains.Annotations;

namespace Durango.Logic.PlayGuide;

public class FlowStack
{
	public bool Started;

	public FlowRegion Region;

	private FlowIterator _iterator;

	private readonly Action _finished;

	public string Name { get; private set; }

	public GuideRecoder Recoder { get; private set; }

	public bool Completed => _iterator == null;

	public bool Progressed
	{
		get
		{
			if (!Started)
			{
				return GetCurrent() != null;
			}
			return true;
		}
	}

	public FlowStack(string name, [NotNull] Flow container, Action finished = null)
	{
		Name = name;
		_iterator = new FlowIterator(container);
		Recoder = new GuideRecoder();
		_finished = finished;
	}

	[CanBeNull]
	public string GetCurrent()
	{
		if (_iterator != null)
		{
			return _iterator.GetCurrent();
		}
		return null;
	}

	[NotNull]
	public string MoveNext(bool canRecord = true, bool canRaiseEvent = false)
	{
		string text = null;
		if (_iterator != null)
		{
			_iterator.MoveNext();
			text = _iterator.GetCurrent();
			if (text == null)
			{
				_iterator = null;
				if (canRaiseEvent && _finished != null)
				{
					_finished();
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "blank";
		}
		if (Recoder != null && canRecord)
		{
			if (Recoder.IsRecordingEnabled)
			{
				Recoder.Record(text);
			}
			else if (text == "very_beginning")
			{
				Recoder.IsRecordingEnabled = true;
			}
		}
		return text;
	}
}
