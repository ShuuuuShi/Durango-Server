using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Camera;

public class CameraShaker : Singleton<CameraShaker>
{
	private class ShakeArguments
	{
		public Vector2 Power;

		public float Since;

		public float Until;

		public float DampRatio;

		public float Interval;

		public int IteractionCount;
	}

	private readonly List<Vector2> _shakeDirRef = new List<Vector2>();

	private const float DefaultShakeDuration = 0.3f;

	private const float DefaultShakeInterval = 0.02f;

	private readonly ShakeArguments _arguments = new ShakeArguments();

	private void Start()
	{
		InitShakerDirRef();
	}

	private void InitShakerDirRef()
	{
		_shakeDirRef.Add(new Vector2(0f, 0f));
		_shakeDirRef.Add(new Vector2(-0.5f, 1f));
		_shakeDirRef.Add(new Vector2(-0.5f, -1f));
		_shakeDirRef.Add(new Vector2(0.5f, 1f));
		_shakeDirRef.Add(new Vector2(0.5f, -1f));
	}

	private void LateUpdate()
	{
		if (!(Time.realtimeSinceStartup < _arguments.Until) || !(Time.realtimeSinceStartup > _arguments.Since))
		{
			return;
		}
		float num = Time.realtimeSinceStartup - _arguments.Since;
		int iteractionCount = _arguments.IteractionCount;
		if (_arguments.Interval > 0f)
		{
			int num2 = (int)(num / _arguments.Interval);
			if (num2 == _arguments.IteractionCount)
			{
				return;
			}
			_arguments.IteractionCount = num2;
		}
		else
		{
			_arguments.IteractionCount++;
		}
		Vector2 vector = Vector2.Scale(_shakeDirRef[_arguments.IteractionCount % _shakeDirRef.Count], _arguments.Power);
		int num3 = _arguments.IteractionCount / _shakeDirRef.Count - iteractionCount / _shakeDirRef.Count;
		for (int i = 0; i < num3; i++)
		{
			_arguments.Power *= _arguments.DampRatio;
		}
		base.gameObject.transform.position += new Vector3(vector.x, vector.y);
	}

	public void Shake(float shakeScaleU, float shakeScaleV, float? updateInterval = null, float? duration = null, float? dampRatio = null, float? delay = null)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		_arguments.Power = new Vector2(shakeScaleU, shakeScaleV);
		_arguments.Since = realtimeSinceStartup + delay.GetValueOrDefault();
		_arguments.Until = _arguments.Since + duration.GetValueOrDefault(0.3f);
		_arguments.Interval = updateInterval.GetValueOrDefault(0.02f);
		_arguments.DampRatio = dampRatio.GetValueOrDefault(0.5f);
		_arguments.IteractionCount = 0;
	}
}
