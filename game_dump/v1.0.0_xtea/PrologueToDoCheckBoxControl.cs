using UnityEngine;

public class PrologueToDoCheckBoxControl : MonoBehaviour
{
	[SerializeField]
	private GameObject _title;

	[SerializeField]
	private UISprite _titleIcon;

	[SerializeField]
	private UILabel _titleText;

	[SerializeField]
	private UITexture _portrait;

	[SerializeField]
	private GameObject _unchecked;

	[SerializeField]
	private GameObject _checked;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private UILabel _progressText;

	[SerializeField]
	private UISprite _back;

	[SerializeField]
	private Color _checkTextColor = Color.white;

	private int _currentProgress;

	private int _totalProgress;

	private Color _uncheckTextColor = Color.white;

	private TweenScale _scaleTweener;

	private TweenAlpha _alphaTweener;

	private string _realText;

	private TweenScale ScaleTweener
	{
		get
		{
			if ((Object)(object)_scaleTweener == (Object)null)
			{
				_scaleTweener = ((Component)this).GetComponent<TweenScale>();
			}
			return _scaleTweener;
		}
	}

	private TweenAlpha AlphaTweener
	{
		get
		{
			if ((Object)(object)_alphaTweener == (Object)null)
			{
				_alphaTweener = ((Component)this).GetComponent<TweenAlpha>();
			}
			return _alphaTweener;
		}
	}

	public bool Checked
	{
		get
		{
			return _checked.activeSelf;
		}
		set
		{
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			if (!Checked && value)
			{
				ShowUpdatedFeedBack();
			}
			_unchecked.SetActive(!value);
			_checked.SetActive(value);
			if (value)
			{
				_text.color = _checkTextColor;
				_progressText.text = string.Empty;
				_progressText.color = _checkTextColor;
			}
			else
			{
				_text.color = _uncheckTextColor;
				SetProgressText(feedback: true);
			}
			UpdateText();
		}
	}

	public bool TitleVisible
	{
		get
		{
			return _title.activeSelf;
		}
		private set
		{
			_title.SetActive(value);
		}
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_uncheckTextColor = _text.color;
	}

	private void OnEnable()
	{
		if ((Object)(object)ScaleTweener != (Object)null)
		{
			ScaleTweener.tweenFactor = 0f;
			ScaleTweener.PlayForward();
		}
		if ((Object)(object)AlphaTweener != (Object)null)
		{
			AlphaTweener.tweenFactor = 0f;
			AlphaTweener.PlayForward();
		}
	}

	public void SetTitle(string text, string icon)
	{
		TitleVisible = !string.IsNullOrEmpty(text);
		_titleIcon.spriteName = icon;
		UIUtility.ResizeToSquare(_titleIcon, _titleText.height);
		_titleText.text = text;
	}

	public void SetProgress(int current, int total)
	{
		int currentProgress = _currentProgress;
		int totalProgress = _totalProgress;
		_currentProgress = current;
		_totalProgress = total;
		SetProgressText(currentProgress != _currentProgress || totalProgress != _totalProgress);
	}

	public void SetText(string text)
	{
		_realText = text;
		UpdateText();
	}

	public void SetNotifyMode(Material material)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		_portrait.material = material;
		((Component)_portrait).gameObject.SetActive((Object)(object)material != (Object)null);
		_unchecked.SetActive(false);
		_checked.SetActive(false);
		Vector3 localPosition = ((Component)_text).transform.localPosition;
		localPosition.x = 64f;
		((Component)_text).transform.localPosition = localPosition;
	}

	public int GetHeight()
	{
		return _back.height;
	}

	private void SetProgressText(bool feedback)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		_progressText.text = ((_totalProgress <= 0) ? string.Empty : $"{_currentProgress} / {_totalProgress}");
		_progressText.color = _uncheckTextColor;
		if (feedback)
		{
			ShowUpdatedFeedBack();
		}
	}

	private void UpdateText()
	{
		_text.text = ((!Checked) ? _realText : ("[s]" + _realText));
		int height = _text.height + 22;
		_back.height = height;
	}

	private void ShowUpdatedFeedBack()
	{
		if ((Object)(object)ScaleTweener != (Object)null)
		{
			ScaleTweener.ResetToBeginning();
			ScaleTweener.PlayForward();
		}
	}
}
