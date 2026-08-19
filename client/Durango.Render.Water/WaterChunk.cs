using UnityEngine;

namespace Durango.Render.Water;

public class WaterChunk : MonoBehaviour
{
	private int _tileCount = -1;

	public GameObject[] WaterTiles { get; private set; }

	public void Init(Material material)
	{
		_tileCount = 16;
		Vector3 vector = new Vector3(800f, 0f, 800f);
		WaterTiles = new GameObject[_tileCount];
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				GameObject gameObject = new GameObject();
				gameObject.AddComponent<MeshFilter>();
				Mesh sharedMesh = WaterMeshCreator.CreateMesh(new Point2(j, i), new Point2(2, 2), new Point2(8, 8));
				gameObject.GetComponent<MeshFilter>().sharedMesh = sharedMesh;
				gameObject.AddComponent<MeshRenderer>();
				gameObject.GetComponent<Renderer>().sharedMaterial = material;
				gameObject.name = "WaterTile";
				gameObject.transform.parent = base.gameObject.transform;
				gameObject.transform.localPosition = new Vector3(((float)j - 2f) * vector.x, 0f, ((float)i - 2f) * vector.z);
				gameObject.transform.localScale = new Vector3(vector.x, 1f, vector.z);
				WaterTiles[num] = gameObject;
				num++;
			}
		}
	}

	public void SetMaterial(Material material)
	{
		for (int i = 0; i < WaterTiles.Length; i++)
		{
			WaterTiles[i].GetComponent<Renderer>().sharedMaterial = material;
		}
	}

	public void UpdateWaterMasking(Color32[][] colors)
	{
		for (int i = 0; i < _tileCount; i++)
		{
			MeshFilter component = WaterTiles[i].GetComponent<MeshFilter>();
			component.sharedMesh.colors32 = colors[i];
		}
	}
}
