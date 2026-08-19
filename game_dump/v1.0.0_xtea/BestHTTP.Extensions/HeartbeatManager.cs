using System;
using System.Collections.Generic;

namespace BestHTTP.Extensions;

public sealed class HeartbeatManager
{
	private List<IHeartbeat> Heartbeats = new List<IHeartbeat>();

	private IHeartbeat[] UpdateArray;

	private DateTime LastUpdate = DateTime.MinValue;

	public void Subscribe(IHeartbeat heartbeat)
	{
		lock (Heartbeats)
		{
			if (!Heartbeats.Contains(heartbeat))
			{
				Heartbeats.Add(heartbeat);
			}
		}
	}

	public void Unsubscribe(IHeartbeat heartbeat)
	{
		lock (Heartbeats)
		{
			Heartbeats.Remove(heartbeat);
		}
	}

	public void Update()
	{
		if (LastUpdate == DateTime.MinValue)
		{
			LastUpdate = DateTime.UtcNow;
			return;
		}
		TimeSpan dif = DateTime.UtcNow - LastUpdate;
		LastUpdate = DateTime.UtcNow;
		int num = 0;
		lock (Heartbeats)
		{
			if (UpdateArray == null || UpdateArray.Length < Heartbeats.Count)
			{
				Array.Resize(ref UpdateArray, Heartbeats.Count);
			}
			Heartbeats.CopyTo(0, UpdateArray, 0, Heartbeats.Count);
			num = Heartbeats.Count;
		}
		for (int i = 0; i < num; i++)
		{
			try
			{
				UpdateArray[i].OnHeartbeatUpdate(dif);
			}
			catch
			{
			}
		}
	}
}
