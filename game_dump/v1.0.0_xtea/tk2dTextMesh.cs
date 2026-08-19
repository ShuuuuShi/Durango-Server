using System;
using System.Text;
using UnityEngine;
using tk2dRuntime;

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/Text/tk2dTextMesh")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class tk2dTextMesh : MonoBehaviour, ISpriteCollectionForceBuild
{
	[Flags]
	private enum UpdateFlags
	{
		UpdateNone = 0,
		UpdateText = 1,
		UpdateColors = 2,
		UpdateBuffers = 4
	}

	private tk2dFontData _fontInst;

	private string _formattedText = string.Empty;

	[SerializeField]
	private tk2dFontData _font;

	[SerializeField]
	private string _text = string.Empty;

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private Color _color2 = Color.white;

	[SerializeField]
	private bool _useGradient;

	[SerializeField]
	private int _textureGradient;

	[SerializeField]
	private TextAnchor _anchor = (TextAnchor)6;

	[SerializeField]
	private Vector3 _scale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	private bool _kerning;

	[SerializeField]
	private int _maxChars = 16;

	[SerializeField]
	private bool _inlineStyling;

	[SerializeField]
	private bool _formatting;

	[SerializeField]
	private int _wordWrapWidth;

	[SerializeField]
	private float spacing;

	[SerializeField]
	private float lineSpacing;

	[SerializeField]
	private tk2dTextMeshData data = new tk2dTextMeshData();

	private Vector3[] vertices;

	private Vector2[] uvs;

	private Vector2[] uv2;

	private Color32[] colors;

	private Color32[] untintedColors;

	private UpdateFlags updateFlags = UpdateFlags.UpdateBuffers;

	private Mesh mesh;

	private MeshFilter meshFilter;

	private Renderer _cachedRenderer;

	public string FormattedText => _formattedText;

	public tk2dFontData font
	{
		get
		{
			UpgradeData();
			return data.font;
		}
		set
		{
			UpgradeData();
			data.font = value;
			_fontInst = data.font.inst;
			SetNeedUpdate(UpdateFlags.UpdateText);
			UpdateMaterial();
		}
	}

	public bool formatting
	{
		get
		{
			UpgradeData();
			return data.formatting;
		}
		set
		{
			UpgradeData();
			if (data.formatting != value)
			{
				data.formatting = value;
				SetNeedUpdate(UpdateFlags.UpdateText);
			}
		}
	}

	public int wordWrapWidth
	{
		get
		{
			UpgradeData();
			return data.wordWrapWidth;
		}
		set
		{
			UpgradeData();
			if (data.wordWrapWidth != value)
			{
				data.wordWrapWidth = value;
				SetNeedUpdate(UpdateFlags.UpdateText);
			}
		}
	}

	public string text
	{
		get
		{
			UpgradeData();
			return data.text;
		}
		set
		{
			UpgradeData();
			data.text = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public Color color
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			UpgradeData();
			return data.color;
		}
		set
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			UpgradeData();
			data.color = value;
			SetNeedUpdate(UpdateFlags.UpdateColors);
		}
	}

	public Color color2
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			UpgradeData();
			return data.color2;
		}
		set
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			UpgradeData();
			data.color2 = value;
			SetNeedUpdate(UpdateFlags.UpdateColors);
		}
	}

	public bool useGradient
	{
		get
		{
			UpgradeData();
			return data.useGradient;
		}
		set
		{
			UpgradeData();
			data.useGradient = value;
			SetNeedUpdate(UpdateFlags.UpdateColors);
		}
	}

	public TextAnchor anchor
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			UpgradeData();
			return data.anchor;
		}
		set
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			UpgradeData();
			data.anchor = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public Vector3 scale
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			UpgradeData();
			return data.scale;
		}
		set
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			UpgradeData();
			data.scale = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public bool kerning
	{
		get
		{
			UpgradeData();
			return data.kerning;
		}
		set
		{
			UpgradeData();
			data.kerning = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public int maxChars
	{
		get
		{
			UpgradeData();
			return data.maxChars;
		}
		set
		{
			UpgradeData();
			data.maxChars = value;
			SetNeedUpdate(UpdateFlags.UpdateBuffers);
		}
	}

	public int textureGradient
	{
		get
		{
			UpgradeData();
			return data.textureGradient;
		}
		set
		{
			UpgradeData();
			data.textureGradient = value % font.gradientCount;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public bool inlineStyling
	{
		get
		{
			UpgradeData();
			return data.inlineStyling;
		}
		set
		{
			UpgradeData();
			data.inlineStyling = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	public float Spacing
	{
		get
		{
			UpgradeData();
			return data.spacing;
		}
		set
		{
			UpgradeData();
			if (data.spacing != value)
			{
				data.spacing = value;
				SetNeedUpdate(UpdateFlags.UpdateText);
			}
		}
	}

	public float LineSpacing
	{
		get
		{
			UpgradeData();
			return data.lineSpacing;
		}
		set
		{
			UpgradeData();
			if (data.lineSpacing != value)
			{
				data.lineSpacing = value;
				SetNeedUpdate(UpdateFlags.UpdateText);
			}
		}
	}

	public int SortingOrder
	{
		get
		{
			return CachedRenderer.sortingOrder;
		}
		set
		{
			if (CachedRenderer.sortingOrder != value)
			{
				data.renderLayer = value;
				CachedRenderer.sortingOrder = value;
			}
		}
	}

	private Renderer CachedRenderer
	{
		get
		{
			if ((Object)(object)_cachedRenderer == (Object)null)
			{
				_cachedRenderer = ((Component)this).GetComponent<Renderer>();
			}
			return _cachedRenderer;
		}
	}

	private bool useInlineStyling => inlineStyling && _fontInst.textureGradients;

	private void UpgradeData()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (data.version != 1)
		{
			data.font = _font;
			data.text = _text;
			data.color = _color;
			data.color2 = _color2;
			data.useGradient = _useGradient;
			data.textureGradient = _textureGradient;
			data.anchor = _anchor;
			data.scale = _scale;
			data.kerning = _kerning;
			data.maxChars = _maxChars;
			data.inlineStyling = _inlineStyling;
			data.formatting = _formatting;
			data.wordWrapWidth = _wordWrapWidth;
			data.spacing = spacing;
			data.lineSpacing = lineSpacing;
		}
		data.version = 1;
	}

	private static int GetInlineStyleCommandLength(int cmdSymbol)
	{
		int result = 0;
		switch (cmdSymbol)
		{
		case 99:
			result = 5;
			break;
		case 67:
			result = 9;
			break;
		case 103:
			result = 9;
			break;
		case 71:
			result = 17;
			break;
		}
		return result;
	}

	public string FormatText(string unformattedString)
	{
		string _targetString = string.Empty;
		FormatText(ref _targetString, unformattedString);
		return _targetString;
	}

	private void FormatText()
	{
		FormatText(ref _formattedText, data.text);
	}

	private void FormatText(ref string _targetString, string _source)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		InitInstance();
		if (!formatting || wordWrapWidth == 0 || _fontInst.texelSize == Vector2.zero)
		{
			_targetString = _source;
			return;
		}
		float num = _fontInst.texelSize.x * (float)wordWrapWidth;
		StringBuilder stringBuilder = new StringBuilder(_source.Length);
		float num2 = 0f;
		float num3 = 0f;
		int num4 = -1;
		int num5 = -1;
		bool flag = false;
		for (int i = 0; i < _source.Length; i++)
		{
			char c = _source[i];
			bool flag2 = c == '^';
			tk2dFontChar tk2dFontChar2;
			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(c))
				{
					c = '\0';
				}
				tk2dFontChar2 = _fontInst.charDict[c];
			}
			else
			{
				if (c >= _fontInst.chars.Length)
				{
					c = '\0';
				}
				tk2dFontChar2 = _fontInst.chars[(uint)c];
			}
			if (flag2)
			{
				c = '^';
			}
			if (flag)
			{
				flag = false;
				continue;
			}
			if (data.inlineStyling && c == '^' && i + 1 < _source.Length)
			{
				if (_source[i + 1] != '^')
				{
					int inlineStyleCommandLength = GetInlineStyleCommandLength(_source[i + 1]);
					int num6 = 1 + inlineStyleCommandLength;
					for (int j = 0; j < num6; j++)
					{
						if (i + j < _source.Length)
						{
							stringBuilder.Append(_source[i + j]);
						}
					}
					i += num6 - 1;
					continue;
				}
				flag = true;
				stringBuilder.Append('^');
			}
			switch (c)
			{
			case '\n':
				num2 = 0f;
				num3 = 0f;
				num4 = stringBuilder.Length;
				num5 = i;
				break;
			case ' ':
				num2 += (tk2dFontChar2.advance + data.spacing) * data.scale.x;
				num3 = num2;
				num4 = stringBuilder.Length;
				num5 = i;
				break;
			default:
				if (num2 + tk2dFontChar2.p1.x * data.scale.x > num)
				{
					if (num3 > 0f)
					{
						num3 = 0f;
						num2 = 0f;
						stringBuilder.Remove(num4 + 1, stringBuilder.Length - num4 - 1);
						stringBuilder.Append('\n');
						i = num5;
						continue;
					}
					stringBuilder.Append('\n');
					num2 = (tk2dFontChar2.advance + data.spacing) * data.scale.x;
				}
				else
				{
					num2 += (tk2dFontChar2.advance + data.spacing) * data.scale.x;
				}
				break;
			}
			stringBuilder.Append(c);
		}
		_targetString = stringBuilder.ToString();
	}

	private void SetNeedUpdate(UpdateFlags uf)
	{
		if (updateFlags == UpdateFlags.UpdateNone)
		{
			updateFlags |= uf;
			tk2dUpdateManager.QueueCommit(this);
		}
		else
		{
			updateFlags |= uf;
		}
	}

	private void InitInstance()
	{
		if (data != null && (Object)(object)data.font != (Object)null)
		{
			_fontInst = data.font.inst;
			_fontInst.InitDictionary();
		}
	}

	private void Awake()
	{
		UpgradeData();
		if ((Object)(object)data.font != (Object)null)
		{
			_fontInst = data.font.inst;
		}
		updateFlags = UpdateFlags.UpdateBuffers;
		if ((Object)(object)data.font != (Object)null)
		{
			Init();
			UpdateMaterial();
		}
		updateFlags = UpdateFlags.UpdateNone;
	}

	protected void OnDestroy()
	{
		if ((Object)(object)meshFilter == (Object)null)
		{
			meshFilter = ((Component)this).GetComponent<MeshFilter>();
		}
		if ((Object)(object)meshFilter != (Object)null)
		{
			mesh = meshFilter.sharedMesh;
		}
		if (Object.op_Implicit((Object)(object)mesh))
		{
			Object.DestroyImmediate((Object)(object)mesh, true);
			meshFilter.mesh = null;
		}
	}

	public int NumDrawnCharacters()
	{
		int num = NumTotalCharacters();
		if (num > data.maxChars)
		{
			num = data.maxChars;
		}
		return num;
	}

	public int NumTotalCharacters()
	{
		InitInstance();
		if ((updateFlags & (UpdateFlags.UpdateText | UpdateFlags.UpdateBuffers)) != 0)
		{
			FormatText();
		}
		int num = 0;
		for (int i = 0; i < _formattedText.Length; i++)
		{
			int num2 = _formattedText[i];
			bool flag = num2 == 94;
			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(num2))
				{
					num2 = 0;
				}
			}
			else if (num2 >= _fontInst.chars.Length)
			{
				num2 = 0;
			}
			if (flag)
			{
				num2 = 94;
			}
			if (num2 == 10)
			{
				continue;
			}
			if (data.inlineStyling && num2 == 94 && i + 1 < _formattedText.Length)
			{
				if (_formattedText[i + 1] != '^')
				{
					i += GetInlineStyleCommandLength(_formattedText[i + 1]);
					continue;
				}
				i++;
			}
			num++;
		}
		return num;
	}

	[Obsolete("Use GetEstimatedMeshBoundsForString().size instead")]
	public Vector2 GetMeshDimensionsForString(string str)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return tk2dTextGeomGen.GetMeshDimensionsForString(str, tk2dTextGeomGen.Data(data, _fontInst, _formattedText));
	}

	public Bounds GetEstimatedMeshBoundsForString(string str)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		InitInstance();
		tk2dTextGeomGen.GeomData geomData = tk2dTextGeomGen.Data(data, _fontInst, _formattedText);
		Vector2 meshDimensionsForString = tk2dTextGeomGen.GetMeshDimensionsForString(FormatText(str), geomData);
		float yAnchorForHeight = tk2dTextGeomGen.GetYAnchorForHeight(meshDimensionsForString.y, geomData);
		float xAnchorForWidth = tk2dTextGeomGen.GetXAnchorForWidth(meshDimensionsForString.x, geomData);
		float num = (_fontInst.lineHeight + data.lineSpacing) * data.scale.y;
		return new Bounds(new Vector3(xAnchorForWidth + meshDimensionsForString.x * 0.5f, yAnchorForHeight + meshDimensionsForString.y * 0.5f + num, 0f), Vector3.Scale(Vector2.op_Implicit(meshDimensionsForString), new Vector3(1f, -1f, 1f)));
	}

	public void Init(bool force)
	{
		if (force)
		{
			SetNeedUpdate(UpdateFlags.UpdateBuffers);
		}
		Init();
	}

	public void Init()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Expected O, but got Unknown
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)_fontInst) || ((updateFlags & UpdateFlags.UpdateBuffers) == 0 && !((Object)(object)mesh == (Object)null)))
		{
			return;
		}
		_fontInst.InitDictionary();
		FormatText();
		tk2dTextGeomGen.GeomData geomData = tk2dTextGeomGen.Data(data, _fontInst, _formattedText);
		tk2dTextGeomGen.GetTextMeshGeomDesc(out var numVertices, out var numIndices, geomData);
		vertices = (Vector3[])(object)new Vector3[numVertices];
		uvs = (Vector2[])(object)new Vector2[numVertices];
		colors = (Color32[])(object)new Color32[numVertices];
		untintedColors = (Color32[])(object)new Color32[numVertices];
		if (_fontInst.textureGradients)
		{
			uv2 = (Vector2[])(object)new Vector2[numVertices];
		}
		int[] array = new int[numIndices];
		int target = tk2dTextGeomGen.SetTextMeshGeom(vertices, uvs, uv2, untintedColors, 0, geomData);
		if (!_fontInst.isPacked)
		{
			Color32 val = Color32.op_Implicit(data.color);
			Color32 val2 = Color32.op_Implicit((!data.useGradient) ? data.color : data.color2);
			for (int i = 0; i < numVertices; i++)
			{
				Color32 val3 = ((i % 4 >= 2) ? val2 : val);
				byte b = (byte)(untintedColors[i].r * val3.r / 255);
				byte b2 = (byte)(untintedColors[i].g * val3.g / 255);
				byte b3 = (byte)(untintedColors[i].b * val3.b / 255);
				byte b4 = (byte)(untintedColors[i].a * val3.a / 255);
				if (_fontInst.premultipliedAlpha)
				{
					b = (byte)(b * b4 / 255);
					b2 = (byte)(b2 * b4 / 255);
					b3 = (byte)(b3 * b4 / 255);
				}
				ref Color32 reference = ref colors[i];
				reference = new Color32(b, b2, b3, b4);
			}
		}
		else
		{
			colors = untintedColors;
		}
		tk2dTextGeomGen.SetTextMeshIndices(array, 0, 0, geomData, target);
		if ((Object)(object)mesh == (Object)null)
		{
			if ((Object)(object)meshFilter == (Object)null)
			{
				meshFilter = ((Component)this).GetComponent<MeshFilter>();
			}
			mesh = new Mesh();
			mesh.MarkDynamic();
			((Object)mesh).hideFlags = (HideFlags)52;
			meshFilter.mesh = mesh;
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = vertices;
		mesh.uv = uvs;
		if (font.textureGradients)
		{
			mesh.uv2 = uv2;
		}
		mesh.triangles = array;
		mesh.colors32 = colors;
		mesh.RecalculateBounds();
		mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(mesh.bounds, data.renderLayer);
		updateFlags = UpdateFlags.UpdateNone;
	}

	public void Commit()
	{
		tk2dUpdateManager.FlushQueues();
	}

	public void DoNotUse__CommitInternal()
	{
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		InitInstance();
		if ((Object)(object)_fontInst == (Object)null)
		{
			return;
		}
		_fontInst.InitDictionary();
		if ((updateFlags & UpdateFlags.UpdateBuffers) != 0 || (Object)(object)mesh == (Object)null)
		{
			Init();
		}
		else
		{
			if ((updateFlags & UpdateFlags.UpdateText) != 0)
			{
				FormatText();
				tk2dTextGeomGen.GeomData geomData = tk2dTextGeomGen.Data(data, _fontInst, _formattedText);
				int num = tk2dTextGeomGen.SetTextMeshGeom(vertices, uvs, uv2, untintedColors, 0, geomData);
				for (int i = num; i < data.maxChars; i++)
				{
					ref Vector3 reference = ref vertices[i * 4];
					ref Vector3 reference2 = ref vertices[i * 4 + 1];
					ref Vector3 reference3 = ref vertices[i * 4 + 2];
					ref Vector3 reference4 = ref vertices[i * 4 + 3];
					reference = (reference2 = (reference3 = (reference4 = Vector3.zero)));
				}
				mesh.vertices = vertices;
				mesh.uv = uvs;
				if (_fontInst.textureGradients)
				{
					mesh.uv2 = uv2;
				}
				if (_fontInst.isPacked)
				{
					colors = untintedColors;
					mesh.colors32 = colors;
				}
				if (data.inlineStyling)
				{
					SetNeedUpdate(UpdateFlags.UpdateColors);
				}
				mesh.RecalculateBounds();
				mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(mesh.bounds, data.renderLayer);
			}
			if (!font.isPacked && (updateFlags & UpdateFlags.UpdateColors) != 0)
			{
				Color32 val = Color32.op_Implicit(data.color);
				Color32 val2 = Color32.op_Implicit((!data.useGradient) ? data.color : data.color2);
				for (int j = 0; j < colors.Length; j++)
				{
					Color32 val3 = ((j % 4 >= 2) ? val2 : val);
					byte b = (byte)(untintedColors[j].r * val3.r / 255);
					byte b2 = (byte)(untintedColors[j].g * val3.g / 255);
					byte b3 = (byte)(untintedColors[j].b * val3.b / 255);
					byte b4 = (byte)(untintedColors[j].a * val3.a / 255);
					if (_fontInst.premultipliedAlpha)
					{
						b = (byte)(b * b4 / 255);
						b2 = (byte)(b2 * b4 / 255);
						b3 = (byte)(b3 * b4 / 255);
					}
					ref Color32 reference5 = ref colors[j];
					reference5 = new Color32(b, b2, b3, b4);
				}
				mesh.colors32 = colors;
			}
		}
		updateFlags = UpdateFlags.UpdateNone;
	}

	public void MakePixelPerfect()
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		tk2dCamera tk2dCamera2 = tk2dCamera.CameraForLayer(((Component)this).gameObject.layer);
		if ((Object)(object)tk2dCamera2 != (Object)null)
		{
			if (_fontInst.version < 1)
			{
				Debug.LogError((object)"Need to rebuild font.");
			}
			float distance = ((Component)this).transform.position.z - ((Component)tk2dCamera2).transform.position.z;
			float num2 = _fontInst.invOrthoSize * _fontInst.halfTargetHeight;
			num = tk2dCamera2.GetSizeAtDistance(distance) * num2;
		}
		else if (Object.op_Implicit((Object)(object)Camera.main))
		{
			if (Camera.main.orthographic)
			{
				num = Camera.main.orthographicSize;
			}
			else
			{
				float zdist = ((Component)this).transform.position.z - ((Component)Camera.main).transform.position.z;
				num = tk2dPixelPerfectHelper.CalculateScaleForPerspectiveCamera(Camera.main.fieldOfView, zdist);
			}
			num *= _fontInst.invOrthoSize;
		}
		scale = new Vector3(Mathf.Sign(scale.x) * num, Mathf.Sign(scale.y) * num, Mathf.Sign(scale.z) * num);
	}

	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		if ((Object)(object)data.font != (Object)null && (Object)(object)data.font.spriteCollection != (Object)null)
		{
			return (Object)(object)data.font.spriteCollection == (Object)(object)spriteCollection;
		}
		return true;
	}

	private void UpdateMaterial()
	{
		if ((Object)(object)((Component)this).GetComponent<Renderer>().sharedMaterial != (Object)(object)_fontInst.materialInst)
		{
			((Component)this).GetComponent<Renderer>().material = _fontInst.materialInst;
		}
	}

	public void ForceBuild()
	{
		if ((Object)(object)data.font != (Object)null)
		{
			_fontInst = data.font.inst;
			UpdateMaterial();
		}
		Init(force: true);
	}
}
