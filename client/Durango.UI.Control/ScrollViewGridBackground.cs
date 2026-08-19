using UnityEngine;

namespace Durango.UI.Control;

public class ScrollViewGridBackground : MonoBehaviour
{
	public enum Horizontal
	{
		Left,
		Right
	}

	public enum Vertical
	{
		Bottom,
		Top
	}

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UISprite _bgSprite;

	[SerializeField]
	private UISprite _separatorSprite;

	[SerializeField]
	private Vector2 _gridSize;

	[SerializeField]
	private Horizontal _hPivot;

	[SerializeField]
	private Vertical _vPivot;

	private Vector2 _basePos;

	private Vector2 _offset;

	private Vector2 _baseClip;

	[SerializeField]
	[HideInInspector]
	private Transform _container;

	private bool _isInit;

	private ListObjectPool<UISprite> _separators;

	private void LateUpdate()
	{
		if (_isInit)
		{
			Vector2 vector = _scrollView.panel.clipOffset - _baseClip - _offset;
			_container.localPosition = _basePos + Vector2.left * ((!(_gridSize.x > 0f)) ? 0f : (vector.x % _gridSize.x)) + Vector2.down * ((!(_gridSize.y > 0f)) ? 0f : (vector.y % _gridSize.y));
		}
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			Transform container = _container;
			_container = base.gameObject.AddChild().transform;
			if (_bgSprite != null)
			{
				_bgSprite.transform.parent = _container;
				_bgSprite.type = UIBasicSprite.Type.Tiled;
				_bgSprite.pivot = UIWidget.Pivot.BottomLeft;
				_bgSprite.transform.localPosition = Vector3.zero;
			}
			if (_separatorSprite != null)
			{
				_separatorSprite.transform.parent = _container;
				_separators = new ListObjectPool<UISprite>();
				_separators.BaseObject = _separatorSprite;
				_separators.UseBase = true;
			}
			if (container != null)
			{
				Object.Destroy(container.gameObject);
			}
		}
	}

	public void ResetGrid(Vector2 gridSize, Vector2 offset)
	{
		ResetGrid(gridSize, offset, _hPivot, _vPivot);
	}

	public void ResetGrid(Vector2 gridSize, Vector2 offset, Horizontal horizontal, Vertical vertical)
	{
		_gridSize = gridSize;
		_hPivot = horizontal;
		_vPivot = vertical;
		if (!(_scrollView == null))
		{
			Init();
			UIPanel component = _scrollView.GetComponent<UIPanel>();
			float width = component.width;
			float height = component.height;
			_baseClip = component.clipOffset - offset;
			_basePos = base.transform.InverseTransformPoint(component.worldCorners[0]);
			_basePos -= _gridSize;
			if (_bgSprite != null)
			{
				UIUtility.MakeGridBackground(Vector3.zero, Vector2.zero, (int)(width + _gridSize.x * 2f), (int)(height + _gridSize.y * 2f), gridSize, _bgSprite);
			}
			if (_separators != null)
			{
				UIUtility.MakeGridBackground(Vector3.zero, Vector2.zero, (int)(width + _gridSize.x * 2f), (int)(height + _gridSize.y * 2f), gridSize, _separators);
			}
			_offset = Vector3.zero;
			if (_hPivot == Horizontal.Right)
			{
				_offset.x = ((!(gridSize.x > 0f)) ? 0f : (width % _gridSize.x));
			}
			if (_vPivot == Vertical.Top)
			{
				_offset.y = ((!(_gridSize.y > 0f)) ? 0f : (height % _gridSize.y));
			}
		}
	}
}
