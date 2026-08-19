using System;
using System.Text.RegularExpressions;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using L10N;
using UnityEngine;

public class STTPreviewWidget : MonoBehaviour
{
	private enum STTPreviewTextType
	{
		Notice,
		Alert,
		Result
	}

	[SerializeField]
	private UISprite _speakButton;

	[SerializeField]
	private UISprite _sttWaveSprite1;

	[SerializeField]
	private UISprite _sttWaveSprite2;

	[SerializeField]
	private UILabel _sttPreviewLine;

	[SerializeField]
	private UISprite _sttPreviewBg;

	[SerializeField]
	private GameObject _sttAlertIcon;

	[SerializeField]
	private STTWaveWidget _sttWaveWidget;

	private bool _sttButtonPressed;

	private int _sttPreviewBgPadding;

	private Color _sttNoticeColor = new Color(1f, 0.85f, 0.36f);

	private Color _sttPreviewColor = new Color(0.77f, 0.72f, 0.58f);

	private int _sttPreviewLeftMargin;

	private int _sttPreviewRightMargin;

	private string _text;

	private bool _isInitLayout;

	private UIWidget _widget;

	private STTController _sttController;

	private float _sttPressedTime;

	private bool _sttTapMode;

	private Regex _filterWordRegex;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Awake()
	{
		_sttController = new STTController();
		_sttController.OnSTTProcessed += STTProcessed;
		_sttController.OnSTTPartialResult += STTPartialResult;
		STTController sttController = _sttController;
		sttController.OnEndOfSpeech = (STTController.OnEndOfSpeechDelegate)Delegate.Combine(sttController.OnEndOfSpeech, new STTController.OnEndOfSpeechDelegate(OnEndOfSpeech));
		_sttController.InstallEvent();
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_speakButton).gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPress_SpeakButton));
		InitLayout();
		((Component)this).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (_sttController != null)
		{
			_sttController.UninstallEvent();
		}
	}

	private void OnEnable()
	{
		SetLineText(_text);
	}

	private void InitLayout()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInitLayout)
		{
			_isInitLayout = true;
			_sttPreviewBg.SetAnchor((Transform)null);
			_sttPreviewBgPadding = _sttPreviewBg.height - _sttPreviewLine.fontSize;
			_sttPreviewLeftMargin = (int)(_sttPreviewLine.GetPosition(0f, 0f).x - Widget.localCorners[0].x);
			_sttPreviewRightMargin = (int)(Widget.localCorners[3].x - _sttPreviewLine.GetPosition(1f, 0f).x);
		}
	}

	private void STTInputDisabled()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		HOTween.Kill((object)_sttWaveSprite1);
		HOTween.Kill((object)_sttWaveSprite2);
		((Component)_sttWaveSprite1).gameObject.SetActive(false);
		((Component)_sttWaveSprite2).gameObject.SetActive(false);
		((Component)_sttWaveWidget).gameObject.SetActive(false);
		_speakButton.color = Color.gray;
		if (!_sttButtonPressed)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void SetSTTPreviewText(string text, STTPreviewTextType type)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		SetLineText(text);
		switch (type)
		{
		case STTPreviewTextType.Notice:
		case STTPreviewTextType.Alert:
			_sttPreviewLine.color = _sttNoticeColor;
			break;
		case STTPreviewTextType.Result:
			_sttPreviewLine.color = _sttPreviewColor;
			break;
		}
		if (type == STTPreviewTextType.Alert)
		{
			_sttAlertIcon.SetActive(true);
			((Component)_sttWaveWidget).gameObject.SetActive(false);
		}
		else
		{
			_sttAlertIcon.SetActive(false);
			((Component)_sttWaveWidget).gameObject.SetActive(true);
		}
		Widget.alpha = 1f;
		((Component)this).gameObject.SetActive(true);
	}

	private void RefreshSTTSpeakButtonColor()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!_sttButtonPressed)
		{
			if (_sttController.STTStarted)
			{
				_speakButton.color = Color.gray;
				return;
			}
			_speakButton.color = Color.white;
			_speakButton.alpha = 0.8f;
		}
	}

	private void ResetSTTButtonToNormal()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		HOTween.Kill((object)_speakButton);
		STTInputDisabled();
		RefreshSTTSpeakButtonColor();
		HOTween.To((object)_speakButton, 0.2f, new TweenParms().Prop("width", (object)28));
		HOTween.To((object)_speakButton, 0.2f, new TweenParms().Prop("height", (object)47));
	}

	private void OnPress_SpeakButton(GameObject go, bool press)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		HOTween.Kill((object)Widget);
		HOTween.Kill((object)((Component)Widget).transform);
		_sttButtonPressed = press;
		if (press)
		{
			if (!_sttController.STTStarted)
			{
				_sttPressedTime = Time.realtimeSinceStartup;
				_sttTapMode = false;
				_speakButton.width = 28;
				_speakButton.height = 47;
				HOTween.Kill((object)_speakButton);
				HOTween.To((object)_speakButton, 0.4f, new TweenParms().Prop("width", (object)76));
				HOTween.To((object)_speakButton, 0.4f, new TweenParms().Prop("height", (object)126));
				_sttWaveSprite1.alpha = 0f;
				_sttWaveSprite2.alpha = 0f;
				((Component)_sttWaveSprite1).gameObject.SetActive(true);
				((Component)_sttWaveSprite2).gameObject.SetActive(true);
				HOTween.To((object)_sttWaveSprite1, 1.2f, new TweenParms().Prop("alpha", (object)1f).Ease((EaseType)5).Loops(-1, (LoopType)1));
				HOTween.To((object)_sttWaveSprite2, 1.2f, new TweenParms().Prop("alpha", (object)1f).Delay(0.1f).Ease((EaseType)5)
					.Loops(-1, (LoopType)1));
				SetSTTPreviewText(T._("지금 목소리로 말하면 자동으로 채팅 메세지로 변환됩니다."), STTPreviewTextType.Notice);
				_sttController.StartSTT_IfCan();
			}
		}
		else if (Mathf.Abs(Time.realtimeSinceStartup - _sttPressedTime) < 1f)
		{
			_sttTapMode = true;
		}
		else
		{
			_sttController.StopSTT();
			STTMessageFire_IfCan();
			ResetSTTButtonToNormal();
		}
	}

	private string FilterBadWords(string input)
	{
		if (_filterWordRegex == null)
		{
			TextAsset val = Resources.Load<TextAsset>("filter_words");
			if ((Object)(object)val == (Object)null)
			{
				return input;
			}
			string[] value = val.text.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			_filterWordRegex = new Regex(string.Join("|", value));
		}
		return _filterWordRegex.Replace(input, "*");
	}

	private bool STTMessageFire_IfCan()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		if (!_sttButtonPressed && _sttController.GetLatestRecognizedText(out var result))
		{
			SetSTTPreviewText(result, STTPreviewTextType.Result);
			((Component)_sttWaveWidget).gameObject.SetActive(false);
			Vector3 val = ((Component)this).transform.localPosition - new Vector3(0f, 58f, 0f);
			HOTween.To((object)((Component)this).transform, 0.3f, new TweenParms().Prop("localPosition", (object)val).OnStepComplete((TweenCallback)delegate
			{
				((Component)this).gameObject.SetActive(false);
			}));
			GameSystem<SocialSystem>.Instance().Say(FilterBadWords(result), isDictation: true);
			_sttController.ClearResult();
			return true;
		}
		return false;
	}

	private void OnEndOfSpeech()
	{
		RefreshSTTSpeakButtonColor();
	}

	private void STTProcessed(bool success, string resultText, string confidence)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		RefreshSTTSpeakButtonColor();
		if (success)
		{
			Widget.alpha = 1f;
			if (_sttButtonPressed)
			{
				HOTween.To((object)Widget, 0.8f, new TweenParms().Prop("alpha", (object)0.3f).Loops(-1, (LoopType)1).Ease((EaseType)3));
				_sttPreviewLine.color = _sttPreviewColor;
				SetLineText(resultText);
				STTInputDisabled();
			}
			else
			{
				STTMessageFire_IfCan();
			}
		}
		else if (_sttButtonPressed)
		{
			STTInputDisabled();
			SetSTTPreviewText(T._("음성이 인식되지 않습니다. 버튼을 다시 눌러 말해주세요."), STTPreviewTextType.Alert);
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
		if (_sttTapMode)
		{
			ResetSTTButtonToNormal();
		}
	}

	private void STTPartialResult(string resultText)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_sttPreviewLine.color = _sttPreviewColor;
		SetLineText(resultText);
	}

	private void SetLineText(string text)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		_text = text;
		if (((Behaviour)this).enabled)
		{
			Vector3 pos = Widget.localCorners[0] + Vector3.right * (float)_sttPreviewLeftMargin;
			pos.y += (float)_sttPreviewBgPadding * 0.5f;
			_sttPreviewLine.SetPosition(pos, 0f, 0f);
			_sttPreviewLine.width = Widget.width - _sttPreviewLeftMargin - _sttPreviewRightMargin;
			_sttPreviewLine.text = text;
			int height = _sttPreviewLine.height + _sttPreviewBgPadding;
			_sttPreviewBg.width = Widget.width;
			_sttPreviewBg.height = height;
		}
	}
}
