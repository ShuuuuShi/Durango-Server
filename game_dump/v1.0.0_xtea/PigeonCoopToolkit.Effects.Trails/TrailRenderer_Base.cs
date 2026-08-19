using System.Collections.Generic;
using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

public abstract class TrailRenderer_Base : MonoBehaviour
{
	public PCTrailRendererData TrailData;

	public bool Emit;

	public int MaxNumberOfPoints = 50;

	protected bool _emit;

	protected PCTrail _activeTrail;

	private List<PCTrail> _fadingTrails;

	protected Transform _t;

	[SerializeField]
	private bool _flipU;

	[SerializeField]
	private bool _flipV;

	private Camera _cam;

	[ContextMenu("CLEARER")]
	public void NewClear()
	{
		if (Application.isPlaying)
		{
			ClearSystem(emitState: true);
		}
	}

	protected virtual void Awake()
	{
		_activeTrail = new PCTrail(MaxNumberOfPoints);
		_fadingTrails = new List<PCTrail>();
		_t = ((Component)this).transform;
		_emit = Emit;
		_cam = Camera.main;
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
	}

	protected virtual void LateUpdate()
	{
		CheckEmitChange();
		if (_activeTrail != null)
		{
			UpdatePoints(Time.deltaTime, _activeTrail);
			GenerateMesh(_activeTrail);
			DrawMesh(_activeTrail.Mesh, TrailData.TrailMaterial);
		}
		for (int num = _fadingTrails.Count - 1; num >= 0; num--)
		{
			if (_fadingTrails[num] == null || !_fadingTrails[num].Points.Any((PCTrailPoint a) => a.TimeActive() < TrailData.Lifetime))
			{
				if (_fadingTrails[num] != null)
				{
					_fadingTrails[num].Dispose();
				}
				_fadingTrails.RemoveAt(num);
			}
			else
			{
				UpdatePoints(Time.deltaTime, _fadingTrails[num]);
				GenerateMesh(_fadingTrails[num]);
				DrawMesh(_fadingTrails[num].Mesh, TrailData.TrailMaterial);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (_activeTrail != null)
		{
			_activeTrail.Dispose();
			_activeTrail = null;
		}
		if (_fadingTrails == null)
		{
			return;
		}
		foreach (PCTrail fadingTrail in _fadingTrails)
		{
			fadingTrail?.Dispose();
		}
		_fadingTrails.Clear();
	}

	protected virtual void OnStopEmit()
	{
	}

	protected virtual void OnStartEmit()
	{
	}

	protected virtual void Reset()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		if (TrailData == null)
		{
			TrailData = new PCTrailRendererData();
		}
		TrailData.ColorOverLife = new Gradient();
		TrailData.Lifetime = 1f;
		TrailData.SizeOverLife = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
		{
			new Keyframe(0f, 1f),
			new Keyframe(1f, 0f)
		});
		MaxNumberOfPoints = 50;
	}

	protected virtual void InitialiseNewPoint(PCTrailPoint newPoint)
	{
	}

	protected virtual void UpdatePoint(PCTrailPoint point, float deltaTime)
	{
	}

