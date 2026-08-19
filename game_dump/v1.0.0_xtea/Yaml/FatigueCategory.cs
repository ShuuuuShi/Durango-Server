using UnityEngine;

namespace Yaml;

public class FatigueCategory
{
	public Gettext name;

	public Gettext description;

	public string icon;

	public string color;

	public float default_ratio;

	private Color _color;

	public Color GetColor()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (_color == Color.clear)
		{
			if (color == null)
			{
				return Color.white;
			}
			_color = NGUIText.ParseColor(color);
		}
		return _color;
	}
}
