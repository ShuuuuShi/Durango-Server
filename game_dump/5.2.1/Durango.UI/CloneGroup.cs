using System;
using UnityEngine;

namespace Durango.UI;

public class CloneGroup : UIBase
{
	[SerializeField]
	private UIPanelClone _cloneViewer;

	[SerializeField]
	private UISprite _viewRect;

	[SerializeField]
	private UIWidget _originArea;

	[SerializeField]
	private UIWidget _touchBox;

	[SerializeField]
	private UIWidget _dragBox;

	private Vector3 _cloneViewerPos;

	private float _cloneViewerRatio;

	private UIBase _origin;

	private float _currentRatio;

	private Rect _originScreenRect;

	private Rect _cloneScreenRect;

	private int _postRaycastFrame;

	private int _touchedFrame;

	private UICamera.MouseOrTouch _touch;

	public int BetweenMargin => _dragBox.height;

	private void Start()
	{
		UIBase.UIOpened += OpenUIClosedOpenedChanged;
		UIBase.UIClosed += OpenUIClosedOpenedChanged;
		UICamera.onPostRaycast = (Action<UICamera.MouseOrTouch>)Delegate.Combine(UICamera.onPostRaycast, new Action<UICamera.MouseOrTouch>(OnPostRaycast));
		UIScrollView.onPreDrag = (UIScrollView.OnPreDragProcess)Delegate.Combine(UIScrollView.onPreDrag, new UIScrollView.OnPreDragProcess(OnScrollViewPreDrag));
		base.TryClose();
	}

	private void OnDestroy()
	{
		UICamera.onPostRaycast = (Action<UICamera.MouseOrTouch>)Delegate.Remove(UICamera.onPostRaycast, new Action<UICamera.MouseOrTouch>(OnPostRaycast));
		UIScrollView.onPreDrag = (UIScrollView.OnPreDragProcess)Delegate.Remove(UIScrollView.onPreDrag, new UIScrollView.OnPreDragProcess(OnScrollViewPreDrag));
	}

	protected override bool TryClose()
	{
		if (_origin != null)
		{
			Vector3 localPosition = _origin.transform.localPosition;
			localPosition.x = 0f;
			_origin.transform.localPosition = localPosition;
		}
		return base.TryClose();
	}

	private void OnPostRaycast(UICamera.MouseOrTouch touch)
	{
		if (!base.IsOpened)
		{
			return;
		}
		int frameCount = Time.frameCount;
		if (_postRaycastFrame == frameCount)
		{
			return;
		}
		_postRaycastFrame = frameCount;
		if (touch.current == _touchBox.gameObject || touch.current == _dragBox.gameObject)
		{
			if (touch.pressed == null || touch.pressed == _touchBox.gameObject || touch.pressed == _dragBox.gameObject)
			{
				_touch = touch;
			}
		}
		else if (_touch == null || !(_touch.pressed != null))
		{
			_touch = null;
		}
		if (_touch != null)
		{
			bool flag = false;
			float currentRatio = _currentRatio;
			if (touch.pressed != null)
			{
				flag = _touchedFrame < frameCount - 1;
				_touchedFrame = frameCount;
				RefreshViewRect(NGUIMath.ScreenToPixels(touch.pos, _touchBox.transform).x - (float)_viewRect.width * 0.5f);
			}
			float num = _currentRatio - currentRatio;
			Rect pixelRect = UICamera.currentCamera.pixelRect;
			Vector2 vector = default(Vector2);
			vector.x = (touch.pos.x - pixelRect.x) / pixelRect.width;
			vector.y = (touch.pos.y - pixelRect.y) / pixelRect.height;
			Vector2 vector2 = default(Vector2);
			vector2.x = (vector.x - _cloneScreenRect.x) / _cloneScreenRect.width;
			vector2.y = (vector.y - _cloneScreenRect.y) / _cloneScreenRect.height;
			Vector2 vector3 = default(Vector2);
			vector3.x = _originScreenRect.x + _originScreenRect.width * (vector2.x - _currentRatio);
			vector3.y = _originScreenRect.y + _originScreenRect.height * vector2.y;
			Vector2 vector4 = default(Vector2);
			vector4.x = pixelRect.x + pixelRect.width * vector3.x;
			vector4.y = pixelRect.y + pixelRect.height * vector3.y;
			touch.pos = vector4;
			touch.delta = ((!flag) ? (vector4 - touch.lastPos + Vector2.right * 1280f * num) : Vector2.zero);
			touch.lastPos = vector4;
			touch.current = touch.last;
			UICamera.Raycast(touch);
		}
	}

