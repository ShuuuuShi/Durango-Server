using System;
using System.Collections.Generic;
using UnityEngine;

public class RGBMaskPanel : MonoBehaviour
{
	private List<Vector3> _uv2s = new List<Vector3>();

	private List<Vector3> _uv3s = new List<Vector3>();

	private List<Vector3> _uv4s = new List<Vector3>();

	private bool _updateDrawCalls;

	private void Start()
	{
		UIPanel component = ((Component)this).GetComponent<UIPanel>();
		component.onGeometryUpdated = (UIPanel.OnGeometryUpdated)Delegate.Combine(component.onGeometryUpdated, new UIPanel.OnGeometryUpdated(OnGeometryUpdated));
		component.onDrawcallListUpdated = (Action)Delegate.Combine(component.onDrawcallListUpdated, new Action(OnDrawcallListUpdated));
	}

	private void OnEnable()
	{
		_updateDrawCalls = true;
	}

	private void LateUpdate()
	{
		if (_updateDrawCalls)
		{
			UpdateDrawCalls();
		}
	}

	private void OnGeometryUpdated()
	{
		_updateDrawCalls = true;
	}

	private void OnDrawcallListUpdated()
	{
		_updateDrawCalls = true;
	}

	private void UpdateDrawCalls()
	{
		_updateDrawCalls = false;
		UIPanel component = ((Component)this).GetComponent<UIPanel>();
		List<UIWidget> widgets = component.widgets;
		for (int i = 0; i < component.drawCalls.Count; i++)
		{
			UpdateDrawCall(component.drawCalls[i], widgets);
		}
	}

	private void UpdateDrawCall(UIDrawCall dc, IList<UIWidget> widgets)
	{
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)dc == (Object)null || !((Object)dc.shader).name.Contains("RGBMask"))
		{
			return;
		}
		Mesh sharedMesh = ((Component)dc).GetComponent<MeshFilter>().sharedMesh;
		sharedMesh.GetUVs(1, _uv2s);
		sharedMesh.GetUVs(2, _uv3s);
		sharedMesh.GetUVs(3, _uv4s);
		if (_uv2s.Count < sharedMesh.vertexCount)
		{
			_uv2s.AddRange((IEnumerable<Vector3>)(object)new Vector3[sharedMesh.vertexCount - _uv2s.Count]);
		}
		if (_uv3s.Count < sharedMesh.vertexCount)
		{
			_uv3s.AddRange((IEnumerable<Vector3>)(object)new Vector3[sharedMesh.vertexCount - _uv3s.Count]);
		}
		if (_uv4s.Count < sharedMesh.vertexCount)
		{
			_uv4s.AddRange((IEnumerable<Vector3>)(object)new Vector3[sharedMesh.vertexCount - _uv4s.Count]);
		}
		int num = 0;
		int i = 0;
		for (int count = widgets.Count; i < count; i++)
		{
			UIWidget uIWidget = widgets[i];
			if (!((Object)(object)uIWidget.drawCall != (Object)(object)dc) && uIWidget.isVisible && uIWidget.hasVertices)
			{
				RGBMask component = ((Component)uIWidget).GetComponent<RGBMask>();
				int j = 0;
				for (int size = uIWidget.geometry.verts.size; j < size; j++)
				{
					List<Vector3> uv2s = _uv2s;
					int index = num;
					Color r = component.R;
					float num2 = ((Color)(ref r))[0];
					Color r2 = component.R;
					float num3 = ((Color)(ref r2))[1];
					Color r3 = component.R;
					uv2s[index] = new Vector3(num2, num3, ((Color)(ref r3))[2]);
					List<Vector3> uv3s = _uv3s;
					int index2 = num;
					Color g = component.G;
					float num4 = ((Color)(ref g))[0];
					Color g2 = component.G;
					float num5 = ((Color)(ref g2))[1];
					Color g3 = component.G;
					uv3s[index2] = new Vector3(num4, num5, ((Color)(ref g3))[2]);
					List<Vector3> uv4s = _uv4s;
					int index3 = num;
					Color b = component.B;
					float num6 = ((Color)(ref b))[0];
					Color b2 = component.B;
					float num7 = ((Color)(ref b2))[1];
					Color b3 = component.B;
					uv4s[index3] = new Vector3(num6, num7, ((Color)(ref b3))[2]);
					num++;
				}
			}
		}
		sharedMesh.SetUVs(1, _uv2s);
		sharedMesh.SetUVs(2, _uv3s);
		sharedMesh.SetUVs(3, _uv4s);
		_uv2s.Clear();
		_uv3s.Clear();
		_uv4s.Clear();
	}
}
