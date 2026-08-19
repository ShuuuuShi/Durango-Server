using UnityEngine;

public class DrawableCanvas : MonoBehaviour
{
	private struct UndoStruct
	{
		public Color Color;

		public Point2 Pos;
	}

	private const int UndoCount = 100;

	[SerializeField]
	private UITexture _canvasWidget;

	[SerializeField]
	private ListObjectPool _lines;

	private Texture2D _canvas;

	private UIWidget _widget;

	private Point2 _prevPos = -Point2.one;

	private UndoStruct[] _undoQueue = new UndoStruct[100];

	private int _undoStartIndex;

	private int _undoIndex;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public UITexture Canvas => _canvasWidget;

	public Color Color { get; set; }

	public int Size { get; set; }

	public bool IsDrawing { get; set; }

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_canvasWidget).gameObject);
		uIEventListener.onPress = OnPress_Canvas;
		uIEventListener.onDrag = OnDrag_Canvas;
	}

	public void SetCanvas(Texture2D texture)
	{
		_canvas = texture;
		_undoIndex = _undoStartIndex;
		_canvasWidget.mainTexture = (Texture)(object)texture;
		CanvasReposition();
	}

	private void OnPress_Canvas(GameObject go, bool press)
	{
		if (press)
		{
			DrawCurrentTouch();
		}
		else
		{
			_prevPos = -Point2.one;
		}
	}

	private void OnDrag_Canvas(GameObject go, Vector2 delta)
	{
		DrawCurrentTouch();
	}

	private void DrawCurrentTouch()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_canvas == (Object)null)
		{
			return;
		}
		Vector2 val = NGUIMath.ScreenToPixels(UICamera.currentTouch.pos, ((Component)_canvasWidget).transform);
		Vector2 pivotOffset = _canvasWidget.pivotOffset;
		val.x += pivotOffset.x * (float)_canvasWidget.width;
		val.y += pivotOffset.y * (float)_canvasWidget.height;
		Point2 point = default(Point2);
		point.x = (int)(val.x * (float)((Texture)_canvas).width / (float)_canvasWidget.width);
		point.y = (int)(val.y * (float)((Texture)_canvas).height / (float)_canvasWidget.height);
		point.x = Mathf.Clamp(point.x, 0, ((Texture)_canvas).width - 1);
		point.y = Mathf.Clamp(point.y, 0, ((Texture)_canvas).height - 1);
		if (point == _prevPos)
		{
			return;
		}
		if (_prevPos != -Point2.one)
		{
			Point2 point2 = point - _prevPos;
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector((float)point2.x, (float)point2.y);
			float magnitude = ((Vector2)(ref val2)).magnitude;
			((Vector2)(ref val2)).Normalize();
			Vector2 val3 = default(Vector2);
			((Vector2)(ref val3))._002Ector((float)_prevPos.x, (float)_prevPos.y);
			for (int i = 0; (float)i < magnitude; i++)
			{
				val3 += val2;
				SetCanvasPixel((int)val3.x, (int)val3.y, Color32.op_Implicit(Color));
			}
		}
		SetCanvasPixel(point.x, point.y, Color32.op_Implicit(Color));
		_canvas.Apply();
		_prevPos = point;
	}

	private void SetCanvasPixel(int x, int y, Color32 color)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Color32 val = Color32.op_Implicit(_canvas.GetPixel(x, y));
		if (color.a != val.a || color.r != val.r || color.g != val.g || color.b != val.b)
		{
			_canvas.SetPixel(x, y, Color32.op_Implicit(color));
			RecordUndo(Color32.op_Implicit(val), new Point2(x, y));
			IsDrawing = true;
		}
	}

	public void CanvasReposition()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		Widget.UpdateAnchors();
		int num = Mathf.Min(Widget.width, Widget.height);
		_canvasWidget.width = num;
		_canvasWidget.height = num;
		if ((Object)(object)_canvas != (Object)null)
		{
			((Component)this).gameObject.SetActive(true);
			int width = ((Texture)_canvas).width;
			float num2 = (float)num / (float)width;
			Vector2 val = Vector2.one * (float)width * 0.5f;
			_lines.Set((width + 1) * 2);
			int i = 0;
			for (int num3 = width + 1; i < num3; i++)
			{
				UIWidget component = _lines[i].GetComponent<UIWidget>();
				UIWidget component2 = _lines[num3 + i].GetComponent<UIWidget>();
				component.width = num;
				component2.width = num;
				float num4 = ((float)i - val.x) * num2;
				((Component)component).transform.localPosition = Vector3.up * num4;
				((Component)component).transform.localEulerAngles = Vector3.zero;
				((Component)component2).transform.localPosition = Vector3.right * num4;
				((Component)component2).transform.localEulerAngles = Vector3.forward * 90f;
			}
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void Undo()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_canvas == (Object)null) && _undoIndex != _undoStartIndex)
		{
			UndoStruct undoStruct = _undoQueue[_undoIndex];
			_canvas.SetPixel(undoStruct.Pos.x, undoStruct.Pos.y, undoStruct.Color);
			_canvas.Apply();
			_undoIndex--;
			if (_undoIndex < 0)
			{
				_undoIndex += 100;
			}
		}
	}

	private void RecordUndo(Color color, Point2 pos)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_undoIndex++;
		_undoIndex %= 100;
		_undoQueue[_undoIndex].Color = color;
		_undoQueue[_undoIndex].Pos = pos;
		if (_undoIndex == _undoStartIndex)
		{
			_undoStartIndex++;
			_undoStartIndex %= 100;
		}
	}
}
