using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;
using Yaml;

namespace Durango.UI.Control;

public class ItemIconTex : UITexture
{
	private const string RGBMaskShader = "Durango/NGUI/RGBMask";

	private const float StableMin = 0.3f;

	private const float StableMax = 1f;

	private const float GlitchMin = 0.05f;

	private const float GlitchMax = 0.1f;

	private const int DivideCount = 5;

	private const float PositionGlitchPower = 0.1f;

	private const float ColorGlitchPower = 0.15f;

	private static readonly Color ShadowColor;

	public const float ShadowOffset = 2f;

	private ItemColor _colors = new ItemColor(Color.white, Color.white, Color.white);

	private Vector4 _iconRect;

	private float _timer;

	private bool _glitchEnable;

	private bool _glitchEffectOn;

	private float[] _divideList;

	private Rect[] _rects;

	private Rect[] _uvs;

	private Vector3[] _colorShift;

	private bool _isRawIcon;

	private bool _isRGBMask;

	private int? _randomSeed;

	[HideInInspector]
	[SerializeField]
	private UISprite _subIconSprite;

	private static readonly Dictionary<UIAtlas, Material> RGBMaterials;

	public string Icon { get; private set; }

	public bool HideShadow { get; set; }

	static ItemIconTex()
	{
		ShadowColor = new Color(0f, 0f, 0f, 0.6f);
		RGBMaterials = new Dictionary<UIAtlas, Material>();
		GameManager.Reset += delegate
		{
			RGBMaterials.Clear();
		};
	}

