using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class Glitter : UIWidget
{
	private static readonly Dictionary<Texture, Material> Materials;

	private UIWidget _target;

	private Material _material;

	public override Material material => _material;

	static Glitter()
	{
		Materials = new Dictionary<Texture, Material>();
		GameManager.Reset += delegate
		{
			Materials.Clear();
		};
	}

	public static Material GetMaterial(Texture texture, bool isDual)
	{
		if (Materials.TryGetValue(texture, out var value))
		{
			return value;
		}
		value = new Material(Shader.Find("Durango/NGUI/GlitterEffect"));
		value.mainTexture = texture;
		if (isDual)
		{
			value.EnableKeyword("_DUAL_ON");
		}
		else
		{
			value.DisableKeyword("_DUAL_ON");
		}
		Materials.Add(texture, value);
		return value;
	}

	private static Material GetMaterial(UIWidget widget)
	{
		if (widget == null)
		{
			return null;
		}
		Material material = widget.material;
		Texture texture = null;
		bool flag = false;
		if ((bool)material)
		{
			if (material.HasProperty("_AlphaTex"))
			{
				Texture texture2 = material.GetTexture("_AlphaTex");
				if (texture2 != null)
				{
					flag = true;
					texture = texture2;
				}
			}
			if (!flag)
			{
				texture = material.mainTexture;
			}
		}
		else
		{
			texture = widget.mainTexture;
		}
		if (texture == null)
		{
			return null;
		}
		return GetMaterial(texture, flag);
	}

	protected override void OnStart()
	{
		base.OnStart();
		SetDimensions(2, 2);
		base.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		int size = arguments.verts.size;
		_target.OnFill(arguments);
		arguments.extentionUvs.Clear();
		BetterList<Color> cols = arguments.cols;
		Color value = color;
		for (int i = size; i < cols.size; i++)
		{
			cols[i] = value;
		}
		List<Vector2> vector2Uvs = arguments.extentionUvs.GetVector2Uvs(1);
		vector2Uvs.Clear();
		Vector3[] array = _target.localCorners;
		Vector2 vector = array[2] - array[0];
		BetterList<Vector3> verts = arguments.verts;
		Vector2 item = default(Vector2);
		for (int j = 0; j < verts.size; j++)
		{
			Vector3 vector2 = verts[j];
			item.x = (vector2.x - array[0].x) / vector.x;
			item.y = (vector2.y - array[0].y) / vector.y;
			vector2Uvs.Add(item);
		}
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	private void Set(UIWidget target, Material mat)
	{
		_target = target;
		depth = _target.depth + 1;
		_material = mat;
		MarkAsChanged();
	}

	public static void On([NotNull] UIWidget widget)
	{
		On(widget, Color.white);
	}

	public static void On([NotNull] UIWidget widget, Color color)
	{
		Material material = GetMaterial(widget);
		if (!(material == null))
		{
			Glitter glitter = widget.gameObject.AddMissingComponent<Glitter>();
			glitter.color = color;
			glitter.Set(widget, material);
		}
	}

	public static void Off([NotNull] UIWidget widget)
	{
		Glitter component = widget.GetComponent<Glitter>();
		if (!(component == null))
		{
			Object.Destroy(component);
		}
	}
}
