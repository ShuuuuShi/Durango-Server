using UnityEngine;

public class RiverChunk : MonoBehaviour
{
	private int _tileCount = -1;

	public GameObject[] WaterTiles { get; private set; }

	private void Awake()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		_tileCount = 16;
		WaterTiles = (GameObject[])(object)new GameObject[_tileCount];
		Vector3 localScale = default(Vector3);
		((Vector3)(ref localScale))._002Ector(800f, 1f, 800f);
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				GameObject val = new GameObject();
				val.AddComponent<MeshFilter>();
				Mesh sharedMesh = KSingleton<WaterMeshCreator>.Instance().CreateMesh(new Point2(j, i), new Point2(4, 4), new Point2(16, 16));
				val.GetComponent<MeshFilter>().sharedMesh = sharedMesh;
				val.AddComponent<MeshRenderer>();
				val.GetComponent<Renderer>().sharedMaterial = KSingleton<River>.Instance().SharedMaterial;
				((Object)val).name = "WaterTile";
				val.transform.parent = ((Component)this).gameObject.transform;
				val.transform.localPosition = new Vector3(((float)j - 2f) * localScale.x, 0f, ((float)i - 2f) * localScale.z);
				val.transform.localScale = localScale;
				WaterTiles[num] = val;
				num++;
			}
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