	private static Material GetRGBMaterial([NotNull] UIAtlas atlas)
	{
		if (RGBMaterials.TryGetValue(atlas, out var value))
		{
			return value;
		}
		if (atlas.spriteMaterial.shader.name == "Durango/NGUI/RGBMax")
		{
			value = new Material(Shader.Find("Durango/NGUI/RGBMask"));
			value.name = atlas.name + " Material";
			value.mainTexture = atlas.spriteMaterial.mainTexture;
			Texture texture = atlas.spriteMaterial.GetTexture("_AlphaTex");
			value.SetTexture("_AlphaMask", (!(texture == null)) ? texture : Texture2D.whiteTexture);
		}
		RGBMaterials.Add(atlas, value);
		return value;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_randomSeed = null;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!_glitchEnable)
		{
			if (_glitchEffectOn)
			{
				GlitchEffectOn(on: false);
			}
		}
		else if (_timer > 0f)
		{
			_timer -= Time.deltaTime;
		}
		else
		{
			GlitchEffectOn(!_glitchEffectOn);
		}
	}

	public void SetIcon(ItemData item)
	{
		SetIcon(item.Icon, item.Unstable);
	}

	public void SetIcon(string prototypeId, int level)
	{
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(prototypeId, level);
		if (itemPrototype == null)
		{
			SetIcon((ItemData)null);
		}
		else
		{
			SetIcon(itemPrototype.Icon, itemPrototype.ColorR, itemPrototype.ColorG, itemPrototype.ColorB);
		}
	}

	public void SetIcon(Messages.RewardItem rewardItem)
	{
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(rewardItem.PrototypeId, rewardItem.Level);
		if (itemPrototype == null)
		{
			SetIcon((ItemData)null);
		}
		else if (string.IsNullOrEmpty(rewardItem.ColorR) && string.IsNullOrEmpty(rewardItem.ColorG) && string.IsNullOrEmpty(rewardItem.ColorB))
		{
			SetIcon(itemPrototype.Icon, itemPrototype.ColorR, itemPrototype.ColorG, itemPrototype.ColorB);
		}
		else
		{
			SetIcon(itemPrototype.Icon, new ItemColor(rewardItem.ColorR, rewardItem.ColorG, rewardItem.ColorB));
		}
	}

	public void SetIcon(string icon, string subIcon = null)
	{
		_SetIcon(icon, subIcon, Color.white, Color.white, Color.white);
		_SetGlitch(enable: false);
	}

	public void SetIcon(string icon, ItemColor cols)
	{
		SetIcon(new ItemIcon
		{
			Main = icon,
			Colors = cols
		});
	}

	public void SetIcon(ItemIcon icon, bool glitch = false)
	{
		_SetIcon(icon.Main, icon.Sub, icon.Colors[0], icon.Colors[1], icon.Colors[2]);
		_SetGlitch(glitch);
	}

	public void SetIcon(string icon, string rTable, string gTable, string bTable)
	{
		SetIcon(icon, null, rTable, gTable, bTable);
	}

	public void SetIcon(string icon, string subIcon, string rTable, string gTable, string bTable)
	{
		int? randomSeed = _randomSeed;
		if (!randomSeed.HasValue)
		{
			_randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}
		SetIcon(new ItemIcon
		{
			Main = icon,
			Sub = subIcon,
			Colors = MakeFromTableKey(rTable, gTable, bTable, _randomSeed)
		});
	}

	public static ItemColor MakeFromTableKey(string rTable, string gTable, string bTable, int? randomSeed = null)
	{
		ItemColor result = new ItemColor(3);
		Vector4 zero = Vector4.zero;
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			string key = null;
			switch (i)
			{
			case 0:
				key = rTable;
				break;
			case 1:
				key = gTable;
				break;
			case 2:
				key = bTable;
				break;
			}
			if (TryGetDefaultColor(key, out var col, randomSeed))
			{
				result[i] = col;
				zero += (Vector4)col;
				num++;
			}
			else
			{
				result[i] = Color.clear;
			}
		}
		if (num == 0)
		{
			return new ItemColor(Color.white);
		}
		Color col2 = zero / num;
		for (int j = 0; j < 3; j++)
		{
			if (result.GetColor(j, origin: true) == Color.clear)
			{
				result.SetColor(j, col2);
			}
		}
		return result;
	}

	public static bool TryGetDefaultColor(string key, out Color col, int? seed = null, Color defaultColor = default(Color))
	{
		if (string.IsNullOrEmpty(key))
		{
			col = defaultColor;
			return false;
		}
		if (key[0] == '#')
		{
			if (key.Length < 7)
			{
				col = defaultColor;
				return false;
			}
			col = NGUIText.ParseColor24(key, 1);
			return true;
		}
		ColorTable colorTable = ColorTableLoader.Load(key + ".raw");
		if (colorTable == null)
		{
			col = Color.clear;
			return false;
		}
		col = (seed.HasValue ? colorTable.GetRandom(seed.Value) : colorTable.GetColor(0f));
		return true;
	}

	private void _SetIcon(string icon, string subIcon, Color rChannel, Color gChannel, Color bChannel)
	{
		Icon = icon;
		_isRawIcon = !string.IsNullOrEmpty(icon) && icon[0] == '_';
		_colors[0] = rChannel;
		_colors[1] = gChannel;
		_colors[2] = bChannel;
		if (ResourceSingleton<UISpriteManager>.Instance().TryGet(icon, out var atlas, out var spriteData))
		{
			base.enabled = true;
			Texture texture = atlas.texture;
			float num = texture.width;
			float num2 = texture.height;
			Vector4 vector = new Vector4((float)spriteData.x / num, (float)spriteData.y / num2, (float)spriteData.width / num, (float)spriteData.height / num2);
			vector.y = 1f - (vector.y + vector.w);
			float num3 = spriteData.width + spriteData.paddingLeft + spriteData.paddingRight;
			float num4 = spriteData.height + spriteData.paddingBottom + spriteData.paddingTop;
			_iconRect.x = (float)spriteData.paddingLeft / num3;
			_iconRect.y = (float)spriteData.paddingBottom / num4;
			_iconRect.z = _iconRect.x + (float)spriteData.width / num3;
			_iconRect.w = _iconRect.y + (float)spriteData.height / num4;
			base.uvRect = new Rect(vector.x, vector.y, vector.z, vector.w);
			Material material = GetRGBMaterial(atlas);
			if (material == null)
			{
				_isRGBMask = false;
				material = atlas.spriteMaterial;
			}
			else
			{
				_isRGBMask = true;
			}
			this.material = material;
			MarkAsChanged();
		}
		else
		{
			base.enabled = false;
		}
		SetSubIcon(subIcon);
	}

	private void SetSubIcon(string icon)
	{
		if (ResourceSingleton<UISpriteManager>.Instance().TryGet(icon, out var _, out var _))
		{
			if (_subIconSprite == null)
			{
				_subIconSprite = base.gameObject.AddChild<UISprite>();
				_subIconSprite.fit = Fit.FitInside;
				_subIconSprite.depth = depth + 1;
				_subIconSprite.SetAnchor(base.gameObject, 1f, -24, 0f, 0, 1f, 0, 0f, 24);
				_subIconSprite.updateAnchors = AnchorUpdate.OnUpdate;
				_subIconSprite.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			}
			_subIconSprite.spriteName = icon;
		}
		else if ((bool)_subIconSprite)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_subIconSprite.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_subIconSprite.gameObject);
			}
		}
	}

	private void _SetGlitch(bool enable)
	{
		if (_glitchEnable != enable)
		{
			_glitchEnable = enable;
			GlitchEffectOn(on: false);
		}
	}

	private void GlitchEffectOn(bool on)
	{
		_glitchEffectOn = on;
		_timer = ((!on) ? Mathf.Lerp(0.3f, 1f, UnityEngine.Random.value) : Mathf.Lerp(0.05f, 0.1f, UnityEngine.Random.value));
		MarkAsChanged();
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		Vector4 vector = drawingDimensions;
		Vector4 drawingArea = default(Vector4);
		drawingArea.x = Mathf.Lerp(vector.x, vector.z, _iconRect.x);
		drawingArea.z = Mathf.Lerp(vector.x, vector.z, _iconRect.z);
		drawingArea.y = Mathf.Lerp(vector.y, vector.w, _iconRect.y);
		drawingArea.w = Mathf.Lerp(vector.y, vector.w, _iconRect.w);
		Rect outer = base.uvRect;
		Rect inner = outer;
		CalcFitArea(ref drawingArea, ref outer, ref inner);
		int size = arguments.verts.size;
		Rect rect = new Rect(drawingArea.x, drawingArea.y, drawingArea.z - drawingArea.x, drawingArea.w - drawingArea.y);
		Rect uv = base.uvRect;
		Color col = color;
		if (!_isRGBMask && _colors.HasValue)
		{
			Color clear = Color.clear;
			for (int i = 0; i < _colors.Count; i++)
			{
				clear += _colors[i];
			}
			clear /= (float)_colors.Count;
			col *= clear;
		}
		float num = (col.a = finalAlpha * finalAlpha);
		if (!HideShadow)
		{
			Rect vert = new Rect(rect);
			vert.x += 2f;
			vert.y -= 2f;
			Color shadowColor = ShadowColor;
			shadowColor.a *= num;
			DrawQuad(arguments, vert, uv, shadowColor);
		}
		if (!_glitchEffectOn)
		{
			DrawQuad(arguments, rect, uv, col);
			return;
		}
		if (_divideList == null)
		{
			_divideList = new float[4];
			_rects = new Rect[5];
			_uvs = new Rect[5];
			_colorShift = new Vector3[3];
		}
		float[] divideList = _divideList;
		for (int j = 0; j < divideList.Length; j++)
		{
			divideList[j] = UnityEngine.Random.value;
		}
		Array.Sort(divideList);
		Rect[] rects = _rects;
		Rect[] uvs = _uvs;
		for (int k = 0; k < 5; k++)
		{
			float num2 = ((k - 1 >= 0) ? divideList[k - 1] : 0f);
			float num3 = ((k >= 4) ? 1f : divideList[k]);
			Rect rect2 = new Rect(rect.xMin, Mathf.Lerp(rect.yMin, rect.yMax, num2), rect.width, rect.height * (num3 - num2));
			Rect rect3 = new Rect(uv.xMin, Mathf.Lerp(uv.yMin, uv.yMax, num2), uv.width, uv.height * (num3 - num2));
			float num4 = (UnityEngine.Random.value * 2f - 1f) * 0.1f;
			if (num4 > 0f)
			{
				rect2.xMin += num4 * rect2.width;
				rect3.xMax -= num4 * rect3.width;
			}
			else if (num4 < 0f)
			{
				rect2.xMax += num4 * rect2.width;
				rect3.xMin -= num4 * rect3.width;
			}
			rects[k] = rect2;
			uvs[k] = rect3;
		}
		Vector3[] colorShift = _colorShift;
		for (int l = 0; l < colorShift.Length; l++)
		{
			ref Vector3 reference = ref colorShift[l];
			reference = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value) * 2f - Vector2.one;
			colorShift[l].x *= rect.width * 0.15f;
			colorShift[l].y *= rect.height * 0.15f;
		}
		for (int m = 0; m < 5; m++)
		{
			for (int n = 0; n < colorShift.Length; n++)
			{
				Vector2 vector2 = colorShift[n];
				Rect vert2 = new Rect(rects[m]);
				vert2.position += vector2;
				Color clear2 = Color.clear;
				clear2[n] = 1f;
				clear2.a = 1f / 3f;
				DrawQuad(arguments, vert2, uvs[m], clear2);
			}
		}
		for (int num5 = 0; num5 < 5; num5++)
		{
			DrawQuad(arguments, rects[num5], uvs[num5], col);
		}
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	private void DrawQuad(UIGeometry.Arguments arguments, Rect vert, Rect uv, Color col)
	{
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		verts.Add(new Vector2(vert.xMin, vert.yMin));
		verts.Add(new Vector2(vert.xMin, vert.yMax));
		verts.Add(new Vector2(vert.xMax, vert.yMax));
		verts.Add(new Vector2(vert.xMax, vert.yMin));
		uvs.Add(new Vector2(uv.xMin, uv.yMin));
		uvs.Add(new Vector2(uv.xMin, uv.yMax));
		uvs.Add(new Vector2(uv.xMax, uv.yMax));
		uvs.Add(new Vector2(uv.xMax, uv.yMin));
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
		if (_isRGBMask)
		{
			List<Vector3> vector3Uvs = arguments.extentionUvs.GetVector3Uvs(1);
			List<Vector3> vector3Uvs2 = arguments.extentionUvs.GetVector3Uvs(2);
			List<Vector3> vector3Uvs3 = arguments.extentionUvs.GetVector3Uvs(3);
			FillColor(out var r, out var g, out var b);
			for (int i = 0; i < 4; i++)
			{
				vector3Uvs.Add(r);
				vector3Uvs2.Add(g);
				vector3Uvs3.Add(b);
			}
		}
	}

	private void FillColor(out Vector3 r, out Vector3 g, out Vector3 b)
	{
		if (_isRawIcon)
		{
			r = new Vector3(1f, 0f, 0f);
			g = new Vector3(0f, 1f, 0f);
			b = new Vector3(0f, 0f, 1f);
		}
		else
		{
			r = ColorToVector3(_colors[0]);
			g = ColorToVector3(_colors[1]);
			b = ColorToVector3(_colors[2]);
		}
	}

	private static Vector3 ColorToVector3(Color color)
	{
		return new Vector3(color.r, color.g, color.b);
	}
}
