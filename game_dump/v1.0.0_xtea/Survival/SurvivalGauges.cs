using System.Collections.Generic;

namespace Survival;

public class SurvivalGauges
{
	private Dictionary<string, Gauge> _gauges;

	public SurvivalGauges()
	{
		_gauges = new Dictionary<string, Gauge>();
	}

	public SurvivalGauges(Dictionary<string, Gauge> gauges)
	{
		_gauges = gauges;
	}

	public void SetGauge(string name, Gauge gauge)
	{
		_gauges[name] = gauge;
	}

	public bool ContainsGauge(string name)
	{
		return _gauges.ContainsKey(name);
	}

	public Gauge GetGauge(string name)
	{
		Gauge value = null;
		_gauges.TryGetValue(name, out value);
		return value;
	}

	public void SetGauges(Dictionary<string, Gauge> gauges)
	{
		_gauges = gauges;
	}
}
