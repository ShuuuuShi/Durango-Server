using System;
using UnityEngine;

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

	[SerializeField]
	private UIWidget _bottomWidget;

	private Vector3 _cloneViewerPos;

	private float _cloneViewerRatio;

	private UIBase _origin;

	private bool _isPortrait;

	private float _currentRatio;

	private Rect _originScreenRect;

	private Rect _cloneScreenRect;

	private int _postRaycastFrame;

	private int _touchedFrame;

	private UICamera.MouseOrTouch _touch;

	public int BottomMargin => _bottomWidget.height;

	public int BetweenMargin => _dragBox.height;

	private void Start()
	{
		base.OnClose();
	}

	private void OnEnable()
	{
		UIBase.OnOpenCloseableUI += OpenUIChanged;
		UIBase.OnCloseCloseableUI += OpenUIChanged;
	}

	private void OnDisable()
	{
		UIBase.OnOpenCloseableUI -= OpenUIChanged;
		UIBase.OnCloseCloseableUI -= OpenUIChanged;
	}

	protected override bool OnOpen()
	{
		UICamera.onPostRaycast = (Action<UICamera.MouseOrTouch>)Delegate.Combine(UICamera.onPostRaycast, new Action<UICamera.MouseOrTouch>(OnPostRaycast));
		UIScrollView.onPreDrag = (UIScrollView.OnPreDragProcess)Delegate.Combine(UIScrollView.onPreDrag, new UIScrollView.OnPreDragProcess(OnScrollViewPreDrag));
		return base.OnOpen();
	}

	protected override bool OnClose()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		UICamera.onPostRaycast = (Action<UICamera.MouseOrTouch>)Delegate.Remove(UICamera.onPostRaycast, new Action<UICamera.MouseOrTouch>(OnPostRaycast));
		UIScrollView.onPreDrag = (UIScrollView.OnPreDragProcess)Delegate.Remove(UIScrollView.onPreDrag, new UIScrollView.OnPreDragProcess(OnScrollViewPreDrag));
		if ((Object)(object)_origin != (Object)null)
		{
			Vector3 localPosition = ((Component)_origin).transform.localPosition;
			localPosition.x = 0f;
			((Component)_origin).transform.localPosition = localPosition;
		}
		return base.OnClose();
	}

	private void OnPostRaycast(UICamera.MouseOrTouch touch)
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		int frameCount = Time.frameCount;
		if (_postRaycastFrame == frameCount)
		{
			return;
		}
		_postRaycastFrame = frameCount;
		if ((Object)(object)touch.current == (Object)(object)((Component)_touchBox).gameObject || (Object)(object)touch.current == (Object)(object)((Component)_dragBox).gameObject)
		{
			if ((Object)(object)touch.pressed == (Object)null || (Object)(object)touch.pressed == (Object)(object)((Component)_touchBox).gameObject || (Object)(object)touch.pressed == (Object)(object)((Component)_dragBox).gameObject)
			{
				_touch = touch;
			}
		}
		else if (_touch == null || !((Object)(object)_touch.pressed != (Object)null))
		{
			_touch = null;
		}
		if (_touch != null)
		{
			bool flag = false;
			float currentRatio = _currentRatio;
			if ((Object)(object)touch.pressed != (Object)null)
			{
				flag = _touchedFrame < frameCount - 1;
				_touchedFrame = frameCount;
				RefreshViewRect(NGUIMath.ScreenToPixels(touch.pos, ((Component)_touchBox).transform).x - (float)_viewRect.width * 0.5f);
			}
			float num = _currentRatio - currentRatio;
			Rect pixelRect = UICamera.currentCamera.pixelRect;
			Vector2 val = default(Vector2);
			val.x = (touch.pos.x - ((Rect)(ref pixelRect)).x) / ((Rect)(ref pixelRect)).width;
			val.y = (touch.pos.y - ((Rect)(ref pixelRect)).y) / ((Rect)(ref pixelRect)).height;
			Vector2 val2 = default(Vector2);
			val2.x = (val.x - ((Rect)(ref _cloneScreenRect)).x) / ((Rect)(ref _cloneScreenRect)).width;
			val2.y = (val.y - ((Rect)(ref _cloneScreenRect)).y) / ((Rect)(ref _cloneScreenRect)).height;
			Vector2 val3 = default(Vector2);
			val3.x = ((Rect)(ref _originScreenRect)).x + ((Rect)(ref _originScreenRect)).width * (val2.x - _currentRatio);
			val3.y = ((Rect)(ref _originScreenRect)).y + ((Rect)(ref _originScreenRect)).height * val2.y;
			Vector2 val4 = default(Vector2);
			val4.x = ((Rect)(ref pixelRect)).x + ((Rect)(ref pixelRect)).width * val3.x;
			val4.y = ((Rect)(ref pixelRect)).y + ((Rect)(ref pixelRect)).height * val3.y;
			touch.pos = val4;
			touch.delta = ((!flag) ? (val4 - touch.lastPos + Vector2.right * 1280f * num) : Vector2.zero);
			touch.lastPos = val4;
			touch.current = touch.last;
			UICamera.Raycast(touch);
		}
	}

	private void RefreshViewRect(float offset)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		offset = Mathf.Clamp(offset, 0f, (float)(_touchBox.width - _viewRect.width));
		((Component)_viewRect).transform.localPosition = Vector3.right * offset;
		float num = 1280f * (_currentRatio = offset / (float)_touchBox.width);
		((Component)_origin).transform.localPosition = Vector3.left * num;
		((Component)_cloneViewer).transform.localPosition = _cloneViewerPos + Vector3.right * num * _cloneViewerRatio;
	}

	private void OnScrollViewPreDrag(ref Vector3 offset)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsOpen && _touch != null)
		{
			UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
			if (currentTouch == _touch)
			{
				Camera currentCamera = UICamera.currentCamera;
				offset = Vector2.op_Implicit(currentTouch.delta);
				Vector3 val = offset;
				Rect rect = currentCamera.rect;
				float width = ((Rect)(ref rect)).width;
				Rect pixelRect = currentCamera.pixelRect;
				offset = val * (width / ((Rect)(ref pixelRect)).width);
			}
		}
	}

	private void OpenUIChanged()
	{
		if (_isPortrait)
		{
			UIBase fullScreenUI = UIBase.FullScreenUI;
			if ((Object)(object)fullScreenUI != (Object)null && fullScreenUI.Anchor == AnchorType.Fullscreen)
			{
				Open();
				Set(fullScreenUI);
			}
			else
			{
				Close();
			}
		}
	}

	private void Set(UIBase ui)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_origin != (Object)null)
		{
			Vector3 localPosition = ((Component)_origin).transform.localPosition;
			localPosition.x = 0f;
			((Component)_origin).transform.localPosition = localPosition;
		}
		_cloneViewer.SetTarget(((Component)ui).GetComponent<UIPanel>());
		_origin = ui;
		RefreshViewRect(0f);
		UIUtility.UpdateAnchors(((Component)_origin).transform);
	}

	private void OnPortraitMode(bool isPortrait)
	{
		_isPortrait = isPortrait;
		if (isPortrait)
		{
			UpdateClonePosition();
			OpenUIChanged();
		}
		else
		{
			Close();
		}
	}

	private void UpdateClonePosition()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		int screenWidth = UIManager.ScreenWidth;
		int screenHeight = UIManager.ScreenHeight;
		float cloneViewerRatio = (float)screenWidth / 1280f;
		_cloneViewerRatio = cloneViewerRatio;
		((Component)_cloneViewer).transform.localScale = Vector3.one * _cloneViewerRatio;
		UIWidget rootAnchor = UIManager.GetRootAnchor(AnchorType.Fullscreen);
		UIWidget rootAnchor2 = UIManager.GetRootAnchor(AnchorType.Clone);
		Vector2 val = Vector2.op_Implicit(((Component)rootAnchor).transform.localPosition + rootAnchor.localCorners[0]);
		val.x = Mathf.Abs(val.x / (float)rootAnchor.width);
		val.y = Mathf.Abs(val.y / (float)rootAnchor.height);
		_cloneViewerPos = ((Component)rootAnchor2).transform.localPosition + rootAnchor2.localCorners[0] + Vector3.Scale(Vector2.op_Implicit(rootAnchor2.localSize), Vector2.op_Implicit(val));
		((Component)_cloneViewer).transform.localPosition = _cloneViewerPos;
		_viewRect.height = rootAnchor2.height;
		_viewRect.width = (int)((float)screenWidth / (float)rootAnchor.height * (float)_viewRect.height);
		UIUtility.UpdateAnchors(((Component)_viewRect).transform);
		_originArea.width = screenWidth;
		_originArea.height = screenHeight - rootAnchor2.height;
		((Component)_originArea).transform.localPosition = rootAnchor.localCorners[0] + ((Component)rootAnchor).transform.localPosition + Vector3.Scale(Vector2.op_Implicit(_originArea.localSize), Vector2.op_Implicit(_originArea.pivotOffset));
		UIUtility.UpdateAnchors(((Component)_originArea).transform);
		_originScreenRect = new Rect((((Component)rootAnchor).transform.localPosition.x + rootAnchor.localCorners[0].x + (float)screenWidth * 0.5f) / (float)screenWidth, (((Component)rootAnchor).transform.localPosition.y + rootAnchor.localCorners[0].y + (float)screenHeight * 0.5f) / (float)screenHeight, rootAnchor.localSize.x / (float)screenWidth, rootAnchor.localSize.y / (float)screenHeight);
		_cloneScreenRect = new Rect((((Component)rootAnchor2).transform.localPosition.x + rootAnchor2.localCorners[0].x + (float)screenWidth * 0.5f) / (float)screenWidth, (((Component)rootAnchor2).transform.localPosition.y + rootAnchor2.localCorners[0].y + (float)screenHeight * 0.5f) / (float)screenHeight, rootAnchor2.localSize.x / (float)screenWidth, rootAnchor2.localSize.y / (float)screenHeight);
	}
}
