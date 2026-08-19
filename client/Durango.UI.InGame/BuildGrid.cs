using UnityEngine;

namespace Durango.UI.InGame;

public class BuildGrid : MonoBehaviour
{
	[SerializeField]
	private float _height;

	[SerializeField]
	private float _yOffset;

	private Point2 _size;

	public void Init(Point2 size)
	{
		_size = size;
		InitGrids();
	}

	private void Awake()
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[24];
		int[] array2 = new int[36];
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			Vector3 vector = new Vector3(200f, _height, 200f);
			Vector3 vector2 = vector;
			vector2[i] = 0f;
			ref Vector3 reference = ref array[num];
			reference = Vector3.zero;
			array[num + 1] = vector2;
			array[num + 1][(i + 1) % 3] = 0f;
			array[num + 2] = vector2;
			array[num + 3] = vector2;
			array[num + 3][(i + 2) % 3] = 0f;
			for (int j = 0; j < 4; j++)
			{
				Vector3 vector3 = array[num + j];
				vector3[i] = vector[i];
				array[num + 4 + j] = vector3;
			}
			num += 8;
		}
		Vector3 vector4 = -new Vector3(200f, 0f, 200f) * 0.5f;
		for (int k = 0; k < array.Length; k++)
		{
			array[k] += vector4;
		}
		int num2 = 0;
		for (int l = 0; l < 6; l++)
		{
			int num3 = num2 * 6;
			int num4 = num2 * 4;
			array2[num3] = num4 + ((num2 % 2 == 0) ? 2 : 0);
			array2[num3 + 1] = num4 + 1;
			array2[num3 + 2] = num4 + ((num2 % 2 != 0) ? 2 : 0);
			array2[num3 + 3] = num4 + ((num2 % 2 == 0) ? 3 : 0);
			array2[num3 + 4] = num4 + 2;
			array2[num3 + 5] = num4 + ((num2 % 2 != 0) ? 3 : 0);
			num2++;
		}
		mesh.vertices = array;
		mesh.triangles = array2;
		GetComponent<MeshFilter>().mesh = mesh;
	}

	private void InitGrids()
	{
		Transform transform = base.transform;
		transform.localPosition = Vector3.up * _yOffset;
		transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		transform.localScale = new Vector3(_size.x, 0f, _size.y);
	}
}
