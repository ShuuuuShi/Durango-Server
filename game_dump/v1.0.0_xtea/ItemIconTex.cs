using System;
using ItemSystem;
using UnityEngine;

[RequireComponent(typeof(UITexture))]
public class ItemIconTex : MonoBehaviour
{
	private struct AtlasStruct
	{
		public UIAtlas Atlas;

		public Material Material;
	}

	private static AtlasStruct[] _atlases;

	private UITexture _uiTexture;

	private RGBMask _rgbMask;

	private ItemColor _colors = new ItemColor(Color.white, Color.white, Color.white);

	private Rect _drawRect;

	private static AtlasStruct[] Atlases
	{
		get
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			if (_atlases != null)
			{
				return _atlases;
			}
			_atlases = new AtlasStruct[2];
			_atlases[0].Atlas = KSingleton<UIManager>.Instance().RGBAtlas;
			_atlases[1].Atlas = KSingleton<UIManager>.Instance().IconAtlas;
			for (int i = 0; i < _atlases.Length; i++)
			{
				Material val = new Material(Shader.Find("Custom/RGBMask"));
				((Object)val).name = $"{((Object)_atlases[i].Atlas).name} Material";
				val.mainTexture = _atlases[i].Atlas.spriteMaterial.mainTexture;
				Texture val2 = null;
				if (_atlases[i].Atlas.spriteMaterial.HasProperty("_AlphaTex"))
				{
					val2 = _atlases[i].Atlas.spriteMaterial.GetTexture("_AlphaTex");
				}
				val.SetTexture("_AlphaMask", (Texture)((!((Object)(object)val2 == (Object)null)) ? ((object)val2) : ((object)Texture2D.whiteTexture)));
				_atlases[i].Material = val;
			}
			return _atlases;
		}
	}

	public UITexture UITexture
	{
		get
		{
			if ((Object)(object)_uiTexture == (Object)null)
			{
				_uiTexture = ((Component)this).GetComponent<UITexture>();
				UITexture uiTexture = _uiTexture;
				uiTexture.onPostFill = (UIWidget.OnPostFillCallback)Delegate.Combine(uiTexture.onPostFill, new UIWidget.OnPostFillCallback(OnPostFill));
			}
			return _uiTexture;
		}
	}

	public RGBMask RGBMask
	{
		get
		{
			if ((Object)(object)_rgbMask == (Object)null)
			{
				_rgbMask = ((Component)this).gameObject.AddMissingComponent<RGBMask>();
			}
			return _rgbMask;
		}
	}

	public string Icon { get; private set; }

	public void SetIcon(ItemData item)
	{
		SetIcon(item.Icon, item.Colors);
	}

	public void SetIcon(string icon)
	{
		SetIcon(icon, _colors);
	}

	public void SetIcon(string icon, ItemColor cols)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		SetIcon(icon, cols[0], cols[1], cols[2]);
	}

	public void SetIcon(string icon, Color rChannel, Color gChannel, Color bChannel)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		Icon = icon;
		_colors[0] = rChannel;
		_colors[1] = gChannel;
		_colors[2] = bChannel;
		UISpriteData uISpriteData = null;
		AtlasStruct atlasStruct = default(AtlasStruct);
		AtlasStruct[] atlases = Atlases;
		for (int i = 0; i < atlases.Length; i++)
		{
			uISpriteData = atlases[i].Atlas.GetSprite(icon);
			if (uISpriteData != null)
			{
				atlasStruct = atlases[i];
				break;
			}
		}
		if (uISpriteData != null)
		{
			((Component)this).gameObject.SetActive(true);
			Texture mainTexture = atlasStruct.Atlas.spriteMaterial.mainTexture;
			float num = mainTexture.width;
			float num2 = mainTexture.height;
			Vector4 val = default(Vector4);
			((Vector4)(ref val))._002Ector((float)uISpriteData.x / num, (float)uISpriteData.y / num2, (float)uISpriteData.width / num, (float)uISpriteData.height / num2);
			val.y = 1f - (val.y + val.w);
			float num3 = uISpriteData.width + uISpriteData.paddingLeft + uISpriteData.paddingRight;
			float num4 = uISpriteData.height + uISpriteData.paddingBottom + uISpriteData.paddingTop;
			((Rect)(ref _drawRect)).x = (float)uISpriteData.paddingLeft / num3;
			((Rect)(ref _drawRect)).y = (float)uISpriteData.paddingBottom / num4;
			((Rect)(ref _drawRect)).width = (float)uISpriteData.width / num3;
			((Rect)(ref _drawRect)).height = (float)uISpriteData.height / num4;
			UITexture.material = atlasStruct.Material;
			UITexture.uvRect = new Rect(val.x, val.y, val.z, val.w);
			RGBMask.SetColor(rChannel, gChannel, bChannel);
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void OnPostFill(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		int width = widget.width;
		int height = widget.height;
		Vector2 pivotOffset = widget.pivotOffset;
		Vector2 val = default(Vector2);
		val.x = (float)width * (0f - pivotOffset.x);
		val.y = (float)height * (0f - pivotOffset.y);
		int i = 0;
		for (int size = verts.size; i < size; i++)
		{
			Vector3 value = verts[i];
			value.x = (value.x - val.x) * ((Rect)(ref _drawRect)).width + val.x;
			value.y = (value.y - val.y) * ((Rect)(ref _drawRect)).height + val.y;
			value.x += (float)width * ((Rect)(ref _drawRect)).x;
			value.y += (float)height * ((Rect)(ref _drawRect)).y;
			verts[i] = value;
		}
	}
}
