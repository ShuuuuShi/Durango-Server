using System;
using Durango.Network;
using Messages;
using UnityEngine;

public class PlayerBufferTime
{
	private const float MaxBufferTime = 5f;

	private double _preMoveLastTime;

	private float _bufferTime;

	public float BufferTime => Mathf.Min(_bufferTime, 5f);

	public void MoveReceived(Move move)
	{
		if (KUtility.GetSize(move.Movements) != 0 && KUtility.GetSize(move.Movements[0].Path) != 0 && (move.Movements[0].MotionOption & 0x20u) != 0)
		{
			double time = move.Movements[0].Path[0].Time;
			bool continuousMove = time - _preMoveLastTime < double.Epsilon;
			double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
			float timeDiff = (float)(bufferedServerTime - time);
			double num = bufferedServerTime - (double)BufferTime;
			bool preMoveProcessed = num >= _preMoveLastTime;
			UpdateBufferTime(continuousMove, preMoveProcessed, timeDiff);
			UpdateLastLocationTime(move);
		}
	}

	private void UpdateLastLocationTime(Move move)
	{
		Movement movement = move.Movements[move.Movements.Length - 1];
		Location location = movement.Path[movement.Path.Length - 1];
		_preMoveLastTime = Math.Max(location.Time, _preMoveLastTime);
	}

	private void UpdateBufferTime(bool continuousMove, bool preMoveProcessed, float timeDiff)
	{
		if ((!continuousMove || !(timeDiff < _bufferTime)) && preMoveProcessed)
		{
			_bufferTime = Mathf.Max(0f, timeDiff);
		}
	}
}
