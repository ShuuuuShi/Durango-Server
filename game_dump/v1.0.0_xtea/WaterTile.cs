using UnityEngine;

public class WaterTile : MonoBehaviour
{
	public int TileIndex;

	public void Init(Point2 tileIndex, Point2 tileSize, Point2 chunkSize, bool shared = true)
	{
		MeshFilter component = ((Component)this).gameObject.GetComponent<MeshFilter>();
		Mesh sharedMesh = ((!shared) ? KSingleton<WaterMeshCreator>.Instance().CreateMesh(tileIndex, tileSize, chunkSize) : KSingleton<WaterMeshCreator>.Instance().GetSharedMesh(tileIndex, tileSize, chunkSize));
		component.sharedMesh = sharedMesh;
	}
}
