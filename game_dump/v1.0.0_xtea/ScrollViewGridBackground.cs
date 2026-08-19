using UnityEngine;

public class ScrollViewGridBackground : MonoBehaviour
{
	private enum Horizontal
	{
		Left,
		Right
	}

	private enum Vertical
	{
		Bottom,
		Top
	}

	[SerializeField]
	private bool _onEnable;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UISprite _bgSprite;

	[SerializeField]
	private Vector2 _gridSize;

	[SerializeField]
	private Horizontal _hPivot;

	[SerializeField]
	private Vertical _vPivot;

	private Vector2 _basePos;

	private Vector2 _offset;

	private Vector3 _bgScale;

	private Vector2 _baseClip;

	private Transform _bgTransform;

	private void OnEnable()
	{
		if (_onEnable)
		{
			Reset();
		}
	}

	private void LateUpdate()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = _scrollView.panel.clipOffset - _baseClip - _offset;
		_bgTransform.localPosition = Vector2.op_Implicit(_basePos + Vector2.left * (val.x % _gridSize.x) + Vector2.down * (val.y % _gridSize.y));
	}

	[ExposedInEditor(null)]
	public void Reset()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Reset(_gridSize);
	}

	public void Reset(Vector2 gridSize)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		_gridSize = gridSize;
		if (!((Object)(object)_scrollView == (Object)null))
		{
			UIPanel component = ((Component)_scrollView).GetComponent<UIPanel>();
			float width = component.width;
			float height = component.height;
			_baseClip = component.clipOffset;
			_bgTransform = ((Component)_bgSprite).transform;
			_bgSprite.type = UIBasicSprite.Type.Tiled;
			_bgSprite.pivot = UIWidget.Pivot.BottomLeft;
			UISpriteData atlasSprite = _bgSprite.GetAtlasSprite();
			_bgScale = Vector3.one;
			_bgScale.x = _gridSize.x / (float)atlasSprite.width;
			_bgScale.y = _gridSize.y / (float)atlasSprite.height;
			_bgTransform.localScale = _bgScale;
			_bgSprite.width = (int)((width + _gridSize.x * 2f) / _bgScale.x);
			_bgSprite.height = (int)((height + _gridSize.y * 2f) / _bgScale.y);
			Vector3 position = Vector3.Lerp(component.worldCorners[0], component.worldCorners[2], 0.5f);
			((Component)_bgTransform).transform.position = position;
			_basePos = Vector2.op_Implicit(_bgTransform.localPosition);
			_basePos -= new Vector2(width, height) * 0.5f + _gridSize;
			_offset = Vector2.op_Implicit(Vector3.zero);
			if (_hPivot == Horizontal.Right)
			{
				_offset.x = width % _gridSize.x;
			}
			if (_vPivot == Vertical.Top)
			{
				_offset.y = height % _gridSize.y;
			}
		}
	}
}
