using System.Collections;
using UnityEngine;

public class RGBMask : MonoBehaviour
{
	private UIWidget _widget;

	public Color R { get; private set; }

	public Color G { get; private set; }

	public Color B { get; private set; }

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private IEnumerator Start()
	{
		while ((Object)(object)Widget.panel == (Object)null)
		{
			yield return null;
		}
		InitWidget();
	}

	private void InitWidget()
	{
		UIPanel panel = Widget.panel;
		((Component)panel).gameObject.AddMissingComponent<RGBMaskPanel>();
		Shader val = Shader.Find("Custom/RGBMask");
		if ((Object)(object)Widget.shader != (Object)(object)val)
		{
			Widget.shader = val;
		}
	}

	public void SetColor(Color r, Color g, Color b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		R = r;
		G = g;
		B = b;
		Widget.MarkAsChanged();
	}
}
