using UnityEngine;

public class GuideTooltip : TooltipBase
{
	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private UISpriteLabel _commentLabel;

	[SerializeField]
	private int _defaultCommentWidth;

	[SerializeField]
	private float _textPadding;

	[SerializeField]
	private float _titleCommentSpacing;

	[SerializeField]
	private float _titlePadding;

	private string _title;

	private string _comment;

	public int CommentWidth { get; set; }

	public void Set(string title, string comment)
	{
		_title = title;
		_comment = comment;
	}

	protected override void OnFinish()
	{
		base.OnFinish();
		CommentWidth = 0;
	}

	protected override void FillData()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(_title))
		{
			((Component)_titleLabel).gameObject.SetActive(false);
		}
		else
		{
			_titleLabel.Label.overflowMethod = UILabel.Overflow.ResizeFreely;
			_titleLabel.text = _title;
			((Component)_titleLabel).gameObject.SetActive(true);
		}
		_commentLabel.Label.overflowMethod = UILabel.Overflow.ResizeFreely;
		_commentLabel.text = _comment;
		Vector2 val = ((!string.IsNullOrEmpty(_commentLabel.text)) ? (_commentLabel.Label.printedSize - new Vector2((float)_commentLabel.Label.spacingX, (float)_commentLabel.Label.spacingY)) : Vector2.zero);
		int num = ((CommentWidth <= 0) ? _defaultCommentWidth : CommentWidth);
		Vector2 val2 = _titleLabel.Label.printedSize - new Vector2((float)_titleLabel.Label.spacingX, (float)_titleLabel.Label.spacingY);
		int num2 = (int)((float)(UIManager.ScreenWidth - 40) - val2.x);
		num = ((num <= 0) ? num2 : Mathf.Min(num, num2));
		if (num > 0 && val.x > (float)num)
		{
			_commentLabel.Label.overflowMethod = UILabel.Overflow.ResizeHeight;
			_commentLabel.Label.width = num;
		}
	}

	protected override void UpdateLayout()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = _titleLabel.Label.printedSize - new Vector2((float)_titleLabel.Label.spacingX, (float)_titleLabel.Label.spacingY);
		Vector2 val2 = _commentLabel.Label.printedSize - new Vector2((float)_commentLabel.Label.spacingX, (float)_commentLabel.Label.spacingY);
		val += Vector2.one * _titlePadding * 2f;
		Vector2 val3 = default(Vector2);
		val3.x = val.x + val2.x + _titleCommentSpacing + _textPadding * 2f;
		val3.y = Mathf.Max(val.y, val2.y) + _textPadding * 2f;
		base.Widget.width = (int)val3.x;
		base.Widget.height = (int)val3.y;
		NGUIText.Alignment alignment = ((!(base.TargetPos.x > 0f)) ? NGUIText.Alignment.Left : NGUIText.Alignment.Right);
		_titleLabel.Label.alignment = alignment;
		_commentLabel.Label.alignment = alignment;
		Vector3 val4 = (Vector3.right + Vector3.down) * _textPadding;
		if (((Component)_titleLabel).gameObject.activeSelf)
		{
			if (base.TargetPos.x > 0f)
			{
				((Component)_commentLabel).transform.localPosition = val4;
				((Component)_titleLabel).transform.localPosition = val4 + Vector3.right * (val2.x + _titleCommentSpacing) + (Vector3.right + Vector3.down) * _titlePadding;
			}
			else
			{
				((Component)_titleLabel).transform.localPosition = val4 + (Vector3.right + Vector3.down) * _titlePadding;
				((Component)_commentLabel).transform.localPosition = val4 + Vector3.right * (val.x + _titleCommentSpacing);
			}
		}
		else
		{
			((Component)_commentLabel).transform.localPosition = val4;
		}
	}
}
