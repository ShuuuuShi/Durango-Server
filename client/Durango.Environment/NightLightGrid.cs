using System;
using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Environment;

public class NightLightGrid : Singleton<NightLightGrid>
{
	[SerializeField]
	public GameObject LightMaskPrefab;

	private readonly HashSet<NightLight> _nightLights = new HashSet<NightLight>();

	private void LateUpdate()
	{
		foreach (NightLight nightLight in _nightLights)
		{
			nightLight.Process();
		}
	}

	public void AddNightLight(NightLight nightLight)
	{
		_nightLights.Add(nightLight);
	}

	public void RemoveNightLight(NightLight nightLight)
	{
		_nightLights.Remove(nightLight);
	}

	public float GetRotationDegree(Vector3 currentPosition)
	{
		NightLight nearestLight = GetNearestLight(currentPosition);
		if (nearestLight == null)
		{
			return 0f;
		}
		Vector3 normalized = (currentPosition - nearestLight.transform.position).normalized;
		Vector2 vector = new Vector2(0f, 1f);
		float num = Mathf.Atan2(normalized.z - vector.y, normalized.x - vector.x);
		return -2f * num - (float)Math.PI;
	}

	[CanBeNull]
	private NightLight GetNearestLight(Vector3 characterPosition)
	{
		NightLight result = null;
		double num = double.MaxValue;
		foreach (NightLight nightLight in _nightLights)
		{
			if (nightLight.IsVisible())
			{
				double num2 = nightLight.Radius;
				num2 *= num2;
				Vector3 vector = characterPosition - nightLight.transform.position;
				double num3 = Vector3.SqrMagnitude(vector);
				bool flag = nightLight.Direction == default(Vector3) || Vector3.Dot(vector.normalized, nightLight.transform.localToWorldMatrix * nightLight.Direction) > 0f;
				if (!(num3 > num) && !(num3 > num2) && flag)
				{
					result = nightLight;
					num = num3;
				}
			}
		}
		return result;
	}
}