	private void RefreshViewRect(float offset)
	{
		offset = Mathf.Clamp(offset, 0f, _touchBox.width - _viewRect.width);
		_viewRect.transform.localPosition = Vector3.right * offset;
		float num = 1280f * (_currentRatio = offset / (float)_touchBox.width);
		_origin.transform.localPosition = Vector3.left * num;
		_cloneViewer.transform.localPosition = _cloneViewerPos + Vector3.right * num * _cloneViewerRatio;
	}

	private void OnScrollViewPreDrag(ref Vector3 offset)
	{
		if (base.IsOpened && _touch != null)
		{
			UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
			if (currentTouch == _touch)
			{
				Camera currentCamera = UICamera.currentCamera;
				offset = currentTouch.delta;
				offset *= currentCamera.rect.width / currentCamera.pixelRect.width;
			}
		}
	}

	private void OpenUIClosedOpenedChanged()
	{
		if (base.IsPortrait || base.IsOpened)
		{
			UIBase currentUI = UIBase.CurrentUI;
			if (base.IsPortrait && currentUI != null && currentUI.Anchor == AnchorType.CloneFullscreen)
			{
				Open();
				Set(currentUI);
			}
			else
			{
				Close();
			}
		}
	}

	private void Set(UIBase ui)
	{
		if (_origin != null)
		{
			Vector3 localPosition = _origin.transform.localPosition;
			localPosition.x = 0f;
			_origin.transform.localPosition = localPosition;
		}
		_cloneViewer.SetTarget(ui.GetComponent<UIPanel>());
		_origin = ui;
		RefreshViewRect(0f);
		UIUtility.UpdateAnchors(_origin.transform);
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		if (base.IsPortrait)
		{
			UpdateClonePosition();
			OpenUIClosedOpenedChanged();
		}
		else
		{
			Close();
		}
	}

	private void UpdateClonePosition()
	{
		int screenWidth = UIManager.ScreenWidth;
		int screenHeight = UIManager.ScreenHeight;
		float cloneViewerRatio = (float)screenWidth / 1280f;
		_cloneViewerRatio = cloneViewerRatio;
		_cloneViewer.transform.localScale = Vector3.one * _cloneViewerRatio;
		UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(AnchorType.CloneFullscreen);
		UIWidget rootAnchor2 = UIRootAnchor.GetRootAnchor(AnchorType.Clone);
		Vector2 vector = rootAnchor.transform.localPosition + rootAnchor.localCorners[0];
		vector.x = Mathf.Abs(vector.x / (float)rootAnchor.width);
		vector.y = Mathf.Abs(vector.y / (float)rootAnchor.height);
		_cloneViewerPos = rootAnchor2.transform.localPosition + rootAnchor2.localCorners[0] + Vector3.Scale(rootAnchor2.localSize, vector);
		_cloneViewer.transform.localPosition = _cloneViewerPos;
		_viewRect.height = rootAnchor2.height;
		_viewRect.width = (int)((float)screenWidth / (float)rootAnchor.height * (float)_viewRect.height);
		UIUtility.UpdateAnchors(_viewRect.transform);
		_originArea.width = screenWidth;
		_originArea.height = screenHeight - rootAnchor2.height;
		_originArea.transform.localPosition = rootAnchor.localCorners[0] + rootAnchor.transform.localPosition + Vector3.Scale(_originArea.localSize, _originArea.pivotOffset);
		UIUtility.UpdateAnchors(_originArea.transform);
		_originScreenRect = new Rect((rootAnchor.transform.localPosition.x + rootAnchor.localCorners[0].x + (float)screenWidth * 0.5f) / (float)screenWidth, (rootAnchor.transform.localPosition.y + rootAnchor.localCorners[0].y + (float)screenHeight * 0.5f) / (float)screenHeight, rootAnchor.localSize.x / (float)screenWidth, rootAnchor.localSize.y / (float)screenHeight);
		_cloneScreenRect = new Rect((rootAnchor2.transform.localPosition.x + rootAnchor2.localCorners[0].x + (float)screenWidth * 0.5f) / (float)screenWidth, (rootAnchor2.transform.localPosition.y + rootAnchor2.localCorners[0].y + (float)screenHeight * 0.5f) / (float)screenHeight, rootAnchor2.localSize.x / (float)screenWidth, rootAnchor2.localSize.y / (float)screenHeight);
	}
}
