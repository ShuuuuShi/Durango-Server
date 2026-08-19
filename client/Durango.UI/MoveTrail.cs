using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

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

	private bool _isDirty;

	private void Start()
	{
		_moveHistory = new Vector2[_maxCount];
		_sqrDist = (float)(_trailDistance * _trailDistance) / 4f;
	}

	private void Update()
	{
		// [แก้เอง] PlayerBehavior.LocalPlayer ยังเป็น null จนกว่าตัวละครจะเกิดในโลก
		// เดิมไม่เช็ค ⇒ โยน NullReferenceException **ทุกเฟรม**ตั้งแต่หน้าโหลด
		// Unity เขียน stack trace ลง log ทุกครั้ง เกมเลยอืดจนโหลดแมพไม่จบ
		if (PlayerBehavior.LocalPlayer == null || _moveHistory == null || _trailsParent == null || _mapTexture == null)
		{
			return;
		}
		Vector2 vector = Util.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		UpdateMoveHistory(vector);
		_lastPlayerTile = vector;
		if (_trailsParent.localScale != _mapTexture.localScale)
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
		bool flag = false;
		if (_moveHistoryCount == 0)
		{
			_moveHistory[_moveHistoryIndex] = tile;
			_moveHistoryCount++;
			flag = true;
		}
		else
		{
			Vector2 vector = _moveHistory[_moveHistoryIndex];
			if ((_lastPlayerTile - tile).sqrMagnitude > _sqrDist)
			{
				_moveHistory[_moveHistoryIndex] = tile;
				_moveHistoryCount = 1;
				flag = true;
			}
			else if ((vector - tile).sqrMagnitude > _sqrDist)
			{
				_moveHistoryIndex = (_moveHistoryIndex + 1) % _moveHistory.Length;
				_moveHistory[_moveHistoryIndex] = tile;
				_moveHistoryCount = Mathf.Min(_moveHistoryCount + 1, _moveHistory.Length);
				flag = true;
			}
		}
		MapContext mapContext = Singleton<MapContext>.Instance();
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
		_isDirty = false;
		_moveHistorySprites.Set(_moveHistoryCount);
		Vector3 localScale = _mapTexture.localScale;
		for (int i = 0; i < 3; i++)
		{
			localScale[i] = 1f / localScale[i];
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
			component.transform.localScale = localScale;
			component.alpha = 1f - (float)j / (float)_maxCount;
			component.transform.localPosition = Singleton<MapContext>.Instance().TileToMapPosition(tilePos, applyScale: false);
		}
	}
}
