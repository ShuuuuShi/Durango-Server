using UnityEngine;

namespace Durango.UI.Prologue;

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

	[SerializeField]
	private int _itemOffsetY;

	private int _currentProgress;

	private int _totalProgress;

	private int _height;

	private Color _uncheckTextColor = Color.white;

	private TweenScale _scaleTweener;

	private TweenAlpha _alphaTweener;

	private string _realText;

	private TweenScale ScaleTweener
	{
		get
		{
			if (_scaleTweener == null)
			{
				_scaleTweener = GetComponent<TweenScale>();
			}
			return _scaleTweener;
		}
	}

	private TweenAlpha AlphaTweener
	{
		get
		{
			if (_alphaTweener == null)
			{
				_alphaTweener = GetComponent<TweenAlpha>();
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
		_uncheckTextColor = _text.color;
	}

	private void OnEnable()
	{
		if (ScaleTweener != null)
		{
			ScaleTweener.tweenFactor = 0f;
			ScaleTweener.PlayForward();
		}
		if (AlphaTweener != null)
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
		if (_portrait != null)
		{
			_portrait.material = material;
			_portrait.gameObject.SetActive(material != null);
		}
		_unchecked.SetActive(value: false);
		_checked.SetActive(value: false);
		Vector3 localPosition = _text.transform.localPosition;
		localPosition.x = 64f;
		_text.transform.localPosition = localPosition;
	}

	public int GetHeight()
	{
		return _height;
	}

	private void SetProgressText(bool feedback)
	{
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
		_height = _text.height + _itemOffsetY;
		if (_back != null)
		{
			_back.height = _height;
		}
	}

	private void ShowUpdatedFeedBack()
	{
		if (ScaleTweener != null)
		{
			ScaleTweener.ResetToBeginning();
			ScaleTweener.PlayForward();
		}
	}
}
