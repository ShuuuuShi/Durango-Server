using JetBrains.Annotations;
using UnityEngine;

public class StaticObjectChunk : MonoBehaviour
{
	private TileObject[,] _tileObjects;

	private void Awake()
	{
		_tileObjects = new TileObject[16, 16];
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				_tileObjects[i, j] = new TileObject();
			}
		}
	}

	public void ResetAllTiles()
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				TileObject tileObject = _tileObjects[j, i];
				tileObject.Reset();
			}
		}
	}

	public void AttachObject(Point2 tile, GameObject staticObject, bool center, Vector3 offset, Quaternion rotation)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (center)
		{
			offset.x += 100f;
			offset.z += 100f;
		}
		staticObject.transform.parent = ((Component)this).gameObject.transform;
		staticObject.transform.localRotation = rotation;
		staticObject.transform.localPosition = new Vector3((float)(tile.x * 200) + offset.x, offset.y, (float)(tile.y * 200) + offset.z);
	}

	[CanBeNull]
	public TileObject GetTileObject(Point2 tile)
	{
		if (tile.x < 0 || 16 <= tile.x || tile.y < 0 || 16 <= tile.y)
		{
			return null;
		}
		return _tileObjects[tile.x, tile.y];
	}

	public Point2 GetNearestEmptyTile(Point2 tile)
	{
		int num = int.MaxValue;
		Point2 result = tile;
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				TileObject tileObject = _tileObjects[j, i];
				if (tileObject.TileType == TileObject.Type.Empty)
				{
					int num2 = (j - tile.x) * (j - tile.x) + (i - tile.y) * (i - tile.y);
					if (num2 < num)
					{
						num = num2;
						result = new Point2(j, i);
					}
				}
			}
		}
		return result;
	}
}
