using System.Collections.Generic;
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
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
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
				byte b = (byte)(255f * (1f - (time - lineSegment.BeginTime - _fadeBeginTime) / _fadeTime));
				lineSegment.Line.SetColor(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, b));
			}
		}
	}

	public void AddLineSegment()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		LineSegment lineSegment = new LineSegment();
		float num = 7500f / KSingleton<MainCamera>.Instance().CameraDistance;
		float num2 = _width * num;
		lineSegment.Line = new VectorLine("LineSegment" + _lineSegmentList.Count, lineSegment.LinePoints, num2, (LineType)0, (Joins)1);
		lineSegment.Line.material = _lineMaterial;
		lineSegment.Line.texture = _lineMaterial.mainTexture;
		lineSegment.Line.layer = OverlayCamera.Layer;
		lineSegment.Line.maxWeldDistance = float.MaxValue;
		lineSegment.BeginTime = Time.time;
		_lineSegmentList.Add(lineSegment);
	}

	public void AddLinePoint(Vector3 worldPos)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
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
