using Durango.Terrain;
using UnityEngine;

public struct WorldPosition
{
	public float x;

	public float y;

	public WorldPosition(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, 0f, y);
	}

	public Vector3 ToClientPosition()
	{
		return ToVector3();
	}

	public void SetFromClientPosition(Vector3 clientPosition)
	{
		x = clientPosition.x;
		y = clientPosition.z;
	}

	public override string ToString()
	{
		return $"<WorldPosition x={x} y={y}>";
	}
}
