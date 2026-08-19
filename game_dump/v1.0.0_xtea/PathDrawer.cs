using System;
using System.Collections.Generic;
using Messages;
using UnityEngine;

public class PathDrawer : KSingleton<PathDrawer>
{
	private class PathStruct
	{
		public Action<PathStruct> OnUpdatePath;

		public readonly PathMovable path;

		public readonly List<PathDrawLine> line;

		public readonly List<PathPoint> organizePath;

		public PathStruct(PathMovable path)
		{
			this.path = path;
			line = new List<PathDrawLine>();
			organizePath = new List<PathPoint>();
			this.path.MovementProcessed += PathOnMovementProcessed;
			PathOrganizing();
		}

		private void PathOrganizing()
		{
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
			double at = bufferedServerTime - 3.0;
			ForgetPast(at);
			int count = path.PathBuffer.Count;
			if (count <= 0)
			{
				return;
			}
			int num = -1;
			for (int i = 0; i < count; i++)
			{
				if (path.PathBuffer[i].Time > bufferedServerTime)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				return;
			}
			ChangeFuture(bufferedServerTime);
			Vector3 positionAt = path.GetPositionAt(bufferedServerTime);
			organizePath.Add(new PathPoint(positionAt, bufferedServerTime));
			PathMovable.LocationClient locationClient = path.PathBuffer[num];
			organizePath.Add(new PathPoint(locationClient.ClientPosition, locationClient.Time));
			for (int j = num + 1; j < count - 1; j++)
			{
				PathMovable.LocationClient locationClient2 = path.PathBuffer[j];
				if (!(locationClient2.Direction == Vector2.zero))
				{
					float num2 = Mathf.DeltaAngle(locationClient.Yaw, locationClient2.Yaw);
					if (Mathf.Abs(num2) > 10f)
					{
						organizePath.Add(new PathPoint(locationClient2.ClientPosition, locationClient2.Time));
						locationClient = locationClient2;
					}
				}
			}
			if (count - num > 1)
			{
				PathMovable.LocationClient locationClient3 = path.PathBuffer[count - 1];
				organizePath.Add(new PathPoint(locationClient3.ClientPosition, locationClient3.Time));
			}
		}

		private void ForgetPast(double at)
		{
			int count = organizePath.Count;
			int num = count;
			for (int i = 0; i < count; i++)
			{
				if (organizePath[i].time > at)
				{
					num = i - 1;
					break;
				}
			}
			if (num > 0)
			{
				organizePath.RemoveRange(0, num);
			}
		}

		private void ChangeFuture(double at)
		{
			int num = 0;
			for (int num2 = organizePath.Count - 1; num2 >= 0; num2--)
			{
				if (organizePath[num2].time < at)
				{
					num = num2;
					break;
				}
			}
			if (organizePath.Count > num)
			{
				organizePath.RemoveRange(num, organizePath.Count - num);
			}
		}

		private void PathOnMovementProcessed(Movement movement)
		{
			PathOrganizing();
			if (OnUpdatePath != null)
			{
				OnUpdatePath(this);
			}
		}

		public void RemoveCallback()
		{
			path.MovementProcessed -= PathOnMovementProcessed;
		}
	}

	public struct PathPoint
	{
		public Vector3 pos;

		public double time;

		public PathPoint(Vector3 pos, double time)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			this.pos = pos;
			this.time = time;
		}
	}

	private const float VisiblePastPathTime = 3f;

	private const float PathOrganizeLevel = 10f;

	[SerializeField]
	private UIPanel _parentPanel;

	[SerializeField]
	private PathDrawLine _pathLine;

	private Stack<PathDrawLine> _pathPool;

	private List<PathStruct> _drawingPath;

	protected override void OnAwake()
	{
		_pathPool = new Stack<PathDrawLine>();
		_drawingPath = new List<PathStruct>();
	}

	private PathDrawLine PopPathLine()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (_pathPool.Count == 0)
		{
			PathDrawLine component = ((Component)_parentPanel).gameObject.AddChild(((Component)_pathLine).gameObject).GetComponent<PathDrawLine>();
			((Component)component).transform.localScale = ((Component)_pathLine).transform.localScale;
			return component;
		}
		return _pathPool.Pop();
	}

	private void PushPathLine(PathDrawLine line)
	{
		_pathPool.Push(line);
	}

	private void DrawPath(PathStruct pathStruct)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		int count = pathStruct.organizePath.Count;
		if (count == 0)
		{
			return;
		}
		double num = Connections.Frontend.GetBufferedServerTime() - 3.0;
		PathPoint pathPoint = pathStruct.organizePath[0];
		for (int i = 1; i < count; i++)
		{
			PathPoint pathPoint2 = pathStruct.organizePath[i];
			PathDrawLine pathLine = GetPathLine(pathStruct, i - 1);
			((Component)pathLine).gameObject.SetActive(true);
			pathLine.Position = pathPoint2.pos;
			Vector3 val = pathPoint.pos - pathPoint2.pos;
			pathLine.Length = (int)(((Vector3)(ref val)).magnitude / ((Component)pathLine).transform.localScale.x);
			float num2 = (0f - Mathf.Atan2(val.z, val.x)) * 57.29578f;
			pathLine.Angle = new Vector3(90f, num2);
			float duration;
			float delay;
			if (pathPoint.time < num)
			{
				float num3 = (float)((pathPoint2.time - num) / (pathPoint2.time - pathPoint.time));
				pathLine.Length = (int)((float)pathLine.Length * num3);
				duration = (float)(pathPoint2.time - num);
				delay = 0f;
			}
			else
			{
				duration = (float)(pathPoint2.time - pathPoint.time);
				delay = (float)(pathPoint.time - num);
			}
			pathLine.TweenLength(delay, duration);
			pathPoint = pathPoint2;
		}
		for (int j = count - 1; j < pathStruct.line.Count; j++)
		{
			((Component)pathStruct.line[j]).gameObject.SetActive(false);
		}
	}

	private PathDrawLine GetPathLine(PathStruct pathStruct, int index)
	{
		PathDrawLine pathDrawLine = null;
		int count = pathStruct.line.Count;
		if (count == index)
		{
			pathDrawLine = ((Component)PopPathLine()).GetComponent<PathDrawLine>();
			pathStruct.line.Add(pathDrawLine);
		}
		else
		{
			pathDrawLine = pathStruct.line[index];
		}
		return pathDrawLine;
	}

	private int IndexOf(PathMovable path)
	{
		int result = -1;
		int i = 0;
		for (int count = _drawingPath.Count; i < count; i++)
		{
			if (_drawingPath[i].path == path)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public void DrawPath(PathMovable pathMove)
	{
		int num = IndexOf(pathMove);
		if (num == -1)
		{
			PathStruct pathStruct = new PathStruct(pathMove);
			_drawingPath.Add(pathStruct);
			pathStruct.OnUpdatePath = DrawPath;
			DrawPath(pathStruct);
		}
		else
		{
			DrawPath(_drawingPath[num]);
		}
	}

	public void StopDraw(PathMovable pathMove)
	{
		int num = IndexOf(pathMove);
		if (num != -1)
		{
			_drawingPath[num].RemoveCallback();
			int i = 0;
			for (int count = _drawingPath[num].line.Count; i < count; i++)
			{
				PushPathLine(_drawingPath[num].line[i]);
				((Component)_drawingPath[num].line[i]).gameObject.SetActive(false);
			}
			_drawingPath[num].line.Clear();
			_drawingPath.RemoveAt(num);
		}
	}
}
