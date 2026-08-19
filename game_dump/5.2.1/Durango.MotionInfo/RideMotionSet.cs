using System;
using Newtonsoft.Json;

namespace Durango.MotionInfo;

[Serializable]
public class RideMotionSet
{
	[JsonProperty(PropertyName = "mount")]
	public string Mount = "Ride_Mount";

	[JsonProperty(PropertyName = "dis_mount")]
	public string DisMount = "Ride_DisMount";

	[JsonProperty(PropertyName = "stand_mount")]
	public string StandMount = "Ride_Stand_Mount";

	[JsonProperty(PropertyName = "run_mount")]
	public string RunMount = "Ride_Run_Mount";
}
