using UnityEngine;

public class MoveTrail : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _moveHistorySprites;

	[SerializeField]
	private Transform _trailsParent;

	[SerializeField]
	private Transform _mapTexture;

	[SerializeField]
	private int _maxCount;

	[SerializeField]
	private int _trailDistance;

	private Vector2 _lastPlayerTile;

	private int _moveHistoryIndex;

	private int _moveHistoryCount;

	private Vector2[] _moveHistory;

	private float _sqrDist;

	private int _prevMapSize;

	private Vector3 _prevScale;

	private bool _isDirty;

	private void Start()
	{
		_moveHistory = (Vector2[])(object)new Vector2[_maxCount];
		_sqrDist = (float)(_trailDistance * _trailDistance) / 4f;
	}

	private void Update()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = TerrainA6.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		UpdateMoveHistory(val);
		_lastPlayerTile = val;
		Vector3 localScale = _mapTexture.localScale;
		if (_prevScale != localScale)
		{
			_trailsParent.localScale = _mapTexture.localScale;
			_isDirty = true;
		}
		if (_isDirty)
		{
			UpdateMoveTrails();
		}
	}

	private void UpdateMoveHistory(Vector2 tile)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (_moveHistoryCount == 0)
		{
			_moveHistory[_moveHistoryIndex] = tile;
			_moveHistoryCount++;
			flag = true;
		}
		else
		{
			Vector2 val = _moveHistory[_moveHistoryIndex];
			Vector2 val2 = _lastPlayerTile - tile;
			if (((Vector2)(ref val2)).sqrMagnitude > _sqrDist)
			{
				_moveHistory[_moveHistoryIndex] = tile;
				_moveHistoryCount = 1;
				flag = true;
			}
			else
			{
				Vector2 val3 = val - tile;
				if (((Vector2)(ref val3)).sqrMagnitude > _sqrDist)
				{
					_moveHistoryIndex = (_moveHistoryIndex + 1) % _moveHistory.Length;
					_moveHistory[_moveHistoryIndex] = tile;
					_moveHistoryCount = Mathf.Min(_moveHistoryCount + 1, _moveHistory.Length);
					flag = true;
				}
			}
		}
		MapContext mapContext = KSingleton<MapContext>.Instance();
		if (_prevMapSize != mapContext.MapNGUISize)
		{
			_prevMapSize = mapContext.MapNGUISize;
			flag = true;
		}
		if (flag)
		{
			_isDirty = true;
		}
	}

	private void UpdateMoveTrails()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		_isDirty = false;
		_moveHistorySprites.Set(_moveHistoryCount);
		Vector3 localScale = _mapTexture.localScale;
		for (int i = 0; i < 3; i++)
		{
			((Vector3)(ref localScale))[i] = 1f / ((Vector3)(ref localScale))[i];
		}
		for (int j = 0; j < _moveHistoryCount; j++)
		{
			int num = _moveHistoryIndex - j;
			if (num < 0)
			{
				num += _moveHistory.Length;
			}
			Vector2 tilePos = _moveHistory[num];
			UIWidget component = _moveHistorySprites[j].GetComponent<UIWidget>();
			((Component)component).transform.localScale = localScale;
			component.alpha = 1f - (float)j / (float)_maxCount;
			((Component)component).transform.localPosition = Vector2.op_Implicit(KSingleton<MapContext>.Instance().TileToMapPosition(tilePos, applyScale: false));
		}
	}
}
