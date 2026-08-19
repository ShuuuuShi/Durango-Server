using System;
using System.Collections;
using Durango.Logic.Encyclopedia;
using Durango.Logic.Explore;
using L10N;
using Messages;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class DefaultLoadingCurtain : LoadingCurtainBase
{
	[Serializable]
	private struct RegionInfo
	{
		public UIWidget Parent;

		public Transform Icon;

		public UILabel Lv;

		public UILabel Region;

		public UIWidget SubtitlesWidget;

		public ListObjectPool Nodes;
	}

	[Serializable]
	private struct GameTipInfo
	{
		public UIWidget Parent;

		public UILabel Contents;

		public void Set(string text)
		{
			Contents.text = text;
		}
	}

	private const int MinimumUnstableFactor = 2;

	private const float CurtainWaitingTime = 10f;

	[SerializeField]
	private RegionInfo _regionInfo;

	[SerializeField]
	private GameTipInfo _gameTipInfo;

	[SerializeField]
	private UITexture _bg;

	[SerializeField]
	private UILabel _continueLabel;

	[SerializeField]
	private GameObject _loadingIcon;

	private bool _isRegionLoaded;

	private bool _isTap;

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
		UICamera.onKey = (UICamera.KeyCodeDelegate)Delegate.Combine(UICamera.onKey, new UICamera.KeyCodeDelegate(OnPressAnyKey));
		SetState(LoadingState.Open);
		StartCoroutine(CoShowRoutine());
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
		UICamera.onKey = (UICamera.KeyCodeDelegate)Delegate.Remove(UICamera.onKey, new UICamera.KeyCodeDelegate(OnPressAnyKey));
	}

	private void Update()
	{
		if (!_isRegionLoaded)
		{
			_isRegionLoaded = TryUpdateRegionInfo();
		}
	}

	private IEnumerator CoShowRoutine()
	{
		_isRegionLoaded = false;
		base.Widget.alpha = 1f;
		_loadingIcon.gameObject.SetActive(value: true);
		_continueLabel.gameObject.SetActive(value: false);
		_bg.color = Color.black;
		UpdateMemoText();
		yield return null;
		TweenColor.Begin(_bg.gameObject, 1f, PresetColor.LoadingColor);
		GameObject memoText = _gameTipInfo.Parent.gameObject;
		TweenAlpha.Begin(memoText, 0.5f, 1f).delay = 1f;
		if (_regionInfo.Parent.gameObject.activeSelf)
		{
			_regionInfo.Parent.alpha = 0f;
			TweenAlpha.Begin(_regionInfo.Parent.gameObject, 0.5f, 1f).delay = 2f;
		}
		yield return WaitForChunkLoading();
		if (LoadingCurtainBase.IsChunkLoadFailed)
		{
			yield break;
		}
		_loadingIcon.gameObject.SetActive(value: false);
		_continueLabel.gameObject.SetActive(value: true);
		_isTap = false;
		float timeWhenLoadingDone = Time.realtimeSinceStartup;
		while (!_isTap)
		{
			if (Time.realtimeSinceStartup - timeWhenLoadingDone > 10f)
			{
				_isTap = true;
			}
			yield return null;
		}
		SetState(LoadingState.Closing);
		yield return Fadeout();
		SetState(LoadingState.Closed);
	}

	private void OnTouchScreen(GameObject obj, bool press)
	{
		_isTap = true;
	}

	private void OnPressAnyKey(GameObject go, KeyCode key)
	{
		OnTouchScreen(null, press: true);
	}

	private void UpdateMemoText()
	{
		_gameTipInfo.Parent.alpha = 0f;
		MemoType type = MemoType.Fiction;
		if (LoadingCurtainGroup.LoadingCount > 0)
		{
			type = MemoType.Tooltip;
		}
		_gameTipInfo.Parent.gameObject.SetActive(value: true);
		int randomMemo = MemoSystem.GetRandomMemo(type);
		string text = ((randomMemo != -1) ? MemoSystem.GetMemoFullText(type, randomMemo) : string.Empty);
		_gameTipInfo.Set(text);
	}

	private bool TryUpdateRegionInfo()
	{
		Durango.Logic.Explore.Region region = GameManager.Region;
		if (region.Template == null)
		{
			_regionInfo.Parent.gameObject.SetActive(value: false);
			return false;
		}
		_regionInfo.Parent.gameObject.SetActive(value: true);
		_regionInfo.Parent.alpha = 0f;
		TweenAlpha tweenAlpha = TweenAlpha.Begin(_regionInfo.Parent.gameObject, 0.5f, 1f);
		tweenAlpha.delay = 2f;
		Durango.Logic.Explore.Region.InstantiateIcon(_regionInfo.Icon, region.GetEmblem());
		_regionInfo.Lv.text = T._("{0:lv:} {1}", region.Level, LocalizeUtil.Get(region.Role()));
		_regionInfo.Region.text = region.Name;
		Messages.Archipelago? archipelago = GameManager.Archipelago;
		_regionInfo.Nodes.BeginLoad();
		if (archipelago.HasValue && archipelago.Value.UnstableFactor >= 2)
		{
			UILabel component = _regionInfo.Nodes.GetNext().GetComponent<UILabel>();
			component.text = $"<em>[icon=icon_unstable_factor] {archipelago.Value.UnstableFactor}</em>";
			component = _regionInfo.Nodes.GetNext().GetComponent<UILabel>();
			component.text = LocalizeUtil.Get(region.MajorBiome());
			Recommends recommends = SingletonDict<int, Recommends>.Instance.Get(archipelago.Value.UnstableFactor);
			Derived derived = Singleton<Constants>.Instance.Resistance.TypeByBiome.Get(region.MajorBiome(), Derived.Invalid);
			if (derived != Derived.Invalid && recommends != null)
			{
				string arg = LocalizeUtil.Get(derived);
				string arg2 = LocalizeUtil.FormatLevel(recommends.ResistanceLevel);
				component = _regionInfo.Nodes.GetNext().GetComponent<UILabel>();
				component.text = $"{arg} {arg2}";
			}
		}
		else
		{
			UILabel component2 = _regionInfo.Nodes.GetNext().GetComponent<UILabel>();
			component2.text = LocalizeUtil.Get(region.MajorBiome());
		}
		_regionInfo.Nodes.EndLoad();
		UIUtility.WidgetsReposition(_regionInfo.Nodes, _regionInfo.SubtitlesWidget, Vector3.right, 18f);
		return true;
	}
}
