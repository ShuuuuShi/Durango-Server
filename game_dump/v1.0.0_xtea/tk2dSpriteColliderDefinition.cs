using System;
using UnityEngine;

[Serializable]
public class tk2dSpriteColliderDefinition
{
	public enum Type
	{
		Box,
		Circle
	}

	public Type type;

	public Vector3 origin;

	public float angle;

	public string name = string.Empty;

	public Vector3[] vectors = (Vector3[])(object)new Vector3[0];

	public float[] floats = new float[0];

	public float Radius => (type != Type.Circle) ? 0f : floats[0];

	public Vector3 Size => (type != 0) ? Vector3.zero : vectors[0];

	public tk2dSpriteColliderDefinition(Type type, Vector3 origin, float angle)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		this.type = type;
		this.origin = origin;
		this.angle = angle;
	}
}
