using System;
using UnityEngine;

[Serializable]
public class tk2dCameraSettings
{
	public enum ProjectionType
	{
		Orthographic,
		Perspective
	}

	public enum OrthographicType
	{
		PixelsPerMeter,
		OrthographicSize
	}

	public enum OrthographicOrigin
	{
		BottomLeft,
		Center
	}

	public ProjectionType projection;

	public float orthographicSize = 10f;

	public float orthographicPixelsPerMeter = 100f;

	public OrthographicOrigin orthographicOrigin = OrthographicOrigin.Center;

	public OrthographicType orthographicType;

	public TransparencySortMode transparencySortMode;

	public float fieldOfView = 60f;

	public Rect rect = new Rect(0f, 0f, 1f, 1f);
}
