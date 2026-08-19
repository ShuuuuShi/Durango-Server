using UnityEngine;

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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		Mesh val = new Mesh();
		Vector3[] array = (Vector3[])(object)new Vector3[24];
		int[] array2 = new int[36];
		int num = 0;
		Vector3 val2 = default(Vector3);
		for (int i = 0; i < 3; i++)
		{
			((Vector3)(ref val2))._002Ector(200f, _height, 200f);
			Vector3 val3 = val2;
			((Vector3)(ref val3))[i] = 0f;
			ref Vector3 reference = ref array[num];
			reference = Vector3.zero;
			array[num + 1] = val3;
			((Vector3)(ref array[num + 1]))[(i + 1) % 3] = 0f;
			array[num + 2] = val3;
			array[num + 3] = val3;
			((Vector3)(ref array[num + 3]))[(i + 2) % 3] = 0f;
			for (int j = 0; j < 4; j++)
			{
				Vector3 val4 = array[num + j];
				((Vector3)(ref val4))[i] = ((Vector3)(ref val2))[i];
				array[num + 4 + j] = val4;
			}
			num += 8;
		}
		int num2 = 0;
		for (int k = 0; k < 6; k++)
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
		val.vertices = array;
		val.triangles = array2;
		((Component)this).GetComponent<MeshFilter>().mesh = val;
	}

	private void InitGrids()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.transform.localPosition = Vector3.up * _yOffset;
		((Component)this).gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		((Component)this).gameObject.transform.localScale = new Vector3((float)_size.x, 0f, (float)_size.y);
	}

	public void UpdateGrids(bool rotated)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (rotated)
		{
			((Component)this).gameObject.transform.localPosition = new Vector3(0f, _yOffset, (float)(_size.x * 200));
			((Component)this).gameObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
		}
		else
		{
			((Component)this).gameObject.transform.localPosition = Vector3.up * _yOffset;
			((Component)this).gameObject.transform.localRotation = Quaternion.identity;
		}
	}
}
