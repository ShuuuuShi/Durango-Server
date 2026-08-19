using System;

namespace BestHTTP.Extensions;

public interface IHeartbeat
{
	void OnHeartbeatUpdate(TimeSpan dif);
}
