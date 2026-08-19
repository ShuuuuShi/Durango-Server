using System;
using Durango.Logic.Statue;
using UnityEngine;

namespace Durango.UI;

public class VoxelPlaneEditCanvas : UIWidget
{
	[SerializeField]
	private UITexture _canvasWidget;

	[SerializeField]
	private ListObjectPool _lines;

	private Texture2D _canvas;

	private VoxelStatue _voxel;

	private Size3 _aFlag;

	private Size3 _bFlag;

	private Size3 _cFlag;

	private int _index;

	private Point2 _prevPos = -Point2.one;

	public byte Value { get; set; }

	public event Action Changed;

	protected override void Awake()
	{
		if (Application.isPlaying)
		{
			UIEventListener uIEventListener = UIEventListener.Get(_canvasWidget.gameObject);
			uIEventListener.onPress = OnPress_Canvas;
			uIEventListener.onDrag = OnDrag_Canvas;
		}
	}

	public void SetVoxel(VoxelStatue voxel, Vector3 side, int index)
	{
		float num;
		if (side.x != 0f)
		{
			_aFlag = new Size3(0, 1, 0);
			_bFlag = new Size3(0, 0, 1);
			_cFlag = new Size3(1, 0, 0);
			num = side.x;
		}
		else if (side.y != 0f)
		{
			_aFlag = new Size3(1, 0, 0);
			_bFlag = new Size3(0, 0, 1);
			_cFlag = new Size3(0, 1, 0);
			num = side.y;
		}
		else
		{
			if (side.z == 0f)
			{
				return;
			}
			_aFlag = new Size3(1, 0, 0);
			_bFlag = new Size3(0, 1, 0);
			_cFlag = new Size3(0, 0, 1);
			num = side.z;
		}
		int num2 = _cFlag.X * voxel.Size.X + _cFlag.Y * voxel.Size.Y + _cFlag.Z * voxel.Size.Z;
		if (num < 0f)
		{
			index = num2 - index - 1;
		}
		_voxel = voxel;
		_index = index;
		int num3 = _aFlag.X * voxel.Size.X + _aFlag.Y * voxel.Size.Y + _aFlag.Z * voxel.Size.Z;
		int num4 = _bFlag.X * voxel.Size.X + _bFlag.Y * voxel.Size.Y + _bFlag.Z * voxel.Size.Z;
		if (_canvas == null)
		{
			_canvas = new Texture2D(num3, num4);
			_canvas.filterMode = FilterMode.Point;
			_canvas.wrapMode = TextureWrapMode.Clamp;
			_canvasWidget.mainTexture = _canvas;
		}
		else if (_canvas.width != num3 || _canvas.height != num4)
		{
			_canvas.Resize(num3, num4);
		}
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				int x = i * _aFlag.X + j * _bFlag.X + index * _cFlag.X;
				int y = i * _aFlag.Y + j * _bFlag.Y + index * _cFlag.Y;
				int z = i * _aFlag.Z + j * _bFlag.Z + index * _cFlag.Z;
				byte voxel2 = voxel.GetVoxel(x, y, z);
				int num5 = ((!(num > 0f)) ? (index + 1) : (index - 1));
				if (voxel2 == 0 && num5 >= 0 && num5 < num2)
				{
					x = i * _aFlag.X + j * _bFlag.X + num5 * _cFlag.X;
					y = i * _aFlag.Y + j * _bFlag.Y + num5 * _cFlag.Y;
					z = i * _aFlag.Z + j * _bFlag.Z + num5 * _cFlag.Z;
					byte voxel3 = voxel.GetVoxel(x, y, z);
					Color color = GetColor(voxel3);
					if (voxel3 > 0)
					{
						color.a = 0.5f;
					}
					_canvas.SetPixel(i, j, color);
				}
				else
				{
					_canvas.SetPixel(i, j, GetColor(voxel2));
				}
			}
		}
		_canvas.Apply();
		CanvasReposition();
	}

	private Color GetColor(byte value)
	{
		if (_voxel == null)
		{
			return Color.clear;
		}
		int size = KUtility.GetSize(_voxel.Colors);
		if (value == 0)
		{
			return Color.clear;
		}
		if (value > size)
		{
			return Color.clear;
		}
		return _voxel.Colors[value - 1];
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
		if (_canvas == null)
		{
			return;
		}
		Vector2 vector = NGUIMath.ScreenToPixels(UICamera.currentTouch.pos, _canvasWidget.transform);
		Vector2 vector2 = _canvasWidget.pivotOffset;
		vector.x += vector2.x * (float)_canvasWidget.width;
		vector.y += vector2.y * (float)_canvasWidget.height;
		Point2 point = default(Point2);
		point.x = (int)(vector.x * (float)_canvas.width / (float)_canvasWidget.width);
		point.y = (int)(vector.y * (float)_canvas.height / (float)_canvasWidget.height);
		point.x = Mathf.Clamp(point.x, 0, _canvas.width - 1);
		point.y = Mathf.Clamp(point.y, 0, _canvas.height - 1);
		if (point == _prevPos)
		{
			return;
		}
		bool flag = false;
		if (_prevPos != -Point2.one)
		{
			Point2 point2 = point - _prevPos;
			Vector2 vector3 = new Vector2(point2.x, point2.y);
			float magnitude = vector3.magnitude;
			vector3.Normalize();
			Vector2 vector4 = new Vector2(_prevPos.x, _prevPos.y);
			for (int i = 0; (float)i < magnitude; i++)
			{
				vector4 += vector3;
				flag |= SetPixel((int)vector4.x, (int)vector4.y, Value);
			}
		}
		flag |= SetPixel(point.x, point.y, Value);
		_canvas.Apply();
		_prevPos = point;
		if (flag && this.Changed != null)
		{
			this.Changed();
		}
	}

	private bool SetPixel(int a, int b, byte value)
	{
		int x = a * _aFlag.X + b * _bFlag.X + _index * _cFlag.X;
		int y = a * _aFlag.Y + b * _bFlag.Y + _index * _cFlag.Y;
		int z = a * _aFlag.Z + b * _bFlag.Z + _index * _cFlag.Z;
		if (_voxel.GetVoxel(x, y, z) == value)
		{
			return false;
		}
		_voxel.SetVoxel(x, y, z, value);
		_canvas.SetPixel(a, b, GetColor(value));
		return true;
	}

	public void CanvasReposition()
	{
		UpdateAnchors();
		int num = Mathf.Min(base.width, base.height);
		_canvasWidget.width = num;
		_canvasWidget.height = num;
		if (_canvas != null)
		{
			base.gameObject.SetActive(value: true);
			int num2 = _canvas.width;
			float num3 = (float)num / (float)num2;
			Vector2 vector = Vector2.one * num2 * 0.5f;
			_lines.Set((num2 + 1) * 2);
			int i = 0;
			for (int num4 = num2 + 1; i < num4; i++)
			{
				UIWidget component = _lines[i].GetComponent<UIWidget>();
				UIWidget component2 = _lines[num4 + i].GetComponent<UIWidget>();
				component.width = num;
				component2.width = num;
				float num5 = ((float)i - vector.x) * num3;
				component.transform.localPosition = Vector3.up * num5;
				component.transform.localEulerAngles = Vector3.zero;
				component2.transform.localPosition = Vector3.right * num5;
				component2.transform.localEulerAngles = Vector3.forward * 90f;
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
