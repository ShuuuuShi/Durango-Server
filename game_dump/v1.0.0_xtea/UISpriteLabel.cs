using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
[AddComponentMenu("NGUI/UI/NGUI SpriteLabel")]
public class UISpriteLabel : MonoBehaviour
{
	private struct IconStruct
	{
		public string Icon;

		public float Aspect;
	}

	private struct TextRange
	{
		public int Begin;

		public int End;

		public int Length => (Begin >= 0) ? (End - Begin + 1) : 0;
	}

	public delegate void OnFillCallback(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, ref bool isOverrideAlpha);

	public OnFillCallback onFill;

	public UIWidget.OnPostFillCallback onPostFill;

	private static StringBuilder _stringBuilder = new StringBuilder();

	[SerializeField]
	private bool mGettext;

	[SerializeField]
	private string mContext = string.Empty;

	[SerializeField]
	private string mComment = string.Empty;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private List<UIAtlas> _atlases = new List<UIAtlas>();

	[SerializeField]
	private int _spriteDepthOffset = 1;

	[SerializeField]
	private string _text;

	private string _printedText;

	[SerializeField]
	private float _maxIconScale = 2f;

	[SerializeField]
	private bool _spriteBBCodeDisable;

	[SerializeField]
	private Color _defaultSpriteColor = Color.white;

	[SerializeField]
	private List<UIAtlas> _iconAtlas = new List<UIAtlas>();

	[SerializeField]
	private List<IconStruct> _icons = new List<IconStruct>();

	[SerializeField]
	private List<UISprite> _sprites = new List<UISprite>();

	public string text
	{
		get
		{
			return _text;
		}
		set
		{
			if (_text != value)
			{
				_text = value;
				_printedText = null;
				SetupLabel();
			}
		}
	}

	public string printedText
	{
		get
		{
			if (string.IsNullOrEmpty(_text))
			{
				return string.Empty;
			}
			if (_printedText != null && Application.isPlaying)
			{
				return _printedText;
			}
			_printedText = UILabelPreProcesser.PreProcessText(this, _text) ?? _text;
			return _printedText;
		}
	}

	public float alpha
	{
		get
		{
			return _label.alpha;
		}
		set
		{
			_label.alpha = value;
		}
	}

	public UILabel Label
	{
		get
		{
			return _label;
		}
		set
		{
			_label = value;
		}
	}

	public List<UIAtlas> Atlases => _atlases;

	public int Depth
	{
		get
		{
			return _label.depth;
		}
		set
		{
			_label.depth = value;
			int i = 0;
			for (int count = _sprites.Count; i < count; i++)
			{
				_sprites[i].depth = value;
			}
		}
	}

	public bool SpriteBBCodeDisable
	{
		get
		{
			return _spriteBBCodeDisable;
		}
		set
		{
			_spriteBBCodeDisable = value;
		}
	}

	private void Awake()
	{
		OnLocalize();
	}

	private void Start()
	{
		SetupLabel();
	}