	protected void AddPoint(PCTrailPoint newPoint, Vector3 pos)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		if (_activeTrail != null)
		{
			newPoint.Position = pos;
			newPoint.PointNumber = ((_activeTrail.Points.Count != 0) ? (_activeTrail.Points[_activeTrail.Points.Count - 1].PointNumber + 1) : 0);
			InitialiseNewPoint(newPoint);
			newPoint.SetDistanceFromStart((_activeTrail.Points.Count != 0) ? (_activeTrail.Points[_activeTrail.Points.Count - 1].GetDistanceFromStart() + Vector3.Distance(_activeTrail.Points[_activeTrail.Points.Count - 1].Position, pos)) : 0f);
			if (TrailData.UseForwardOverride)
			{
				newPoint.Forward = ((!TrailData.ForwardOverrideRelative) ? ((Vector3)(ref TrailData.ForwardOverride)).normalized : _t.TransformDirection(((Vector3)(ref TrailData.ForwardOverride)).normalized));
			}
			_activeTrail.Points.Add(newPoint);
		}
	}

	private void GenerateMesh(PCTrail trail)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		trail.Mesh.Clear(false);
		Vector3 val = ((!((Object)(object)_cam != (Object)null)) ? Vector3.forward : ((Component)_cam).transform.forward);
		if (TrailData.UseForwardOverride)
		{
			val = ((Vector3)(ref TrailData.ForwardOverride)).normalized;
		}
		trail.activePointCount = NumberOfActivePoints(trail);
		if (trail.activePointCount < 2)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < trail.Points.Count; i++)
		{
			PCTrailPoint pCTrailPoint = trail.Points[i];
			float num2 = pCTrailPoint.TimeActive() / TrailData.Lifetime;
			if (!(pCTrailPoint.TimeActive() > TrailData.Lifetime))
			{
				if (TrailData.UseForwardOverride && TrailData.ForwardOverrideRelative)
				{
					val = pCTrailPoint.Forward;
				}
				Vector3 zero = Vector3.zero;
				if (i < trail.Points.Count - 1)
				{
					Vector3 val2 = trail.Points[i + 1].Position - pCTrailPoint.Position;
					Vector3 val3 = Vector3.Cross(((Vector3)(ref val2)).normalized, val);
					zero = ((Vector3)(ref val3)).normalized;
				}
				else
				{
					Vector3 val4 = pCTrailPoint.Position - trail.Points[i - 1].Position;
					Vector3 val5 = Vector3.Cross(((Vector3)(ref val4)).normalized, val);
					zero = ((Vector3)(ref val5)).normalized;
				}
				Vector3 val6 = (pCTrailPoint.Position + pCTrailPoint.Position2) * 0.5f;
				Color val7 = ((!TrailData.StretchColorToFit) ? TrailData.ColorOverLife.Evaluate(num2) : TrailData.ColorOverLife.Evaluate(1f - (float)num / (float)trail.activePointCount / 2f));
				float num3 = ((!TrailData.StretchSizeToFit) ? TrailData.SizeOverLife.Evaluate(num2) : TrailData.SizeOverLife.Evaluate(1f - (float)num / (float)trail.activePointCount / 2f));
				if (pCTrailPoint.Position2 == Vector3.zero)
				{
					ref Vector3 reference = ref trail.verticies[num];
					reference = pCTrailPoint.Position + zero * num3;
				}
				else
				{
					ref Vector3 reference2 = ref trail.verticies[num];
					reference2 = Vector3.Lerp(val6, pCTrailPoint.Position2, num3);
				}
				if (TrailData.MaterialTileLength <= 0f)
				{
					ref Vector2 reference3 = ref trail.uvs[num];
					reference3 = new Vector2((float)num / (float)trail.activePointCount / 2f, 0f);
				}
				else
				{
					ref Vector2 reference4 = ref trail.uvs[num];
					reference4 = new Vector2(pCTrailPoint.GetDistanceFromStart() / TrailData.MaterialTileLength, 0f);
				}
				if (_flipU)
				{
					trail.uvs[num].x = 1f - trail.uvs[num].x;
				}
				if (_flipV)
				{
					trail.uvs[num].y = 1f - trail.uvs[num].y;
				}
				trail.normals[num] = val;
				trail.colors[num] = val7;
				num++;
				if (pCTrailPoint.Position2 == Vector3.zero)
				{
					ref Vector3 reference5 = ref trail.verticies[num];
					reference5 = pCTrailPoint.Position - zero * num3;
				}
				else
				{
					ref Vector3 reference6 = ref trail.verticies[num];
					reference6 = Vector3.Lerp(val6, pCTrailPoint.Position, num3);
				}
				if (TrailData.MaterialTileLength <= 0f)
				{
					ref Vector2 reference7 = ref trail.uvs[num];
					reference7 = new Vector2((float)num / (float)trail.activePointCount / 2f, 1f);
				}
				else
				{
					ref Vector2 reference8 = ref trail.uvs[num];
					reference8 = new Vector2(pCTrailPoint.GetDistanceFromStart() / TrailData.MaterialTileLength, 1f);
				}
				if (_flipU)
				{
					trail.uvs[num].x = 1f - trail.uvs[num].x;
				}
				if (_flipV)
				{
					trail.uvs[num].y = 1f - trail.uvs[num].y;
				}
				trail.normals[num] = val;
				trail.colors[num] = val7;
				num++;
			}
		}
		Vector2 val8 = Vector2.op_Implicit(trail.verticies[num - 1]);
		for (int j = num; j < trail.verticies.Length; j++)
		{
			ref Vector3 reference9 = ref trail.verticies[j];
			reference9 = Vector2.op_Implicit(val8);
		}
		int num4 = 0;
		for (int k = 0; k < 2 * (trail.activePointCount - 1); k++)
		{
			if (k % 2 == 0)
			{
				trail.indicies[num4] = k;
				num4++;
				trail.indicies[num4] = k + 1;
				num4++;
				trail.indicies[num4] = k + 2;
			}
			else
			{
				trail.indicies[num4] = k + 2;
				num4++;
				trail.indicies[num4] = k + 1;
				num4++;
				trail.indicies[num4] = k;
			}
			num4++;
		}
		int num5 = trail.indicies[num4 - 1];
		for (int l = num4; l < trail.indicies.Length; l++)
		{
			trail.indicies[l] = num5;
		}
		trail.Mesh.vertices = trail.verticies;
		trail.Mesh.SetIndices(trail.indicies, (MeshTopology)0, 0);
		trail.Mesh.uv = trail.uvs;
		trail.Mesh.normals = trail.normals;
		trail.Mesh.colors = trail.colors;
	}

	private void DrawMesh(Mesh trailMesh, Material trailMaterial)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Graphics.DrawMesh(trailMesh, Matrix4x4.identity, trailMaterial, ((Component)this).gameObject.layer);
	}

	private void UpdatePoints(float deltaTime, PCTrail line)
	{
		for (int i = 0; i < line.Points.Count; i++)
		{
			line.Points[i].Update(deltaTime);
			UpdatePoint(line.Points[i], deltaTime);
		}
	}

	private void CheckEmitChange()
	{
		if (_emit != Emit)
		{
			_emit = Emit;
			if (_emit)
			{
				OnStartEmit();
				_activeTrail = new PCTrail(MaxNumberOfPoints);
			}
			else
			{
				OnStopEmit();
				_fadingTrails.Add(_activeTrail);
				_activeTrail = null;
			}
		}
	}

	private int NumberOfActivePoints(PCTrail line)
	{
		int num = 0;
		for (int i = 0; i < line.Points.Count; i++)
		{
			if (line.Points[i].TimeActive() < TrailData.Lifetime)
			{
				num++;
			}
		}
		return num;
	}

	public void ClearSystem(bool emitState)
	{
		if (_activeTrail != null)
		{
			_activeTrail.Dispose();
			_activeTrail = null;
		}
		if (_fadingTrails != null)
		{
			foreach (PCTrail fadingTrail in _fadingTrails)
			{
				fadingTrail?.Dispose();
			}
			_fadingTrails.Clear();
		}
		Emit = emitState;
		_emit = !emitState;
		CheckEmitChange();
	}

	public int NumSegments()
	{
		int num = 0;
		if (_activeTrail != null && NumberOfActivePoints(_activeTrail) != 0)
		{
			num++;
		}
		return num + _fadingTrails.Count;
	}
}
