using UnityEngine;

public class UIGeometry
{
	public class Arguments
	{
		public BetterList<Vector3> verts = new BetterList<Vector3>();

		public BetterList<Vector2> uvs = new BetterList<Vector2>();

		public BetterList<Color> cols = new BetterList<Color>();

		public UIDrawCall.ExtentionUvs extentionUvs = new UIDrawCall.ExtentionUvs();
	}

	public Arguments arguments = new Arguments();

	private BetterList<Vector3> mRtpVerts = new BetterList<Vector3>();

	private Vector3 mRtpNormal;

	private Vector4 mRtpTan;

	public BetterList<Vector3> verts => arguments.verts;

	public BetterList<Vector2> uvs => arguments.uvs;

	public BetterList<Color> cols => arguments.cols;

	public UIDrawCall.ExtentionUvs extentionUvs => arguments.extentionUvs;

	public bool hasVertices => verts.size > 0;

	public bool hasTransformed
	{
		get
		{
			if (mRtpVerts != null && mRtpVerts.size > 0)
			{
				return mRtpVerts.size == verts.size;
			}
			return false;
		}
	}

	public void Clear()
	{
		verts.Clear();
		uvs.Clear();
		cols.Clear();
		mRtpVerts.Clear();
		extentionUvs.Clear();
	}

	public void ApplyTransform(Matrix4x4 widgetToPanel, bool generateNormals = true)
	{
		if (verts.size > 0)
		{
			mRtpVerts.Clear();
			int size = verts.size;
			mRtpVerts.EnsureCapacity(size);
			Vector3[] buffer = verts.buffer;
			Vector3[] buffer2 = mRtpVerts.buffer;
			for (int i = 0; i < size; i++)
			{
				ref Vector3 reference = ref buffer2[i];
				reference = widgetToPanel.MultiplyPoint3x4(buffer[i]);
			}
			mRtpVerts.size = size;
			if (generateNormals)
			{
				mRtpNormal = widgetToPanel.MultiplyVector(Vector3.back).normalized;
				Vector3 normalized = widgetToPanel.MultiplyVector(Vector3.right).normalized;
				mRtpTan = new Vector4(normalized.x, normalized.y, normalized.z, -1f);
			}
		}
		else
		{
			mRtpVerts.Clear();
		}
	}

	public void WriteToBuffers(BetterList<Vector3> v, BetterList<Vector2> u, BetterList<Color> c, BetterList<Vector3> n, BetterList<Vector4> t, UIDrawCall.ExtentionUvs extention)
	{
		if (mRtpVerts == null || mRtpVerts.size <= 0)
		{
			return;
		}
		v.AddRange(mRtpVerts.buffer, mRtpVerts.size);
		u.AddRange(uvs.buffer, mRtpVerts.size);
		c.AddRange(cols.buffer, mRtpVerts.size);
		if (n != null)
		{
			for (int i = 0; i < mRtpVerts.size; i++)
			{
				n.Add(mRtpNormal);
				t.Add(mRtpTan);
			}
		}
		extention.Fill(extentionUvs);
	}
}
