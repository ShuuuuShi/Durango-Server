using UnityEngine;

public class UIGeometry
{
	public BetterList<Vector3> verts = new BetterList<Vector3>();

	public BetterList<Vector2> uvs = new BetterList<Vector2>();

	public BetterList<Color> cols = new BetterList<Color>();

	private BetterList<Vector3> mRtpVerts = new BetterList<Vector3>();

	private Vector3 mRtpNormal;

	private Vector4 mRtpTan;

	public bool hasVertices => verts.size > 0;

	public bool hasTransformed => mRtpVerts != null && mRtpVerts.size > 0 && mRtpVerts.size == verts.size;

	public void Clear()
	{
		verts.Clear();
		uvs.Clear();
		cols.Clear();
		mRtpVerts.Clear();
	}

	public void ApplyTransform(Matrix4x4 widgetToPanel, bool generateNormals = true)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (verts.size > 0)
		{
			mRtpVerts.Clear();
			int i = 0;
			for (int size = verts.size; i < size; i++)
			{
				mRtpVerts.Add(((Matrix4x4)(ref widgetToPanel)).MultiplyPoint3x4(verts[i]));
			}
			if (generateNormals)
			{
				Vector3 val = ((Matrix4x4)(ref widgetToPanel)).MultiplyVector(Vector3.back);
				mRtpNormal = ((Vector3)(ref val)).normalized;
				Vector3 val2 = ((Matrix4x4)(ref widgetToPanel)).MultiplyVector(Vector3.right);
				Vector3 normalized = ((Vector3)(ref val2)).normalized;
				mRtpTan = new Vector4(normalized.x, normalized.y, normalized.z, -1f);
			}
		}
		else
		{
			mRtpVerts.Clear();
		}
	}

	public void WriteToBuffers(BetterList<Vector3> v, BetterList<Vector2> u, BetterList<Color> c, BetterList<Vector3> n, BetterList<Vector4> t)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (mRtpVerts == null || mRtpVerts.size <= 0)
		{
			return;
		}
		if (n == null)
		{
			for (int i = 0; i < mRtpVerts.size; i++)
			{
				v.Add(mRtpVerts.buffer[i]);
				u.Add(uvs.buffer[i]);
				c.Add(cols.buffer[i]);
			}
			return;
		}
		for (int j = 0; j < mRtpVerts.size; j++)
		{
			v.Add(mRtpVerts.buffer[j]);
			u.Add(uvs.buffer[j]);
			c.Add(cols.buffer[j]);
			n.Add(mRtpNormal);
			t.Add(mRtpTan);
		}
	}
}
