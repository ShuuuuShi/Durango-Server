using System;
using Assets.PigeonCoopUtil;
using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

public class PCTrail : IDisposable
{
	public CircularBuffer<PCTrailPoint> Points;

	public Mesh Mesh;

	public Vector3[] verticies;

	public Vector3[] normals;

	public Vector2[] uvs;

	public Color[] colors;

	public int[] indicies;

	public int activePointCount;

	public PCTrail(int numPoints)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		Mesh = new Mesh();
		Mesh.MarkDynamic();
		verticies = (Vector3[])(object)new Vector3[2 * numPoints];
		normals = (Vector3[])(object)new Vector3[2 * numPoints];
		uvs = (Vector2[])(object)new Vector2[2 * numPoints];
		colors = (Color[])(object)new Color[2 * numPoints];
		indicies = new int[2 * numPoints * 3];
		Points = new CircularBuffer<PCTrailPoint>(numPoints);
	}

	public void Dispose()
	{
		if ((Object)(object)Mesh != (Object)null)
		{
			if (Application.isEditor)
			{
				Object.DestroyImmediate((Object)(object)Mesh, true);
			}
			else
			{
				Object.Destroy((Object)(object)Mesh);
			}
		}
		Points.Clear();
		Points = null;
	}
}
