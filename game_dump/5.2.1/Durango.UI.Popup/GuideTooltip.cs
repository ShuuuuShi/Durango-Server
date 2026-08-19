using System.Linq;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI.Popup;

public class GuideTooltip : TooltipBase
{
	public static float SpotlightGuideUnskippableTime = 2f;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private UISpriteLabel _commentLabel;

	[SerializeField]
	private UITexture _arrowTexture;

	[SerializeField]
	private UIPanel _nguiOverPanel;

	[SerializeField]
	private int _defaultCommentWidth;

	[SerializeField]
	private float _pad = 15f;

	private string _title;

	private string _comment;

	private UIWidget[] _comps;

	private UIPanel[] _panelCache;

	private ICoroutineBinder _lockSkipProcess;

	private bool _hideWhenTouchDefault;

	private bool _hideWhenTouchModalBgDefault;

	public int CommentWidth { get; set; }

	public void Set(string title, string comment)
	{
		_title = title;
		_comment = comment;
	}

	protected override void OnAwake()
	{
		SoundType = UISound.GroupType.NoSound;
		_hideWhenTouchDefault = base.HideWhenTouch;
		_hideWhenTouchModalBgDefault = base.HideWhenTouchModalBg;
	}

	protected override void OnHide()
	{
		base.OnHide();
		CommentWidth = 0;
		int i = 0;
		for (int size = KUtility.GetSize(_comps); i < size; i++)
		{
			if (_panelCache != null)
			{
				UIPanel drawPanel = _panelCache.Get(i);
				_comps[i].DrawPanel = drawPanel;
			}
		}
	}

	protected override void FillData()
	{
		if (string.IsNullOrEmpty(_title))
		{
			_titleLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_titleLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
			_titleLabel.text = _title;
			_titleLabel.gameObject.SetActive(value: true);
		}
		_commentLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
		_commentLabel.text = _comment;
		Vector2 vector = ((!string.IsNullOrEmpty(_commentLabel.text)) ? _commentLabel.printedSize : Vector2.zero);
		int num = ((CommentWidth <= 0) ? _defaultCommentWidth : CommentWidth);
		Vector2 printedSize = _titleLabel.printedSize;
		int num2 = (int)((float)(UIManager.SafeWidth - 40) - printedSize.x);
		num = ((num <= 0) ? num2 : Mathf.Min(num, num2));
		if (num > 0 && vector.x > (float)num)
		{
			_commentLabel.overflowMethod = UILabel.Overflow.ResizeHeight;
			_commentLabel.width = num;
		}
	}

