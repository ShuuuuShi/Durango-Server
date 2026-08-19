using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Control;

public class UIMaskedSprite : UISprite
{
	private const string ShaderName = "Durango/NGUI/MaskedTransparent";

	private static readonly Dictionary<Material, Material> MaskedMaterials;

	private string _maskedSprite;

	private Rect? _maskRect;

	private static readonly int AlphaMask;

	private static readonly int AlphaTex;

	public override Material material
	{
		get
		{
			if (!Application.isPlaying)
			{
				return base.material;
			}
			return GetMaskedMaterial(base.material);
		}
	}

	public string MaskedSprite
	{
		get
		{
			return _maskedSprite;
		}
		set
		{
			if (!(_maskedSprite == value))
			{
				_maskedSprite = value;
				_maskRect = null;
				MarkAsChanged();
			}
		}
	}

	static UIMaskedSprite()
	{
		MaskedMaterials = new Dictionary<Material, Material>();
		AlphaMask = Shader.PropertyToID("_AlphaMask");
		AlphaTex = Shader.PropertyToID("_AlphaTex");
		GameManager.Reset += delegate
		{
			MaskedMaterials.Clear();
		};
	}

	public string GetShader()
	{
		return "Durango/NGUI/MaskedTransparent";
	}

	private static Material GetMaskedMaterial(Material origin)
	{
		if (origin == null)
		{
			return null;
		}
		if (MaskedMaterials.TryGetValue(origin, out var value))
		{
			return value;
		}
		value = new Material(Shader.Find("Durango/NGUI/MaskedTransparent"));
		value.name = $"{origin.name} Material";
		value.mainTexture = origin.mainTexture;
		Texture texture = origin.GetTexture(AlphaTex);
		value.SetTexture(AlphaMask, (!(texture == null)) ? texture : Texture2D.whiteTexture);
		MaskedMaterials.Add(origin, value);
		return value;
	}

	protected override void RefreshAtlasSprite()
	{
		_maskRect = null;
		base.RefreshAtlasSprite();
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		int size = arguments.verts.size;
		base.OnFill(arguments);
		Texture texture = mainTexture;
		if (texture == null)
		{
			return;
		}
		UISpriteData atlasSprite = GetAtlasSprite();
		if (atlasSprite == null)
		{
			return;
		}
		Rect? maskRect = _maskRect;
		if (!maskRect.HasValue)
		{
			UISpriteData spriteData = null;
			if (!string.IsNullOrEmpty(_maskedSprite) && ResourceSingleton<UISpriteManager>.Instance().TryGet(_maskedSprite, out var atlas, out spriteData) && atlas.spriteMaterial != base.material)
			{
				spriteData = null;
			}
			if (spriteData == null)
			{
				spriteData = atlasSprite;
			}
			_maskRect = NGUIMath.ConvertToTexCoords(new Rect(spriteData.x, spriteData.y, spriteData.width, spriteData.height), texture.width, texture.height);
		}
		BetterList<Vector3> verts = arguments.verts;
		List<Vector2> vector2Uvs = arguments.extentionUvs.GetVector2Uvs(1);
		Vector2 vector = base.localCenter;
		Vector2 vector2 = localSize;
		Rect rectangle = new Rect(vector - vector2 * 0.5f, vector2);
		Rect value = _maskRect.Value;
		for (int i = size; i < verts.size; i++)
		{
			Vector2 point = verts[i];
			Vector2 normalizedRectCoordinates = Rect.PointToNormalized(rectangle, point);
			Vector2 item = Rect.NormalizedToPoint(value, normalizedRectCoordinates);
			vector2Uvs.Add(item);
		}
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}
}