	private void OnEnable()
	{
		if ((Object)(object)_label != (Object)null)
		{
			((Behaviour)_label).enabled = true;
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)_label != (Object)null)
		{
			((Behaviour)_label).enabled = false;
		}
	}

	private void OnLocalize()
	{
		string text = null;
		if (mGettext)
		{
			text = _text;
		}
		else if ((Object)(object)_label != (Object)null && _label.useGettext)
		{
			text = _label.text;
		}
		if (!string.IsNullOrEmpty(text))
		{
			this.text = LocalizeSystem.Get(text);
		}
	}

	private void DestroyIconObject(int count)
	{
		for (int num = _sprites.Count - 1; num >= count; num--)
		{
			if ((Object)(object)_sprites[num] != (Object)null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)((Component)_sprites[num]).gameObject);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)((Component)_sprites[num]).gameObject);
				}
			}
			_sprites.RemoveAt(num);
		}
	}

	private void SetupLabel()
	{
		if ((Object)(object)_label != (Object)null)
		{
			_label.onPostFill = OnPostFill;
			_label.text = ProcessText(printedText);
			_label.MarkAsChanged();
			_label.geometry.Clear();
			_label.OnFill(_label.geometry.verts, _label.geometry.uvs, _label.geometry.cols);
		}
	}

	private static bool FindIconTag(string text, int index, out TextRange total, out TextRange icon, out TextRange ratio, out bool isIconTag)
	{
		total.Begin = -1;
		total.End = -1;
		icon.Begin = -1;
		icon.End = -1;
		ratio.Begin = -1;
		ratio.End = -1;
		isIconTag = false;
		for (int i = index; i < text.Length; i++)
		{
			if (text[i] == '[')
			{
				total.Begin = i;
			}
			else if (text[i] == ']')
			{
				total.End = i;
				break;
			}
		}
		int length = total.Length;
		if (length <= 0)
		{
			return false;
		}
		isIconTag = length - 2 > 5 && text[total.Begin + 1] == 'i' && text[total.Begin + 2] == 'c' && text[total.Begin + 3] == 'o' && text[total.Begin + 4] == 'n' && text[total.Begin + 5] == '=';
		icon.Begin = ((!isIconTag) ? (total.Begin + 1) : (total.Begin + 6));
		icon.End = total.End - 1;
		for (int j = icon.Begin; j < total.End; j++)
		{
			if (text[j] == ':')
			{
				icon.End = j - 1;
				ratio.Begin = j + 1;
				break;
			}
		}
		if (ratio.Begin != 0)
		{
			ratio.End = total.End - 1;
		}
		return true;
	}

	public static bool HasCharacter(string text, IList<UIAtlas> atlases)
	{
		string text2 = NGUIText.StripSymbols(text);
		if (text2.Length == 0)
		{
			return false;
		}
		int num = 0;
		TextRange total;
		TextRange icon;
		TextRange ratio;
		bool isIconTag;
		while (FindIconTag(text2, num, out total, out icon, out ratio, out isIconTag))
		{
			if (total.Begin > num || icon.Length <= 0)
			{
				return true;
			}
			num = total.End + 1;
			if (!isIconTag)
			{
				string spriteName = text2.Substring(icon.Begin, icon.Length);
				if (!GetSpriteData(atlases, spriteName, out var _, out var _))
				{
					return true;
				}
			}
		}
		return num < text2.Length;
	}

	private string ProcessText(string txt)
	{
		_icons.Clear();
		_iconAtlas.Clear();
		int i = 0;
		for (int count = _sprites.Count; i < count; i++)
		{
			((Component)_sprites[i]).gameObject.SetActive(false);
		}
		if (string.IsNullOrEmpty(txt))
		{
			return null;
		}
		StringBuilder stringBuilder = null;
		int num = 0;
		int num2 = 0;
		TextRange total;
		TextRange icon;
		TextRange ratio;
		bool isIconTag;
		IconStruct item = default(IconStruct);
		while (FindIconTag(txt, num, out total, out icon, out ratio, out isIconTag))
		{
			num = total.End + 1;
			if (icon.Length <= 0)
			{
				continue;
			}
			string text = txt.Substring(icon.Begin, icon.Length);
			if (GetSpriteData(_atlases, text, out var sprite, out var atlas))
			{
				if (ratio.Length <= 0 || !float.TryParse(txt.Substring(ratio.Begin, ratio.Length), out var result))
				{
					result = 1f;
				}
				result = Mathf.Min(result, _maxIconScale);
				item.Icon = text;
				item.Aspect = (float)(sprite.height + sprite.paddingBottom + sprite.paddingTop) / (float)(sprite.width + sprite.paddingLeft + sprite.paddingRight);
				_icons.Add(item);
				_iconAtlas.Add(atlas);
				if (stringBuilder == null)
				{
					stringBuilder = _stringBuilder;
					stringBuilder.Length = 0;
				}
				stringBuilder.Append(txt, num2, total.Begin - num2);
				stringBuilder.AppendFormat("[_x{0:0.#}]", result / item.Aspect);
				num2 = num;
			}
			else if (isIconTag)
			{
				if (stringBuilder == null)
				{
					stringBuilder = _stringBuilder;
					stringBuilder.Length = 0;
				}
				stringBuilder.Append(txt, num2, total.Begin - num2);
				num2 = num;
			}
		}
		if (stringBuilder == null)
		{
			return txt;
		}
		if (num2 < txt.Length)
		{
			stringBuilder.Append(txt, num2, txt.Length - num2);
		}
		return stringBuilder.ToString();
	}

	private static bool GetSpriteData(IList<UIAtlas> atlases, string spriteName, out UISpriteData sprite, out UIAtlas atlas)
	{
		if (string.IsNullOrEmpty(spriteName))
		{
			sprite = null;
			atlas = null;
			return false;
		}
		int count = atlases.Count;
		for (int i = 0; i < count; i++)
		{
			atlas = atlases[i];
			sprite = atlas.GetSprite(spriteName);
			if (sprite != null)
			{
				return true;
			}
		}
		atlas = null;
		sprite = null;
		return false;
	}

	public bool HasSprite(string spriteName)
	{
		UISpriteData sprite;
		UIAtlas atlas;
		return GetSpriteData(_atlases, spriteName, out sprite, out atlas);
	}

	private UISprite Get(int index)
	{
		if (_sprites.Count == index)
		{
			UISprite item = ((Component)this).gameObject.AddChild<UISprite>();
			_sprites.Add(item);
		}
		UISprite uISprite = _sprites[index];
		if ((Object)(object)uISprite == (Object)null)
		{
			uISprite = ((Component)this).gameObject.AddChild<UISprite>();
		}
		uISprite.depth = _label.depth + _spriteDepthOffset;
		((Component)_sprites[index]).gameObject.SetActive(true);
		return _sprites[index];
	}

	public void OnPostFill(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		bool isOverrideAlpha = false;
		if (onFill != null)
		{
			onFill(widget, bufferOffset, verts, uvs, cols, ref isOverrideAlpha);
		}
		int offset = bufferOffset;
		int count = _icons.Count;
		for (int i = 0; i < count; i++)
		{
			IconStruct iconStruct = _icons[i];
			offset = NextIconIndex(uvs, offset);
			if (offset == -1)
			{
				break;
			}
			UISprite uISprite = Get(i);
			uISprite.atlas = _iconAtlas[i];
			uISprite.spriteName = iconStruct.Icon;
			float num = float.MaxValue;
			float num2 = float.MinValue;
			float num3 = float.MinValue;
			float num4 = float.MaxValue;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			for (int j = 0; j < 4; j++)
			{
				Vector3 val = verts[offset + j];
				num = Mathf.Min(val.x, num);
				num2 = Mathf.Max(val.x, num2);
				num3 = Mathf.Max(val.y, num3);
				num4 = Mathf.Min(val.y, num4);
				Color32 val2 = Color32.op_Implicit(cols[offset + j]);
				num5 += val2.r;
				num6 += val2.g;
				num7 += val2.b;
				num8 += val2.a;
			}
			float num9 = num2 - num;
			((Component)uISprite).transform.localPosition = new Vector3(num + num2, num4 + num3) * 0.5f;
			uISprite.width = (int)num9;
			uISprite.height = (int)(num9 * iconStruct.Aspect);
			if (_spriteBBCodeDisable)
			{
				uISprite.color = _defaultSpriteColor;
			}
			else
			{
				uISprite.color = Color32.op_Implicit(new Color32((byte)(num5 / 4), (byte)(num6 / 4), (byte)(num7 / 4), (!isOverrideAlpha) ? byte.MaxValue : ((byte)(num8 / 4))));
			}
			offset += 4;
		}
		DestroyIconObject(count);
		if (onPostFill != null)
		{
			onPostFill(widget, bufferOffset, verts, uvs, cols);
		}
	}

	private int NextIconIndex(BetterList<Vector2> uvs, int offset)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = offset; i + 3 < uvs.size; i += 4)
		{
			if (uvs[i] == uvs[i + 1] && uvs[i + 1] == uvs[i + 2] && uvs[i + 2] == uvs[i + 3])
			{
				return i;
			}
		}
		return -1;
	}
}
