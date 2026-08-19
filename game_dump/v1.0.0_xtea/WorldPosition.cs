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
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(x, y);
	}

	public Vector3 ToVector3()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(x, 0f, y);
	}

	public Vector3 ToClientPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return TerrainA6.WorldPositionToClientPosition(ToVector3());
	}

	public void SetFromClientPosition(Vector3 clientPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = TerrainA6.ClientPositionToWorldPosition(clientPosition);
		x = val.x;
		y = val.z;
	}

	public override string ToString()
	{
		return $"<WorldPosition x={x} y={y}>";
	}
}
