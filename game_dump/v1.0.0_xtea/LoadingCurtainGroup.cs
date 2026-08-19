using System;
using System.Collections;
using System.Collections.Generic;
using EncyclopediaData;
using EnvironmentData;
using ExploreData;
using L10N;
using TimerData;
using UnityEngine;

public class LoadingCurtainGroup : MonoBehaviour
{
	[Serializable]
	private struct RegionInfo
	{
		public UIWidget Parent;

		public Transform Icon;

		public UILabel Lv;

		public UILabel Region;

		public UILabel Year;

		public UISpriteLabel Fatigue;
	}

	[Serializable]
	private struct FictionInfo
	{
		public UIWidget Parent;

		public UILabel Contents;

		public void Set(string text)
		{
			Contents.text = text;
		}
	}

	[Serializable]
	private struct TooltipInfo
	{
		public UIWidget Parent;

		public UILabel Label;

		public UILabel Contents;

		public void Set(string title, string text)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			Label.text = title;
			Contents.text = text;
			Vector2 printedSize = Contents.printedSize;
			Vector3 localPosition = ((Component)Label).transform.localPosition;
			localPosition.y = printedSize.y * 0.5f;
			((Component)Label).transform.localPosition = localPosition;
		}
	}

	[Serializable]
	[EnumType(typeof(TerrainEmblem))]
	private class AnimationEmblemLink : EnumKeyList
	{
		[SerializeField]
		private List<GameObject> _values;

		public GameObject Get(string emblem)
		{
			return Get(EmblemToEnum(emblem));
		}

		private GameObject Get(TerrainEmblem emblem)
		{
			int index = IndexOf((int)emblem);
			return _values[index];
		}

		private static TerrainEmblem EmblemToEnum(string emblem)
		{
			string[] array = emblem.Split('_');
			if (array.Length < 2)
			{
				return TerrainEmblem.Unknown;
			}
			try
			{
				return (TerrainEmblem)(int)Enum.Parse(typeof(TerrainEmblem), array[1], ignoreCase: true);
			}
			catch (Exception)
			{
				return TerrainEmblem.Unknown;
			}
		}
	}

	private enum TerrainEmblem
	{
		Ancora,
		Normal,
		Cold,
		Hot,
		Grassland,
		Unknown
	}

	private static int _showCount;

	[HideInInspector]
	public List<EventDelegate> ShowRegionInfoFinished = new List<EventDelegate>();

	[HideInInspector]
	public List<EventDelegate> FadeOutStarted = new List<EventDelegate>();

	[HideInInspector]
	public List<EventDelegate> FadeOutFinished = new List<EventDelegate>();

	[SerializeField]
	private AnimationEmblemLink _animationEmblems;

	[SerializeField]
	private GameObject _loadingIcon;

	[SerializeField]
	private UIWidget _mainContainer;

	[SerializeField]
	private UITexture _backTexture;

	[SerializeField]
	private UITexture _phaseChangedTexture;

	[SerializeField]
	private RegionInfo _regionInfo;

	[SerializeField]
	private FictionInfo _fictionInfo;

	[SerializeField]
	private TooltipInfo _tooltipInfo;

	[SerializeField]
	private SimpleContainer _statusBar;

	[SerializeField]
	private UILabel _continueLabel;

	[SerializeField]
	private UIWidget _downloadWarning;

	[SerializeField]
	private float _regionFadeInDelay;

	[SerializeField]
	private float _regionFadeInDuration;

	[SerializeField]
	private float _regionFadeOutDelay;

	[SerializeField]
	private float _regionFadeOutDuration;

	[SerializeField]
	private float _phaseChangedFadeOutDuration;

	private bool _isRegionFading;

	private bool _isFinished;

	private float _backFadeOutAlpha;

	private float _backFadeOutDuration;

	private float _phaseChangedFadeOutAlpha;

	private UIPanel _panel;

	private bool _hasReadableText;

	private bool _isTap;

	private Texture2D _lastScreenTexture;

	public bool IsVisible
	{
		get
		{
			return ((Component)this).gameObject.activeSelf;
		}
		private set
		{
			((Component)this).gameObject.SetActive(value);
		}
	}

	public bool IsFadeoutStarted { get; private set; }

	public static bool IsFirstPlayAfterCreatePlayer { get; set; }

	private void Awake()
	{
		Show();
	}

	private IEnumerator Start()
	{
		yield return null;
		if (GameManager.IsPrologueMode || IsFirstPlayAfterCreatePlayer)
		{
			_mainContainer.alpha = 1f;
		}
		else if ((Object)(object)_backTexture.mainTexture == (Object)(object)Texture2D.whiteTexture)
		{
			TweenColor.Begin(((Component)_backTexture).gameObject, 1f, PresetColor.LoadingColor);
			TweenAlpha.Begin(((Component)_mainContainer).gameObject, 0.5f, 1f).delay = 0.5f;
		}
	}

	private void OnEnable()
	{
		if (!IsFirstPlayAfterCreatePlayer)
		{
			GameSystem<FatigueSystem>.Instance().FatigueUpdated += OnUpdateFatigue;
		}
	}

	private void OnDisable()
	{
		GameSystem<FatigueSystem>.Instance().FatigueUpdated -= OnUpdateFatigue;
	}

	private void Terrain_LoadingChunksFinished()
	{
		KSingleton<TerrainA6>.Instance().LoadingChunksFinished -= Terrain_LoadingChunksFinished;
		EndLoading();
	}

	private void OnUpdateFatigue()
	{
		if (_isFinished || GameManager.IsPrologueMode)
		{
			return;
		}
		Fatigue fatigue = GameSystem<FatigueSystem>.Instance().Fatigue;
		Fatigue.State state = fatigue.GetState();
		string text2;
		if (state == Fatigue.State.Normal)
		{
			float num = 0f;
			if (fatigue.Velocity > 0.01f)
			{
				num = fatigue.Remain(fatigue.Max);
			}
			if (num > 0f)
			{
				string text = TimerSystem.TimeToString(num, TimePeriod.Min, 1);
				text2 = T._("{0} 활동 가능", text);
			}
			else
			{
				text2 = T._("안전");
			}
		}
		else
		{
			text2 = state.GetName();
		}
		((Component)((Component)_regionInfo.Fatigue).transform.parent).gameObject.SetActive(true);
		_regionInfo.Fatigue.text = text2;
	}

	public void EndLoading()
	{
		if (!_isFinished)
		{
			_isFinished = true;
			((MonoBehaviour)this).StartCoroutine(CoBackFadeOut());
		}
	}

	private IEnumerator CoShowDownloadWarning()
	{
		_loadingIcon.SetActive(false);
		((Component)_downloadWarning).gameObject.SetActive(true);
		yield return (object)new WaitForSeconds(2f);
		float remainTime = 3f;
		while (remainTime > 0f && !Input.GetMouseButtonDown(0))
		{
			remainTime -= Time.deltaTime;
			yield return null;
		}
		while (_downloadWarning.alpha > 0f)
		{
			_downloadWarning.alpha -= Time.deltaTime;
			yield return null;
		}
		((Component)_downloadWarning).gameObject.SetActive(false);
		_loadingIcon.SetActive(true);
		((MonoBehaviour)this).StartCoroutine(CoShowRegionInfo());
	}

	private IEnumerator CoShowRegionInfo()
	{
		_hasReadableText = !GameManager.IsPrologueMode && !IsFirstPlayAfterCreatePlayer;
		UpdateToolTipInfo();
		_isRegionFading = true;
		float maxUpdateRegionInfoTime = _regionFadeInDelay + _regionFadeInDuration;
		yield return (object)new WaitForSeconds(_regionFadeInDelay);
		while (!TryUpdateRegionInfo() && maxUpdateRegionInfoTime >= 0f)
		{
			maxUpdateRegionInfoTime -= Time.deltaTime;
			yield return null;
		}
		float fadeInAlpha = 0f;
		while (!(_regionFadeInDuration <= 0f))
		{
			fadeInAlpha += Time.deltaTime / _regionFadeInDuration;
			if (fadeInAlpha >= 1f)
			{
				break;
			}
			_regionInfo.Parent.alpha = fadeInAlpha;
			yield return null;
		}
		yield return (object)new WaitForSeconds(_regionFadeOutDelay);
		if (_hasReadableText)
		{
			((Component)_continueLabel).gameObject.SetActive(true);
		}
		TweenAlpha.Begin(_loadingIcon, 0.5f, 0f);
		while (_hasReadableText && !_isTap)
		{
			yield return null;
		}
		((Behaviour)((Component)_continueLabel).GetComponent<TweenAlpha>()).enabled = false;
		float fadeOutAlpha = 1f;
		while (!(_regionFadeOutDuration <= 0f))
		{
			fadeOutAlpha -= Time.deltaTime / _regionFadeOutDuration;
			_regionInfo.Parent.alpha = fadeOutAlpha;
			_fictionInfo.Parent.alpha = fadeOutAlpha;
			_tooltipInfo.Parent.alpha = fadeOutAlpha;
			_continueLabel.alpha = fadeOutAlpha;
			if (fadeOutAlpha <= 0f)
			{
				break;
			}
			yield return null;
		}
		_isRegionFading = false;
		FinishShowRegionInfo();
		float forceEndTime = 15f;
		while (forceEndTime > 0f)
		{
			if (_isFinished)
			{
				yield break;
			}
			forceEndTime -= Time.deltaTime;
			yield return null;
		}
		EndLoading();
	}

	private void UpdateToolTipInfo()
	{
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		if (_hasReadableText)
		{
			if (_showCount == 0)
			{
				((Component)_tooltipInfo.Parent).gameObject.SetActive(false);
				((Component)_fictionInfo.Parent).gameObject.SetActive(true);
				_fictionInfo.Parent.alpha = 1f;
				int num = EncyclopediaSystem.RandomMemoGet(MemoType.Fiction);
				string text = ((num != -1) ? EncyclopediaSystem.GetMemoFullText(MemoType.Fiction, num) : string.Empty);
				_fictionInfo.Set(text);
			}
			else
			{
				((Component)_fictionInfo.Parent).gameObject.SetActive(false);
				((Component)_tooltipInfo.Parent).gameObject.SetActive(true);
				_tooltipInfo.Parent.alpha = 1f;
				int num2 = EncyclopediaSystem.RandomMemoGet(MemoType.Tooltip);
				string title = ((num2 != -1) ? EncyclopediaSystem.GetMemoTitle(MemoType.Tooltip, num2) : string.Empty);
				string text2 = ((num2 != -1) ? EncyclopediaSystem.GetMemoText(MemoType.Tooltip, num2) : string.Empty);
				_tooltipInfo.Set(title, text2);
			}
		}
		else
		{
			((Component)_regionInfo.Parent).transform.localPosition = Vector3.zero;
			((Component)_fictionInfo.Parent).gameObject.SetActive(false);
			((Component)_tooltipInfo.Parent).gameObject.SetActive(false);
		}
	}

	private void FinishShowRegionInfo()
	{
		((Component)_regionInfo.Parent).gameObject.SetActive(false);
		((Component)_fictionInfo.Parent).gameObject.SetActive(false);
		((Component)_tooltipInfo.Parent).gameObject.SetActive(false);
		((Component)_continueLabel).gameObject.SetActive(false);
		EventDelegate.Execute(ShowRegionInfoFinished);
	}

	private bool TryUpdateRegionInfo()
	{
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.IsPrologueMode || IsFirstPlayAfterCreatePlayer)
		{
			((Component)_regionInfo.Parent).gameObject.SetActive(true);
			((Component)_regionInfo.Icon).gameObject.SetActive(false);
			((Component)_regionInfo.Fatigue).gameObject.SetActive(false);
			((Component)_regionInfo.Lv).gameObject.SetActive(false);
			_regionInfo.Region.pivot = UIWidget.Pivot.Center;
			_regionInfo.Region.text = ((!IsFirstPlayAfterCreatePlayer) ? T._("지구") : T._("미지의 땅"));
			_regionInfo.Year.text = ConditionalText.Format((!IsFirstPlayAfterCreatePlayer) ? T._("서기 {year}년") : T._("연도 불명"));
			IsFirstPlayAfterCreatePlayer = false;
		}
		else
		{
			Region region = KSingleton<GameManager>.Instance().Region;
			if (region == null || region.Template == null)
			{
				((Component)_regionInfo.Parent).gameObject.SetActive(false);
				return false;
			}
			((Component)_regionInfo.Parent).gameObject.SetActive(true);
			((Component)_regionInfo.Year).gameObject.SetActive(false);
			for (int num = _regionInfo.Icon.childCount - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)((Component)_regionInfo.Icon.GetChild(num)).gameObject);
			}
			GameObject val = _animationEmblems.Get(region.GetEmblem());
			if ((Object)(object)val != (Object)null)
			{
				((Component)_regionInfo.Icon).gameObject.AddChild(val);
			}
			_regionInfo.Lv.text = LocalizeUtil.FormatLevel(region.Level);
			_regionInfo.Region.text = T._("[9d998e]{0}[-] [756961]섬[-]", region.Name);
		}
		if (((Component)_regionInfo.Year).gameObject.activeSelf)
		{
			Vector3 localPosition = ((Component)_regionInfo.Parent).transform.localPosition;
			Vector3 localPosition2 = ((Component)_regionInfo.Region).transform.localPosition;
			localPosition2.x = 0f;
			localPosition2.y = 30f;
			((Component)_regionInfo.Region).transform.localPosition = localPosition2 - localPosition;
			Vector3 localPosition3 = ((Component)_regionInfo.Year).transform.localPosition;
			localPosition3.x = 0f;
			localPosition3.y = -20f;
			((Component)_regionInfo.Year).transform.localPosition = localPosition3 - localPosition;
		}
		return true;
	}

	private void SetStatusBar(string text, Color color, bool tween)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_statusBar == (Object)null))
		{
			UISprite uISprite = _statusBar.Get<UISprite>("bg");
			UILabel uILabel = _statusBar.Get<UILabel>("label");
			uILabel.text = text;
			if (tween)
			{
				TweenColor.Begin(((Component)uISprite).gameObject, 0.5f, color);
			}
			else
			{
				uISprite.color = color;
			}
		}
	}

	private IEnumerator CoBackFadeOut()
	{
		while (_isRegionFading)
		{
			yield return null;
		}
		SetStatusBar(T._("게임 서버와 연결 되었습니다"), PresetColor.ConnectedColor, tween: true);
		IsFadeoutStarted = true;
		EventDelegate.Execute(FadeOutStarted);
		while (_backFadeOutDuration > 0f)
		{
			_backFadeOutAlpha -= Time.deltaTime / _backFadeOutDuration;
			_panel.alpha = _backFadeOutAlpha;
			if (_backFadeOutAlpha <= 0f)
			{
				break;
			}
			yield return null;
		}
		FinishFadeOut();
	}

	private void FinishFadeOut()
	{
		if (!GameManager.IsPrologueMode)
		{
			_showCount++;
		}
		IsVisible = false;
		EventDelegate.Execute(FadeOutFinished);
		_backTexture.mainTexture = null;
		Resources.UnloadUnusedAssets();
	}

	public void Show()
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Invalid comparison between Unknown and I4
		ResetFading();
		((Component)_backTexture).gameObject.SetActive(true);
		_loadingIcon.SetActive(true);
		((Component)((Component)_regionInfo.Fatigue).transform.parent).gameObject.SetActive(false);
		UIEventListener.Get(((Component)_backTexture).gameObject).onPress = delegate
		{
			_isTap = true;
		};
		if ((Object)(object)_lastScreenTexture != (Object)null)
		{
			((Component)_statusBar).gameObject.SetActive(true);
			_statusBar.Get<UISprite>("bg").color = PresetColor.TryConnectColor;
			SetStatusBar(T._("게임 서버와 연결 중 입니다"), PresetColor.ConnectingColor, tween: true);
			_backTexture.mainTexture = (Texture)(object)_lastScreenTexture;
			_backTexture.color = Color.white;
			_mainContainer.alpha = 1f;
			_backFadeOutDuration = 1f;
			_lastScreenTexture = null;
		}
		else
		{
			((Component)_statusBar).gameObject.SetActive(false);
			_backTexture.mainTexture = (Texture)(object)Texture2D.whiteTexture;
			_backTexture.color = Color.black;
			_mainContainer.alpha = 0f;
			_backFadeOutDuration = 0.5f;
			if (GameManager.IsPrologueMode && (int)Application.internetReachability == 1)
			{
				((MonoBehaviour)this).StartCoroutine(CoShowDownloadWarning());
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(CoShowRegionInfo());
			}
		}
		if (KSingleton<TerrainA6>.Exist())
		{
			KSingleton<TerrainA6>.Instance().LoadingChunksFinished += Terrain_LoadingChunksFinished;
		}
	}

	public IEnumerator CoTakeScreenShot()
	{
		yield return (object)new WaitForEndOfFrame();
		_lastScreenTexture = new Texture2D(Screen.width, Screen.height, (TextureFormat)3, false);
		_lastScreenTexture.ReadPixels(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), 0, 0);
		_lastScreenTexture.Apply();
	}

	public void ShowTeleportScreen()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ResetFading();
		((Component)_backTexture).gameObject.SetActive(true);
		_backTexture.mainTexture = (Texture)(object)_lastScreenTexture;
		_backTexture.color = Color.white;
		_mainContainer.alpha = 1f;
		_loadingIcon.SetActive(false);
		((Component)_statusBar).gameObject.SetActive(false);
	}

	public void ShowPhaseChangedScreen()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ResetFading();
		((Component)_phaseChangedTexture).gameObject.SetActive(true);
		_phaseChangedTexture.mainTexture = (Texture)(object)_lastScreenTexture;
		_phaseChangedTexture.color = Color.white;
		_mainContainer.alpha = 1f;
		_lastScreenTexture = null;
		_loadingIcon.SetActive(false);
		((Component)_statusBar).gameObject.SetActive(false);
		((MonoBehaviour)this).StartCoroutine(CoPhaseChangedScreenFadeOut());
	}

	private IEnumerator CoPhaseChangedScreenFadeOut()
	{
		while (_phaseChangedFadeOutDuration > 0f)
		{
			if ((Object)(object)_phaseChangedTexture.drawCall != (Object)null)
			{
				_phaseChangedFadeOutAlpha -= Time.deltaTime / _phaseChangedFadeOutDuration;
				_phaseChangedTexture.drawCall.dynamicMaterial.SetFloat("_Alpha", _phaseChangedFadeOutAlpha * 2f - 1f);
			}
			if (_phaseChangedFadeOutAlpha <= 0f)
			{
				_phaseChangedTexture.mainTexture = null;
				IsVisible = false;
				break;
			}
			yield return null;
		}
	}

	private void ResetFading()
	{
		IsVisible = true;
		_isRegionFading = false;
		_isFinished = false;
		_backFadeOutAlpha = 1f;
		_backFadeOutDuration = 1f;
		_phaseChangedFadeOutAlpha = 1f;
		_panel = ((Component)this).GetComponent<UIPanel>();
		_panel.alpha = _backFadeOutAlpha;
		((Component)_regionInfo.Parent).gameObject.SetActive(false);
		((Component)_fictionInfo.Parent).gameObject.SetActive(false);
		((Component)_tooltipInfo.Parent).gameObject.SetActive(false);
		((Component)_continueLabel).gameObject.SetActive(false);
	}
}
