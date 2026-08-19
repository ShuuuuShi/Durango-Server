using System;
using UnityEngine;

public class LegacyTextComparer : UIWidget
{
	[Serializable]
	public struct Options
	{
		public int size;

		public Color color;

		public FontStyle style;

		public int width;

		public int height;

		public bool encoding;

		public int maxLines;

		public int spacingX;

		public int spacingY;

		public NGUIText.Alignment alignment;

		public int minSize;

		public bool useEllipsis;
	}

	[SerializeField]
	private UIFont _font;

	[SerializeField]
	[TextArea]
	private string _text = "Test Text";

	[SerializeField]
	private Options _options = new Options
	{
		size = 24,
		color = Color.white,
		style = FontStyle.Normal,
		width = 100,
		height = 100,
		encoding = false,
		maxLines = 0,
		spacingX = 0,
		spacingY = 6,
		alignment = NGUIText.Alignment.Left,
		minSize = 2,
		useEllipsis = false
	};

	[SerializeField]
	private bool _isLegacy;

	[SerializeField]
	private Vector2 _printedSize;

	[SerializeField]
	private Vector2 _legacyPrintedSize;

	private BetterList<Vector3> _verts = new BetterList<Vector3>();

	private BetterList<Vector2> _uvs = new BetterList<Vector2>();

	private BetterList<Color> _cols = new BetterList<Color>();

	private BetterList<Vector3> _legacyVerts = new BetterList<Vector3>();

	private BetterList<Vector2> _legacyUvs = new BetterList<Vector2>();

	private BetterList<Color> _legacyCols = new BetterList<Color>();

	public override Material material => (!(_font == null)) ? _font.material : null;

	protected override void OnInit()
	{
		base.OnInit();
		Refresh();
	}

	private void Refresh()
	{
		_verts.Clear();
		_uvs.Clear();
		_cols.Clear();
		_legacyVerts.Clear();
		_legacyUvs.Clear();
		_legacyCols.Clear();
		Test(_font, _text, _options, out _printedSize, _verts, _uvs, _cols, out _legacyPrintedSize, _legacyVerts, _legacyUvs, _legacyCols);
		MarkAsChanged();
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		if (!_isLegacy)
		{
			if (_verts.size > 0)
			{
				arguments.verts.AddRange(_verts.buffer, _verts.size);
			}
			if (_uvs.size > 0)
			{
				arguments.uvs.AddRange(_uvs.buffer, _uvs.size);
			}
			if (_cols.size > 0)
			{
				arguments.cols.AddRange(_cols.buffer, _cols.size);
			}
		}
		if (_isLegacy)
		{
			if (_legacyVerts.size > 0)
			{
				arguments.verts.AddRange(_legacyVerts.buffer, _legacyVerts.size);
			}
			if (_legacyUvs.size > 0)
			{
				arguments.uvs.AddRange(_legacyUvs.buffer, _legacyUvs.size);
			}
			if (_legacyCols.size > 0)
			{
				arguments.cols.AddRange(_legacyCols.buffer, _legacyCols.size);
			}
		}
	}

	public static void Test(UIFont font, string text, Options options, out Vector2 printedSize, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, out Vector2 legacyPrintedSize, BetterList<Vector3> legacyVerts, BetterList<Vector2> legacyUvs, BetterList<Color> legacyCols)
	{
		using TextBuilder textBuilder = TextBuilder.Pop();
		textBuilder.Font = font.dynamicFont;
		textBuilder.FontSize = options.size;
		textBuilder.FontStyle = options.style;
		textBuilder.Width = ((options.width <= 0) ? 1000000 : options.width);
		textBuilder.Height = ((options.height <= 0) ? 1000000 : options.height);
		textBuilder.Encoding = options.encoding;
		textBuilder.MaxLines = options.maxLines;
		textBuilder.SpacingX = options.spacingX;
		textBuilder.SpacingY = options.spacingY;
		switch (options.alignment)
		{
		case NGUIText.Alignment.Center:
			textBuilder.Alignment = 0.5f;
			break;
		case NGUIText.Alignment.Right:
			textBuilder.Alignment = 1f;
			break;
		default:
			textBuilder.Alignment = 0f;
			break;
		}
		textBuilder.Update(request: true);
		LegacyTextBuilder.dynamicFont = font.dynamicFont;
		LegacyTextBuilder.fontSize = options.size;
		LegacyTextBuilder.tint = options.color;
		LegacyTextBuilder.fontStyle = options.style;
		LegacyTextBuilder.rectWidth = (LegacyTextBuilder.regionWidth = ((options.width <= 0) ? 1000000 : options.width));
		LegacyTextBuilder.rectHeight = (LegacyTextBuilder.regionHeight = ((options.height <= 0) ? 1000000 : options.height));
		LegacyTextBuilder.encoding = options.encoding;
		LegacyTextBuilder.maxLines = options.maxLines;
		LegacyTextBuilder.spacingX = options.spacingX;
		LegacyTextBuilder.spacingY = options.spacingY;
		LegacyTextBuilder.alignment = options.alignment;
		LegacyTextBuilder.fontScale = 1f;
		LegacyTextBuilder.Update(request: true);
		TextBuilder.TextTokens textTokens = new TextBuilder.TextTokens();
		TextBuilder.TextTokens textTokens2 = new TextBuilder.TextTokens();
		textBuilder.ParseText(text, textTokens, null);
		int num = textBuilder.ProcessText(textTokens, textTokens2, out printedSize, options.minSize, options.useEllipsis);
		textBuilder.FontScale = (float)num / (float)options.size;
		textBuilder.Build(textTokens2, options.color, (options.width <= 0) ? ((int)printedSize.x) : options.width, verts, uvs, cols);
		string finalText = null;
		for (int num2 = options.size; num2 >= options.minSize; num2--)
		{
			LegacyTextBuilder.fontScale = (float)num2 / (float)options.size;
			if (LegacyTextBuilder.WrapText(text, out finalText, keepCharCount: false, wrapLineColors: false, options.useEllipsis))
			{
				break;
			}
		}
		legacyPrintedSize = LegacyTextBuilder.CalculatePrintedSize(finalText);
		LegacyTextBuilder.rectWidth = ((options.width <= 0) ? ((int)legacyPrintedSize.x) : options.width);
		LegacyTextBuilder.rectHeight = ((options.height <= 0) ? ((int)legacyPrintedSize.y) : options.height);
		LegacyTextBuilder.Print(finalText, legacyVerts, legacyUvs, legacyCols);
	}
}
