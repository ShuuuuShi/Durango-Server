using System.Collections.Generic;

namespace PlayGuide;

public class GuideRecoder
{
	private int _flowIndex = -1;

	private readonly List<string> _flows = new List<string>();

	public bool IsRecordingEnabled { get; set; }

	public string MoveNext()
	{
		_flowIndex++;
		if (_flowIndex < 0 || _flowIndex >= _flows.Count)
		{
			return null;
		}
		return _flows[_flowIndex];
	}

	public void Record(string flow)
	{
		if (!string.IsNullOrEmpty(flow) && IsRecordingEnabled)
		{
			_flows.Add(flow);
		}
	}

	public List<string> GetFlows()
	{
		return _flows;
	}

	public void Load(IList<string> flows)
	{
		_flowIndex = -1;
		IsRecordingEnabled = false;
		_flows.Clear();
		if (flows == null)
		{
			return;
		}
		for (int i = 0; i < flows.Count; i++)
		{
			if (!string.IsNullOrEmpty(flows[i]))
			{
				_flows.Add(flows[i]);
			}
		}
	}

	public bool IsFinished()
	{
		if (_flows.Count == 0)
		{
			return false;
		}
		return _flows[_flows.Count - 1] == "blank";
	}

	public void RemoveRemains()
	{
		if (_flowIndex < _flows.Count)
		{
			_flows.RemoveRange(_flowIndex, _flows.Count - _flowIndex);
		}
	}
}
