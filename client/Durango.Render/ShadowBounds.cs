using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.Render;

[ResourcePath("shadow_bounds")]
public class ShadowBounds : ResourceSingleton<ShadowBounds>
{
	[Serializable]
	public struct MeshExpandTargets
	{
		public string MeshName;

		public Vector3[] ExpandTargets;
	}

	[SerializeField]
	public List<MeshExpandTargets> Infos;

	[SerializeField]
	public Matrix4x4 CamProjMat;

	[SerializeField]
	public Matrix4x4 ShadowProjMat;

	[SerializeField]
	public Vector3 CamForward;

	private Dictionary<string, Vector3[]> _boundsDict;

	private void Init()
	{
		_boundsDict = new Dictionary<string, Vector3[]>();
		foreach (MeshExpandTargets info in ResourceSingleton<ShadowBounds>.Instance().Infos)
		{
			if (!_boundsDict.ContainsKey(info.MeshName))
			{
				Vector3[] expandTargets = info.ExpandTargets;
				_boundsDict.Add(info.MeshName, expandTargets);
			}
		}
	}

	public Vector3 GetExpandTarget(Mesh mesh, Quaternion rotation)
	{
		if (_boundsDict == null)
		{
			Init();
		}
		Vector3[] array = _boundsDict.Get(mesh.name);
		byte b = (byte)(rotation.eulerAngles.y / 90f % 4f);
		return (KUtility.GetSize(array) > b) ? array[b] : Vector3.zero;
	}
}
