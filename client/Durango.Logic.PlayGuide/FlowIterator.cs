using JetBrains.Annotations;

namespace Durango.Logic.PlayGuide;

public class FlowIterator
{
	[NotNull]
	private readonly Flow _flow;

	private int _index;

	public FlowIterator([NotNull] Flow flow)
	{
		_flow = flow;
		_index = -1;
	}

	public void MoveNext()
	{
		_index++;
	}

	[CanBeNull]
	public string GetCurrent()
	{
		if (_index < 0 || _index >= _flow.List.Count)
		{
			return null;
		}
		return _flow.List[_index];
	}
}
