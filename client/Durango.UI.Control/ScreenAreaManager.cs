using System;
using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class ScreenAreaManager : Singleton<ScreenAreaManager>
{
	private struct CurvePoint
	{
		public Vector2 Pos;

		public Vector2 Vector;

		public float Power;

		public float Angle;
	}

	private struct Point
	{
		public Vector2 Pos;

		public bool Base;

		public int Dir;
	}

	private readonly List<ScreenAreaMask> _masks = new List<ScreenAreaMask>();

	private bool _isDirty;

	private bool _isDirtyCurves;

	private readonly List<Vector3> _results = new List<Vector3>();

	private readonly List<Point> _points = new List<Point>();

	private List<CurvePoint> _curvePoints = new List<CurvePoint>();

	private List<Maths.BezierCurve4> _curves = new List<Maths.BezierCurve4>();

	public void Add([NotNull] ScreenAreaMask mask)
	{
		if (!_masks.Contains(mask))
		{
			_masks.Add(mask);
			SetDirty();
		}
	}

	public void Remove([NotNull] ScreenAreaMask mask)
	{
		if (_masks.Remove(mask))
		{
			SetDirty();
		}
	}

	public List<Vector3> GetPoints()
	{
		Refresh();
		return _results;
	}

	public Vector2 GetBorder(float angle)
	{
		RefreshCurves();
		angle = Mathf.Repeat(angle, 360f);
		Maths.BezierCurve4? bezierCurve = null;
		CurvePoint curvePoint = default(CurvePoint);
		CurvePoint curvePoint2 = default(CurvePoint);
		for (int i = 0; i < _curvePoints.Count; i++)
		{
			curvePoint = _curvePoints[i];
			curvePoint2 = _curvePoints[(i + 1) % _curvePoints.Count];
			if (curvePoint.Angle < curvePoint2.Angle)
			{
				if (curvePoint.Angle <= angle && angle < curvePoint2.Angle)
				{
					bezierCurve = _curves[i];
					break;
				}
			}
			else if (curvePoint.Angle <= angle || angle < curvePoint2.Angle)
			{
				bezierCurve = _curves[i];
				break;
			}
		}
		if (!bezierCurve.HasValue)
		{
			return Vector2.zero;
		}
		float num = angle - curvePoint.Angle;
		if (num < 0f)
		{
			num += 360f;
		}
		float num2 = curvePoint2.Angle - curvePoint.Angle;
		if (num2 <= 0f)
		{
			num2 += 360f;
		}
		float r = num / num2;
		return bezierCurve.Value.Get(r);
	}

	public void SetDirty()
	{
		_isDirty = true;
		_isDirtyCurves = true;
	}

	private void Refresh()
	{
		if (_isDirty)
		{
			_isDirty = false;
			Vector2 vector = new Vector2(UIManager.ScreenWidth, UIManager.ScreenHeight);
			Rect safeArea = UIManager.SafeArea;
			Rect parent = new Rect(new Vector2(vector.x * safeArea.x, vector.y * safeArea.y) - vector * 0.5f, new Vector2(vector.x * safeArea.width, vector.y * safeArea.height));
			CalcArea(parent);
		}
	}

	private void RefreshCurves()
	{
		if (_isDirtyCurves)
		{
			_isDirtyCurves = false;
			_curvePoints.Clear();
			_curves.Clear();
			List<Vector3> points = GetPoints();
			for (int i = 0; i < points.Count; i++)
			{
				Vector2 vector = points[i];
				Vector2 vector2 = points[(i + 1) % points.Count];
				Vector2 pos = Vector2.Lerp(vector, vector2, 0.5f);
				Vector2 vector3 = vector2 - vector;
				float magnitude = vector3.magnitude;
				vector3 /= magnitude;
				magnitude *= 0.5f;
				CurvePoint curvePoint = default(CurvePoint);
				curvePoint.Pos = pos;
				curvePoint.Vector = vector3;
				curvePoint.Power = magnitude;
				curvePoint.Angle = Mathf.Repeat(Mathf.Atan2(pos.y, pos.x) * 57.29578f, 360f);
				CurvePoint item = curvePoint;
				_curvePoints.Add(item);
			}
			for (int j = 0; j < _curvePoints.Count; j++)
			{
				CurvePoint curvePoint2 = _curvePoints[j];
				CurvePoint curvePoint3 = _curvePoints[(j + 1) % _curvePoints.Count];
				float num = Mathf.Min(curvePoint2.Power, curvePoint3.Power);
				Maths.BezierCurve4 bezierCurve = default(Maths.BezierCurve4);
				bezierCurve.P1 = curvePoint2.Pos;
				bezierCurve.P2 = curvePoint2.Pos + curvePoint2.Vector * num;
				bezierCurve.P3 = curvePoint3.Pos - curvePoint3.Vector * num;
				bezierCurve.P4 = curvePoint3.Pos;
				Maths.BezierCurve4 item2 = bezierCurve;
				_curves.Add(item2);
			}
		}
	}

	private void CalcArea(Rect parent)
	{
		_points.Clear();
		Transform transform = Singleton<UIManager>.Instance().UIRoot.transform;
		for (int i = 0; i < _masks.Count; i++)
		{
			ScreenAreaMask screenAreaMask = _masks[i];
			if (screenAreaMask == null)
			{
				_masks.RemoveAt(i);
				i--;
			}
			else
			{
				if (!screenAreaMask.IsVisible)
				{
					continue;
				}
				Vector3[] worldCorners = screenAreaMask.Widget.worldCorners;
				for (int j = 0; j < worldCorners.Length; j++)
				{
					ref Vector3 reference = ref worldCorners[j];
					reference = transform.InverseTransformPoint(worldCorners[j]);
				}
				for (int k = 0; k < worldCorners.Length; k++)
				{
					bool flag = false;
					Vector3 vector = worldCorners[k];
					int quadrant = GetQuadrant(vector);
					switch (k)
					{
					case 0:
						switch (quadrant)
						{
						case 0:
							_points.Add(new Point
							{
								Pos = vector,
								Dir = 0
							});
							flag = true;
							break;
						case 1:
							if (GetQuadrant(worldCorners[3]) != 1)
							{
								if (vector.y < parent.yMax)
								{
									parent.yMax = vector.y;
								}
								flag = true;
							}
							break;
						case 3:
							if (GetQuadrant(worldCorners[1]) != 3)
							{
								if (vector.x < parent.xMax)
								{
									parent.xMax = vector.x;
								}
								flag = true;
							}
							break;
						}
						break;
					case 1:
						switch (quadrant)
						{
						case 3:
							_points.Add(new Point
							{
								Pos = vector,
								Dir = 3
							});
							flag = true;
							break;
						case 0:
							if (GetQuadrant(worldCorners[0]) != 0)
							{
								if (vector.x < parent.xMax)
								{
									parent.xMax = vector.x;
								}
								flag = true;
							}
							break;
						case 2:
							if (GetQuadrant(worldCorners[2]) != 2)
							{
								if (vector.y > parent.yMin)
								{
									parent.yMin = vector.y;
								}
								flag = true;
							}
							break;
						}
						break;
					case 2:
						switch (quadrant)
						{
						case 2:
							_points.Add(new Point
							{
								Pos = vector,
								Dir = 2
							});
							flag = true;
							break;
						case 1:
							if (GetQuadrant(worldCorners[3]) != 1)
							{
								if (vector.x > parent.xMin)
								{
									parent.xMin = vector.x;
								}
								flag = true;
							}
							break;
						case 3:
							if (GetQuadrant(worldCorners[1]) != 3)
							{
								if (vector.y > parent.yMin)
								{
									parent.yMin = vector.y;
								}
								flag = true;
							}
							break;
						}
						break;
					case 3:
						switch (quadrant)
						{
						case 1:
							_points.Add(new Point
							{
								Pos = vector,
								Dir = 1
							});
							flag = true;
							break;
						case 2:
							if (GetQuadrant(worldCorners[2]) != 2)
							{
								if (vector.x > parent.xMin)
								{
									parent.xMin = vector.x;
								}
								flag = true;
							}
							break;
						case 0:
							if (GetQuadrant(worldCorners[0]) != 0)
							{
								if (vector.y > parent.yMax)
								{
									parent.yMax = vector.y;
								}
								flag = true;
							}
							break;
						}
						break;
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		_points.Add(new Point
		{
			Base = true,
			Pos = new Vector2(parent.xMin, parent.yMin)
		});
		_points.Add(new Point
		{
			Base = true,
			Pos = new Vector2(parent.xMin, parent.yMax)
		});
		_points.Add(new Point
		{
			Base = true,
			Pos = new Vector2(parent.xMax, parent.yMax)
		});
		_points.Add(new Point
		{
			Base = true,
			Pos = new Vector2(parent.xMax, parent.yMin)
		});
		Rect rect = new Rect(parent);
		rect.xMin -= 1f;
		rect.yMin -= 1f;
		rect.xMax += 1f;
		rect.yMax += 1f;
		Rect rect2 = new Rect(parent);
		rect2.xMin += 1f;
		rect2.yMin += 1f;
		rect2.xMax -= 1f;
		rect2.yMax -= 1f;
		for (int l = 0; l < _points.Count; l++)
		{
			if (_points[l].Base)
			{
				continue;
			}
			Point point = _points[l];
			Rect rect3 = default(Rect);
			switch (_points[l].Dir)
			{
			case 0:
				rect3 = Rect.MinMaxRect(point.Pos.x, point.Pos.y, rect.xMax, rect.yMax);
				break;
			case 1:
				rect3 = Rect.MinMaxRect(rect.xMin, point.Pos.y, point.Pos.x, rect.yMax);
				break;
			case 2:
				rect3 = Rect.MinMaxRect(rect.xMin, rect.yMin, point.Pos.x, point.Pos.y);
				break;
			case 3:
				rect3 = Rect.MinMaxRect(point.Pos.x, rect.yMin, rect.xMax, point.Pos.y);
				break;
			}
			for (int m = 0; m < _points.Count; m++)
			{
				Vector2 pos = _points[m].Pos;
				if ((!_points[m].Base && !rect2.Contains(pos)) || (l != m && rect3.Contains(pos)))
				{
					_points.RemoveAt(m);
					if (l >= m)
					{
						l--;
					}
					m--;
				}
			}
		}
		_points.Sort(ComparisonPoint);
		_results.Clear();
		for (int n = 0; n < _points.Count; n++)
		{
			Point point2 = _points[(n - 1 >= 0) ? (n - 1) : (n - 1 + _points.Count)];
			Point point3 = _points[(n + 1) % _points.Count];
			Point point4 = _points[n];
			if (point4.Base)
			{
				_results.Add(point4.Pos);
				continue;
			}
			Vector2 zero = Vector2.zero;
			bool flag2 = !point2.Base && point2.Dir == point4.Dir;
			switch (point4.Dir)
			{
			case 0:
				zero.x = ((!flag2) ? parent.xMax : point2.Pos.x);
				zero.y = point4.Pos.y;
				break;
			case 1:
				zero.x = point4.Pos.x;
				zero.y = ((!flag2) ? parent.yMax : point2.Pos.y);
				break;
			case 2:
				zero.x = ((!flag2) ? parent.xMin : point2.Pos.x);
				zero.y = point4.Pos.y;
				break;
			case 3:
				zero.x = point4.Pos.x;
				zero.y = ((!flag2) ? parent.yMin : point2.Pos.y);
				break;
			}
			_results.Add(zero);
			_results.Add(point4.Pos);
			if (point3.Base || point3.Dir != point4.Dir)
			{
				zero = Vector2.zero;
				switch (point4.Dir)
				{
				case 0:
					zero.x = point4.Pos.x;
					zero.y = parent.yMax;
					break;
				case 1:
					zero.x = parent.xMin;
					zero.y = point4.Pos.y;
					break;
				case 2:
					zero.x = point4.Pos.x;
					zero.y = parent.yMin;
					break;
				case 3:
					zero.x = parent.xMax;
					zero.y = point4.Pos.y;
					break;
				}
				_results.Add(zero);
			}
		}
	}

	private int GetQuadrant(Vector2 v)
	{
		if (v.x > 0f)
		{
			return (!(v.y > 0f)) ? 3 : 0;
		}
		return (v.y > 0f) ? 1 : 2;
	}

	private int ComparisonPoint(Point p1, Point p2)
	{
		Vector2 pos = p1.Pos;
		Vector2 pos2 = p2.Pos;
		float num = Mathf.Repeat(Mathf.Atan2(pos.y, pos.x), (float)Math.PI * 2f);
		float num2 = Mathf.Repeat(Mathf.Atan2(pos2.y, pos2.x), (float)Math.PI * 2f);
		return (num != num2) ? ((!(num2 > num)) ? 1 : (-1)) : 0;
	}
}
