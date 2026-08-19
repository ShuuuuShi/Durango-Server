using UnityEngine;

public class UITextureMovie : UITexture
{
	private bool _refreshed;

	private void LateUpdate()
	{
		if (!_refreshed && (Object)(object)material.GetTexture("_YTex") != (Object)null)
		{
			UIPanel componentInParent = ((Component)this).GetComponentInParent<UIPanel>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				componentInParent.Refresh();
				_refreshed = true;
			}
		}
	}

	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Rect outer = default(Rect);
		((Rect)(ref outer))._002Ector(0f, 0f, 1f, 1f);
		Rect inner = default(Rect);
		((Rect)(ref inner))._002Ector(0f, 0f, 1f, 1f);
		int size = verts.size;
		Fill(verts, uvs, cols, outer, inner);
		if (onPostFill != null)
		{
			onPostFill(this, size, verts, uvs, cols);
		}
	}
}
