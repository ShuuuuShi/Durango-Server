using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Water;

public class RiverChunk : MonoBehaviour
{
	public GameObject[] WaterTiles { get; private set; }

	private void Awake()
	{
		WaterTiles = new GameObject[16];
		Vector3 localScale = new Vector3(800f, 1f, 800f);
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				GameObject gameObject = new GameObject();
				gameObject.AddComponent<MeshFilter>();
				Mesh sharedMesh = WaterMeshCreator.CreateMesh(new Point2(j, i), new Point2(4, 4), new Point2(16, 16));
				gameObject.GetComponent<MeshFilter>().sharedMesh = sharedMesh;
				gameObject.AddComponent<MeshRenderer>();
				gameObject.GetComponent<Renderer>().sharedMaterial = Singleton<River>.Instance().SharedMaterial;
				gameObject.name = "WaterTile";
				gameObject.transform.parent = base.gameObject.transform;
				gameObject.transform.localPosition = new Vector3(((float)j - 2f) * localScale.x, 0f, ((float)i - 2f) * localScale.z);
				gameObject.transform.localScale = localScale;
				WaterTiles[num] = gameObject;
				num++;
			}
		}
	}

	public void UpdateWaterMasking(Color32[][] colors)
	{
		int size = KUtility.GetSize(WaterTiles);
		for (int i = 0; i < size; i++)
		{
			WaterTiles[i].GetComponent<MeshFilter>().sharedMesh.colors32 = colors[i];
		}
	}

	public void SetMaterial(Material material)
	{
		int size = KUtility.GetSize(WaterTiles);
		for (int i = 0; i < size; i++)
		{
			WaterTiles[i].GetComponent<Renderer>().sharedMaterial = material;
		}
	}
}
