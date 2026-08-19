using System;

namespace BestHTTP.Statistics;

[Flags]
public enum StatisticsQueryFlags : byte
{
	Connections = 1,
	Cache = 2,
	Cookies = 4,
	All = byte.MaxValue
}
