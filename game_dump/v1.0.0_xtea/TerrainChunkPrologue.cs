using UnityEngine;

public class TerrainChunkPrologue : TerrainChunkA6
{
	public override void Init()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("StaticObjects");
		val.transform.parent = ((Component)this).gameObject.transform;
		val.transform.localPosition = new Vector3(-1600f, 0f, -1600f);
		val.transform.localRotation = Quaternion.identity;
		base.StaticObjectChunk = val.AddComponent<StaticObjectChunk>();
		base.StaticObjectChunk.ResetAllTiles();
	}

	public override bool HasCoords(Vector2 coords)
	{
		return true;
	}
}
