using System;
using UnityEngine;

public class UITitleWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _closeButton;

	[SerializeField]
	private UIWidget _backButton;

	[SerializeField]
	private Transform _labelContainer;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Transform _labelNextContainer;

	private bool? _isShowBackButton;

	private Vector3? _baseContainerPos;

	private float? _labelNextMargin;

	private string _titleText;

	public event Action OnClose;

	public event Action OnBack;

	private void Start()
	{
		if ((Object)(object)_closeButton != (Object)null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(((Component)_closeButton).gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
			{
				if (this.OnClose != null)
				{
					this.OnClose();
				}
			});
		}
		if ((Object)(object)_backButton != (Object)null)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(((Component)_backButton).gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
			{
				if (this.OnBack != null)
				{
					this.OnBack();
				}
			});
		}
		if (!_isShowBackButton.HasValue)
		{
			ShowBackButton(isShow: false, instant: true);
		}
	}

	public void ResetTitle()
	{
		if (!((Object)(object)_titleLabel == (Object)null) && _titleText != null)
		{
			_titleLabel.text = _titleText;
		}
	}

	public void SetTitle(string text)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_titleLabel == (Object)null))
		{
			float? labelNextMargin = _labelNextMargin;
			if (!labelNextMargin.HasValue)
			{
				_labelNextMargin = ((Component)_labelNextContainer).transform.localPosition.x - _titleLabel.localCorners[3].x;
			}
			if (_titleText == null)
			{
				_titleText = _titleLabel.text;
			}
			_titleLabel.text = text;
			if ((Object)(object)_labelNextContainer != (Object)null)
			{
				Vector3 localPosition = ((Component)_labelNextContainer).transform.localPosition;
				localPosition.x = _titleLabel.localCorners[3].x + _labelNextMargin.Value;
				((Component)_labelNextContainer).transform.localPosition = localPosition;
				UIUtility.UpdateAnchors(_labelNextContainer);
			}
		}
	}

	public string GetTitle()
	{
		if ((Object)(object)_titleLabel == (Object)null)
		{
			return null;
		}
		return _titleLabel.text;
	}

	public void ShowBackButton(bool isShow, bool instant = false)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_backButton == (Object)null || (_isShowBackButton.HasValue && isShow == _isShowBackButton))
		{
			return;
		}
		_isShowBackButton = isShow;
		if (!_baseContainerPos.HasValue)
		{
			_baseContainerPos = ((!((Object)(object)_labelContainer == (Object)null)) ? ((Component)_labelContainer).transform.localPosition : Vector3.zero);
		}
		Vector3 val = _baseContainerPos.Value;
		float alpha = 0f;
		if (isShow)
		{
			val += Vector3.right * (float)_backButton.width;
			alpha = 1f;
		}
		if (instant)
		{
			_backButton.alpha = alpha;
			((Component)(object)_backButton).SetEnable<TweenAlpha>(enable: false);
			if ((Object)(object)_labelContainer != (Object)null)
			{
				((Component)_labelContainer).transform.localPosition = val;
				((Component)(object)_labelContainer).SetEnable<TweenPosition>(enable: false);
			}
		}
		else
		{
			TweenAlpha.Begin(((Component)_backButton).gameObject, 0.2f, alpha);
			if ((Object)(object)_labelContainer != (Object)null)
			{
				TweenPosition.Begin(((Component)_labelContainer).gameObject, 0.2f, val);
			}
		}
	}
}
