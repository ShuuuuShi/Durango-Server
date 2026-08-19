using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

public class PCTrailPoint
{
	public Vector3 Forward;

	public Vector3 Position;

	public Vector3 Position2 = Vector3.zero;

	public int PointNumber;

	private float _timeActive;

	private float _distance;

	public virtual void Update(float deltaTime)
	{
		_timeActive += deltaTime;
	}

	public float TimeActive()
	{
		return _timeActive;
	}

	public void SetDistanceFromStart(float distance)
	{
		_distance = distance;
	}

	public float GetDistanceFromStart()
	{
		return _distance;
	}
}
