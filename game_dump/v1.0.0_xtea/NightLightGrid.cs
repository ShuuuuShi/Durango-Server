using System;
using System.Collections.Generic;
using UnityEngine;

public class NightLightGrid : KSingleton<NightLightGrid>
{
	private const int LightBoundary = 3;

	[SerializeField]
	public GameObject LightMaskPrefab;

	private readonly List<NightLight> _nightLights = new List<NightLight>();

	public NightLight GetNearestFireLight(Vector3 characterPosition)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		double num = Math.Pow(600.0, 2.0);
		int num2 = -1;
		for (int i = 0; i < _nightLights.Count; i++)
		{
			NightLight nightLight = _nightLights[i];
			if (nightLight.IsVisible())
			{
				Vector3 position = ((Component)nightLight).transform.position;
				double num3 = Vector3.SqrMagnitude(position - PlayerBehavior.LocalPlayer.CurrentPosition);
				if (!(num3 > num))
				{
					num2 = i;
					num = num3;
				}
			}
		}
		return (num2 != -1) ? _nightLights[num2] : null;
	}

	public void AddNightLight(NightLight nightLight)
	{
		_nightLights.Add(nightLight);
	}

	public void RemoveNightLight(NightLight nightLight)
	{
		_nightLights.Remove(nightLight);
	}

	public double GetRotationDegree(Vector3 currentPosition, Vector3 lightPosition)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(0f, 1f);
		Vector3 val2 = currentPosition - lightPosition;
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		double num = Math.Atan2(normalized.z - val.y, normalized.x - val.x);
		num = -2.0 * num - Math.PI;
		double distance = Vector3.SqrMagnitude(currentPosition - lightPosition) / 40000f;
		float num2 = Mathf.Clamp(GetDistanceRatio(distance), 0f, 1f);
		return (double)num2 * num;
	}

	public float GetDistanceRatio(double distance)
	{
		return (float)(1.0 - Math.Pow(distance / 3.0, 5.0));
	}
}