	protected override void UpdateLayout()
	{
		Vector2 printedSize = _titleLabel.printedSize;
		Vector2 printedSize2 = _commentLabel.printedSize;
		UITexture arrowTexture = _arrowTexture;
		NGUIText.Alignment alignment = ((!(base.TargetPos.x > 0f)) ? NGUIText.Alignment.Left : NGUIText.Alignment.Right);
		_titleLabel.alignment = alignment;
		_commentLabel.alignment = alignment;
		if (!_titleLabel.gameObject.activeSelf)
		{
			_commentLabel.transform.localPosition = Vector3.zero;
			return;
		}
		if (base.TargetPos.x > 0f)
		{
			if (base.TargetPos.y > 0f)
			{
				arrowTexture.flip = UIBasicSprite.Flip.Vertically;
				arrowTexture.rotate = UIBasicSprite.Rotate.Radial180;
				Vector3 vector = new Vector3(base.Widget.width - arrowTexture.width, 0f, 0f);
				arrowTexture.transform.localPosition = vector + Vector3.down * 8f;
				Vector3 localPosition = vector + new Vector3(0f - printedSize.x - _pad, printedSize.y - (float)arrowTexture.height, 0f);
				_titleLabel.transform.localPosition = localPosition;
				Vector3 localPosition2 = vector + new Vector3(0f - printedSize2.x - _pad, 0f - printedSize.y - printedSize2.y - _pad, 0f);
				_commentLabel.transform.localPosition = localPosition2;
			}
			else
			{
				arrowTexture.flip = UIBasicSprite.Flip.Nothing;
				arrowTexture.rotate = UIBasicSprite.Rotate.Radial180;
				Vector3 vector2 = new Vector3(base.Widget.width - arrowTexture.width, arrowTexture.height, 0f);
				arrowTexture.transform.localPosition = vector2 + Vector3.down * 8f;
				Vector3 localPosition3 = vector2 + new Vector3(0f - printedSize.x - _pad, printedSize.y, 0f);
				_titleLabel.transform.localPosition = localPosition3;
				Vector3 localPosition4 = vector2 + new Vector3(0f - printedSize2.x - _pad, printedSize2.y - _pad, 0f);
				_commentLabel.transform.localPosition = localPosition4;
			}
		}
		else if (base.TargetPos.y > 0f)
		{
			arrowTexture.flip = UIBasicSprite.Flip.Nothing;
			arrowTexture.rotate = UIBasicSprite.Rotate.Nothing;
			arrowTexture.transform.localPosition = Vector3.zero;
			Vector3 vector3 = new Vector3((float)arrowTexture.width + _pad, printedSize.y - (float)arrowTexture.height);
			_titleLabel.transform.localPosition = vector3 + Vector3.up * 8f;
			Vector3 vector4 = -Vector3.up * (printedSize.y + _pad);
			_commentLabel.transform.localPosition = vector3 + vector4 + Vector3.up * 8f;
		}
		else
		{
			arrowTexture.flip = UIBasicSprite.Flip.Vertically;
			arrowTexture.rotate = UIBasicSprite.Rotate.Nothing;
			arrowTexture.transform.localPosition = Vector3.zero;
			Vector3 vector5 = new Vector3((float)arrowTexture.width + _pad, printedSize.y, 0f);
			_titleLabel.transform.localPosition = vector5 + Vector3.up * 8f;
			Vector3 vector6 = new Vector3((float)arrowTexture.width + _pad, 0f - _pad, 0f);
			_commentLabel.transform.localPosition = vector6 + Vector3.up * 8f;
		}
		float num = Mathf.Min(Mathf.Min(_titleLabel.transform.localPosition.x - (float)_titleLabel.width * _titleLabel.pivotOffset.x, _commentLabel.transform.localPosition.x - (float)_commentLabel.width * _commentLabel.pivotOffset.x), _arrowTexture.transform.localPosition.x - (float)_arrowTexture.width * _arrowTexture.pivotOffset.x);
		float num2 = Mathf.Max(Mathf.Max(_titleLabel.transform.localPosition.x - (float)_titleLabel.width * _titleLabel.pivotOffset.x + (float)_titleLabel.width, _commentLabel.transform.localPosition.x - (float)_commentLabel.width * _commentLabel.pivotOffset.x + (float)_commentLabel.width), _arrowTexture.transform.localPosition.x - (float)_arrowTexture.width * _arrowTexture.pivotOffset.x + (float)_arrowTexture.width);
		float num3 = Mathf.Min(Mathf.Min(0f - (_titleLabel.transform.localPosition.y + (float)_titleLabel.height * (1f - _titleLabel.pivotOffset.y)), 0f - (_commentLabel.transform.localPosition.y - (float)_commentLabel.height * (1f - _commentLabel.pivotOffset.y))), 0f - (_arrowTexture.transform.localPosition.y - (float)_arrowTexture.height * (1f - _arrowTexture.pivotOffset.y)));
		float num4 = Mathf.Max(Mathf.Max(0f - (_titleLabel.transform.localPosition.y + (float)_titleLabel.height * (1f - _titleLabel.pivotOffset.y)) + (float)_titleLabel.height, 0f - (_commentLabel.transform.localPosition.y - (float)_commentLabel.height * (1f - _commentLabel.pivotOffset.y)) + (float)_commentLabel.height), 0f - (_arrowTexture.transform.localPosition.y - (float)_arrowTexture.height * (1f - _arrowTexture.pivotOffset.y)) + (float)_arrowTexture.height);
		base.Widget.SetDimensions((int)(num + num2), (int)(num3 + num4));
	}

	protected override void UpdatePosition()
	{
		if (base.TargetPos.x > 0f)
		{
			base.Widget.transform.localPosition = base.TargetPos - new Vector3(base.Widget.localSize.x, 0f, 0f);
		}
		else
		{
			base.UpdatePosition();
		}
	}

	public void ModifyDrawPanel(Transform target)
	{
		_comps = target.GetComponentsInChildren<UIWidget>();
		_panelCache = _comps.Select((UIWidget elem) => elem.DrawPanel).ToArray();
		for (int i = 0; i < _comps.Length; i++)
		{
			UIWidget uIWidget = _comps[i];
			_panelCache[i] = uIWidget.DrawPanel;
			uIWidget.DrawPanel = _nguiOverPanel;
		}
	}

	public void LockSkip(float lockTime)
	{
		bool flag2 = (base.HideWhenTouch = false);
		bool hideWhenTouchModalBg = flag2;
		base.HideWhenTouchModalBg = hideWhenTouchModalBg;
		this.StartCoroutine(ref _lockSkipProcess, KUtility.CoDelayedCall(RestoreHideWhenTouch, lockTime));
	}

	protected virtual void RestoreHideWhenTouch()
	{
		base.HideWhenTouch = _hideWhenTouchDefault;
		base.HideWhenTouchModalBg = _hideWhenTouchModalBgDefault;
	}
}
