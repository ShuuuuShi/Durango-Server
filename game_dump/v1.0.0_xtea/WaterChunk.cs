using UnityEngine;

public class WaterChunk : MonoBehaviour
{
	private int _tileCount = -1;

	public GameObject[] WaterTiles { get; private set; }

	public void Init(Material material)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		_tileCount = 16;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(800f, 0f, 800f);
		WaterTiles = (GameObject[])(object)new GameObject[_tileCount];
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				GameObject val2 = new GameObject();
				val2.AddComponent<MeshFilter>();
				Mesh sharedMesh = KSingleton<WaterMeshCreator>.Instance().CreateMesh(new Point2(j, i), new Point2(2, 2), new Point2(8, 8));
				val2.GetComponent<MeshFilter>().sharedMesh = sharedMesh;
				val2.AddComponent<MeshRenderer>();
				val2.GetComponent<Renderer>().sharedMaterial = material;
				((Object)val2).name = "WaterTile";
				val2.transform.parent = ((Component)this).gameObject.transform;
				val2.transform.localPosition = new Vector3(((float)j - 2f) * val.x, 0f, ((float)i - 2f) * val.z);
				val2.transform.localScale = new Vector3(val.x, 1f, val.z);
				WaterTiles[num] = val2;
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
