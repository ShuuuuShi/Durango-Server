namespace PlayGuide;

public class FlowStackItem
{
	private readonly FlowContainer _flows;

	private int _index;

	public FlowStackItem(FlowContainer flows)
	{
		_flows = flows;
		_index = -1;
	}

	public void MoveNext()
	{
		_index++;
	}

	public FlowData GetCurrent()
	{
		if (_flows == null || _index < 0 || _index >= _flows.List.Count)
		{
			return null;
		}
		return _flows.List[_index];
	}

	public FlowData GetNext()
	{
		int num = _index + 1;
		if (_flows == null || num < 0 || num >= _flows.List.Count)
		{
			return null;
		}
		return _flows.List[num];
	}
}
