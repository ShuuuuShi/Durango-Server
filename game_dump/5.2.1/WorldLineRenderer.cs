using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;
using Vectrosity;

public class WorldLineRenderer : MonoBehaviour
{
	private class LineSegment
	{
		public readonly List<Vector3> LinePoints = new List<Vector3>();

		public VectorLine Line;

		public float BeginTime;
	}

	private readonly List<LineSegment> _lineSegmentList = new List<LineSegment>();

	[SerializeField]
	private float _fadeTime = 2f;

	[SerializeField]
	private float _fadeBeginTime = 10f;

	[SerializeField]
	private int _maxPointCount = 100;

	[SerializeField]
	private float _width = 4f;

	[SerializeField]
	private Material _lineMaterial;

	private Vector3 _prevAddedLineWorldPos;

	private void Update()
	{
		float time = Time.time;
		for (int num = _lineSegmentList.Count - 1; num >= 0; num--)
		{
			LineSegment lineSegment = _lineSegmentList[num];
			if (lineSegment.BeginTime + _fadeBeginTime + _fadeTime < time)
			{
				VectorLine.Destroy(ref lineSegment.Line);
				_lineSegmentList.RemoveAt(num);
			}
			else if (lineSegment.BeginTime + _fadeBeginTime < time)
			{
				byte a = (byte)(255f * (1f - (time - lineSegment.BeginTime - _fadeBeginTime) / _fadeTime));
				lineSegment.Line.SetColor(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, a));
			}
		}
	}

	public void AddLineSegment()
	{
		LineSegment lineSegment = new LineSegment();
		float num = 7500f / Singleton<MainCamera>.Instance().CameraDistance;
		float width = _width * num;
		lineSegment.Line = new VectorLine("LineSegment" + _lineSegmentList.Count, lineSegment.LinePoints, width, LineType.Continuous, Joins.Weld);
		lineSegment.Line.material = _lineMaterial;
		lineSegment.Line.texture = _lineMaterial.mainTexture;
		lineSegment.Line.layer = OverlayCamera.Layer;
		lineSegment.Line.maxWeldDistance = float.MaxValue;
		lineSegment.BeginTime = Time.time;
		_lineSegmentList.Add(lineSegment);
	}

	public void AddLinePoint(Vector3 worldPos)
	{
		if (_lineSegmentList.Count == 0)
		{
			AddLineSegment();
		}
		LineSegment lineSegment = _lineSegmentList[_lineSegmentList.Count - 1];
		if (lineSegment.LinePoints.Count >= _maxPointCount - 1)
		{
			AddLineSegment();
			AddLinePoint(_prevAddedLineWorldPos);
			AddLinePoint(worldPos);
			return;
		}
		lineSegment.LinePoints.Add(worldPos);
		if (lineSegment.LinePoints.Count > 1)
		{
			lineSegment.Line.Draw3D();
		}
		_prevAddedLineWorldPos = worldPos;
	}

	public bool IsDrawing()
	{
		return _lineSegmentList.Count > 0;
	}
}
