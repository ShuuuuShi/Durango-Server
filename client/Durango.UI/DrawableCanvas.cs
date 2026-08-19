using System;
using System.Collections;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class DrawableCanvas : UIWidget
{
	public bool _blockDrawing;

	[SerializeField]
	private UIWidget _canvasArea;

	[SerializeField]
	private UITexture _canvasTexture;

	[SerializeField]
	private ListObjectPool _lines;

	[SerializeField]
	private SelectableButton _undoButton;

	[SerializeField]
	private SelectableButton _redoButton;

	[SerializeField]
	private BoxCollider _touchCollider;

	[SerializeField]
	[Tooltip("InitialZoomSize만큼 줌 된 상태에서 캔버스를 그릴 수 있는 공간과 실제 캔버스 사이즈의 차이")]
	private Vector2 _padding;

	[SerializeField]
	[Tooltip("그리기를 인식할 수 있고, 드래그하여 볼 수 있는 변두리 크기")]
	private Vector2 _extraDrawableAreaSize = new Vector2(50f, 50f);

	[SerializeField]
	private float _initialZoomSize = 1.2f;

	private Texture2D _canvas;

	private Point2? _prevPos;

	private readonly PenDrawer _penDrawer = new PenDrawer();

	private readonly BrushDrawer _brushDrawer = new BrushDrawer();

	private readonly DrawHistory _history = new DrawHistory();

	private ToolDatum _currentTool;

	private ICoroutineBinder _blockDrawingBinder;

	public BoxCollider TouchCollider => _touchCollider;

	public Color32 CurrentColor { get; set; }

	public bool IsRequiringSave { get; set; }

	public bool IsDrawing => _history.HasHistory();

	public ToolDatum CurrentTool
	{
		get
		{
			return _currentTool;
		}
		set
		{
			value.SetPreviousDrawableTool(_currentTool);
			_currentTool = value;
		}
	}

	public float CurrentZoomScale { get; private set; }

	public event Action<int, int, Color32> Clicked;

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			_history.Clear();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			UIEventListener uIEventListener = UIEventListener.Get(_canvasTexture.gameObject);
			uIEventListener.onPress = OnPressCanvas;
			uIEventListener.onDrag = OnDragCanvas;
			uIEventListener.onClick = OnClick_Canvas;
			_undoButton.Clicked = delegate
			{
				_history.Undo(_canvas);
			};
			_redoButton.Clicked = delegate
			{
				_history.Redo(_canvas);
			};
			_history.HistoryUpdated += OnHistoryUpdate;
			OnHistoryUpdate();
		}
	}

	public void Opened()
	{
		CurrentZoomScale = 1f;
		_blockDrawing = false;
		base.transform.localPosition = Vector3.zero;
	}

	public void SetCanvas(Texture2D texture)
	{
		IsRequiringSave = false;
		_canvas = texture;
		_canvasTexture.mainTexture = texture;
		_history.Clear();
		Zoom(_initialZoomSize);
		int canvasSize = GetCanvasSize();
		ResizeCanvasTexure(canvasSize);
		if (_canvas != null)
		{
			base.gameObject.SetActive(value: true);
			RepositionGrid(canvasSize);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnClick_Canvas(GameObject go)
	{
		Point2 currentPiexel = GetCurrentPiexel();
		if (this.Clicked != null)
		{
			this.Clicked(currentPiexel.x, currentPiexel.y, _canvas.GetPixel(currentPiexel.x, currentPiexel.y));
		}
	}

	private void OnDragCanvas(GameObject go, Vector2 delta)
	{
		OnPressCanvas(go, press: true);
	}

	private void OnPressCanvas(GameObject go, bool press)
	{
		if (Input.touchCount > 1 || _blockDrawing || UICamera.currentTouchID > 0 || UICamera.currentTouchID < -1)
		{
			_prevPos = null;
			return;
		}
		if (press)
		{
			DrawCurrentTouch();
			return;
		}
		_prevPos = null;
		_history.FinishSequence();
	}

	private void DrawCurrentTouch()
	{
		if (_canvas == null)
		{
			return;
		}
		DrawerBase drawerBase;
		if (CurrentTool.Tool == ToolType.Pen || CurrentTool.Tool == ToolType.Eraser)
		{
			drawerBase = _penDrawer;
		}
		else
		{
			if (CurrentTool.Tool != ToolType.Brush)
			{
				return;
			}
			drawerBase = _brushDrawer;
		}
		List<Point2> node = DrawExtension.GetNode(CurrentTool);
		Point2 contourSquareSize = DrawExtension.GetContourSquareSize(node);
		Color targetColor = CurrentTool.ChangeColorByTool(CurrentColor);
		Point2 currentPiexel = GetCurrentPiexel();
		Point2? prevPos = _prevPos;
		if (currentPiexel == prevPos)
		{
			return;
		}
		if (_prevPos.HasValue)
		{
			Point2 value = _prevPos.Value;
			Point2 point = currentPiexel - value;
			Vector2 vector = new Vector2(point.x, point.y);
			float magnitude = vector.magnitude;
			vector.Normalize();
			Vector2 vector2 = new Vector2(value.x, value.y);
			for (int i = 0; (float)i < magnitude; i++)
			{
				vector2 += vector;
				drawerBase.Draw(_canvas, (int)vector2.x, (int)vector2.y, contourSquareSize.x, targetColor, node, _history);
			}
		}
		drawerBase.Draw(_canvas, currentPiexel.x, currentPiexel.y, contourSquareSize.x, targetColor, node, _history);
		_canvas.Apply();
		_prevPos = currentPiexel;
	}

	private Point2 GetCurrentPiexel()
	{
		Vector2 vector = NGUIMath.ScreenToPixels(UICamera.currentTouch.pos, _canvasTexture.transform);
		Vector2 vector2 = _canvasTexture.pivotOffset;
		vector.x += vector2.x * (float)_canvasTexture.width;
		vector.y += vector2.y * (float)_canvasTexture.height;
		Point2 result = default(Point2);
		result.x = (int)(vector.x * (float)_canvas.width / (float)_canvasTexture.width);
		result.y = (int)(vector.y * (float)_canvas.height / (float)_canvasTexture.height);
		result.x = Mathf.Clamp(result.x, 0, _canvas.width - 1);
		result.y = Mathf.Clamp(result.y, 0, _canvas.height - 1);
		return result;
	}

	private void ResizeCanvasTexure(int size)
	{
		_canvasTexture.width = size;
		_canvasTexture.height = size;
		_touchCollider.size = _extraDrawableAreaSize + new Vector2(size, size);
	}

	private int GetCanvasSize(float ratio = -1f)
	{
		if (ratio < 0f)
		{
			ratio = CurrentZoomScale;
		}
		else
		{
			CurrentZoomScale = ratio;
		}
		return Mathf.FloorToInt(Mathf.Min((float)_canvasArea.width - _padding.x, (float)_canvasArea.height - _padding.y) * ratio / _initialZoomSize);
	}

	private void OnHistoryUpdate()
	{
		if (_history.HasHistory())
		{
			IsRequiringSave = true;
		}
		_redoButton.Disabled = !_history.CanRedo();
		_undoButton.Disabled = !_history.CanUndo();
	}

	public void FillBucket(int x, int y, Color32 targetColor)
	{
		DrawExtension.FloodFill(_canvas, x, y, targetColor, _history);
		_history.FinishSequence();
		_canvas.Apply();
	}

	public void ClearCanvas()
	{
		int num = _canvas.width;
		int num2 = _canvas.height;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				_history.Add(i, j, _canvas.GetPixel(i, j), Color.clear);
			}
		}
		_history.FinishSequence();
		_canvas.Clear();
		_canvas.Apply();
	}

	public void SetGridVisibility(bool isActive)
	{
		int num = _canvas.width;
		int i = 0;
		for (int count = _lines.Count; i < count; i++)
		{
			if (i != 0 && i != num && i != num + 1 && i != count - 1)
			{
				_lines[i].gameObject.SetActive(isActive);
			}
		}
	}

	public void RepositionGrid(int size)
	{
		int num = _canvas.width;
		float num2 = (float)size / (float)num;
		Vector2 vector = Vector2.one * num * 0.5f;
		_lines.Set((num + 1) * 2);
		int i = 0;
		for (int num3 = num + 1; i < num3; i++)
		{
			UIWidget component = _lines[i].GetComponent<UIWidget>();
			UIWidget component2 = _lines[num3 + i].GetComponent<UIWidget>();
			component.width = size;
			component2.width = size;
			float num4 = ((float)i - vector.x) * num2;
			component.transform.localPosition = Vector3.up * num4;
			component.transform.localEulerAngles = Vector3.zero;
			component2.transform.localPosition = Vector3.right * num4;
			component2.transform.localEulerAngles = Vector3.forward * 90f;
		}
		SetGridVisibility(isActive: true);
	}

	public void Zoom(float ratio)
	{
		int canvasSize = GetCanvasSize(ratio);
		ResizeCanvasTexure(canvasSize);
		RepositionGrid(canvasSize);
	}

	private void OnScreenResized()
	{
		int canvasSize = GetCanvasSize();
		RepositionGrid(canvasSize);
	}

	public void OnGestureZoomProcess(InputCommandMessage message)
	{
		Vector3 gestureVector = message.GestureVector;
		float currentZoomScale = CurrentZoomScale;
		float value = currentZoomScale * (1f + gestureVector.z);
		currentZoomScale = Mathf.Clamp(value, 1f, 3f);
		Zoom(currentZoomScale);
		Vector3 vector = MoveToInbound(base.transform.localPosition);
		base.transform.localPosition = ((!(currentZoomScale <= 1f)) ? vector : Vector3.Lerp(vector, Vector3.zero, Mathf.Clamp01((0f - gestureVector.z) * 3f)));
		this.StartCoroutine(ref _blockDrawingBinder, BlockDrawingSequence());
	}

	private IEnumerator BlockDrawingSequence()
	{
		_blockDrawing = true;
		yield return new WaitForSeconds(0.2f);
		_blockDrawing = false;
	}

	public void OnGestureMoveProcess(InputCommandMessage message)
	{
		if (!(CurrentZoomScale <= 1f))
		{
			Vector3 localPosition = MoveToInbound(base.transform.localPosition + message.GestureVector);
			base.transform.localPosition = localPosition;
			this.StartCoroutine(ref _blockDrawingBinder, BlockDrawingSequence());
		}
	}

	private Vector2 MoveToInbound(Vector2 targetVec)
	{
		float num = TouchCollider.size.x * 0.5f - (float)base.width * 0.5f;
		float num2 = TouchCollider.size.y * 0.5f - (float)base.height * 0.5f;
		targetVec.x = ((!(0f < num)) ? 0f : Mathf.Clamp(targetVec.x, 0f - num, num));
		targetVec.y = ((!(0f < num2)) ? 0f : Mathf.Clamp(targetVec.y, 0f - num2, num2));
		return targetVec;
	}
}
