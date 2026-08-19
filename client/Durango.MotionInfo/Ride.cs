using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Durango.MotionInfo;

public class Ride
{
	[JsonProperty(PropertyName = "motions")]
	private Dictionary<string, string> _motions;

	[JsonProperty(PropertyName = "motion_sets")]
	private Dictionary<string, RideMotionSet> _motionSets;

	[JsonProperty(PropertyName = "default_motion_set_name")]
	private string _defaultMotionSetName;

	[NotNull]
	private RideMotionSet _defaultMotionSet = new RideMotionSet();

	[NotNull]
	public RideMotionSet Get(string vehicleName)
	{
		if (_motionSets == null || _motions == null)
		{
			return _defaultMotionSet;
		}
		if (!_motions.TryGetValueWithSubStringKey(vehicleName, out var value))
		{
			return _defaultMotionSet;
		}
		if (!_motionSets.TryGetValueWithSubStringKey(value, out var value2))
		{
			return _defaultMotionSet;
		}
		if (string.IsNullOrEmpty(value2.Mount) || string.IsNullOrEmpty(value2.DisMount) || string.IsNullOrEmpty(value2.StandMount) || string.IsNullOrEmpty(value2.RunMount))
		{
			return _defaultMotionSet;
		}
		return value2;
	}

	[OnDeserialized]
	private void CheckRideMotions(StreamingContext context)
	{
		if (string.IsNullOrEmpty(_defaultMotionSetName))
		{
		}
		if (_motionSets == null)
		{
			return;
		}
		foreach (KeyValuePair<string, RideMotionSet> motionSet in _motionSets)
		{
			if (!string.IsNullOrEmpty(motionSet.Value.DisMount) && !string.IsNullOrEmpty(motionSet.Value.Mount) && !string.IsNullOrEmpty(motionSet.Value.StandMount) && !string.IsNullOrEmpty(motionSet.Value.RunMount))
			{
			}
		}
		if (_motions != null)
		{
			foreach (KeyValuePair<string, string> motion in _motions)
			{
				if (_motionSets.ContainsKey(motion.Value))
				{
				}
			}
		}
		_motionSets.TryGetValueWithSubStringKey(_defaultMotionSetName, out var value);
		if (value != null)
		{
			_defaultMotionSet = value;
		}
	}
}
