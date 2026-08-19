using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

	[CompilerGenerated]
	private sealed class _003CCoShowRoutine_003Ed__14 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public DefaultLoadingCurtain _003C_003E4__this;

		private float _003CtimeWhenLoadingDone_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoShowRoutine_003Ed__14(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			DefaultLoadingCurtain defaultLoadingCurtain = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				defaultLoadingCurtain._isRegionLoaded = false;
				defaultLoadingCurtain.Widget.alpha = 1f;
				defaultLoadingCurtain._loadingIcon.gameObject.SetActive(value: true);
				defaultLoadingCurtain._continueLabel.gameObject.SetActive(value: false);
				defaultLoadingCurtain._bg.color = Color.black;
				defaultLoadingCurtain.UpdateMemoText();
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				TweenColor.Begin(defaultLoadingCurtain._bg.gameObject, 1f, PresetColor.LoadingColor);
				TweenAlpha.Begin(defaultLoadingCurtain._gameTipInfo.Parent.gameObject, 0.5f, 1f).delay = 1f;
				if (defaultLoadingCurtain._regionInfo.Parent.gameObject.activeSelf)
				{
					defaultLoadingCurtain._regionInfo.Parent.alpha = 0f;
					TweenAlpha.Begin(defaultLoadingCurtain._regionInfo.Parent.gameObject, 0.5f, 1f).delay = 2f;
				}
				_003C_003E2__current = defaultLoadingCurtain.WaitForChunkLoading();
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				if (LoadingCurtainBase.IsChunkLoadFailed)
				{
					return false;
				}
				defaultLoadingCurtain._loadingIcon.gameObject.SetActive(value: false);
				defaultLoadingCurtain._continueLabel.gameObject.SetActive(value: true);
				defaultLoadingCurtain._isTap = false;
				_003CtimeWhenLoadingDone_003E5__2 = Time.realtimeSinceStartup;
				goto IL_01bc;
			case 3:
				_003C_003E1__state = -1;
				goto IL_01bc;
			case 4:
				{
					_003C_003E1__state = -1;
					defaultLoadingCurtain.SetState(LoadingState.Closed);
					return false;
				}
				IL_01bc:
				if (!defaultLoadingCurtain._isTap)
				{
					if (Time.realtimeSinceStartup - _003CtimeWhenLoadingDone_003E5__2 > 10f)
					{
						defaultLoadingCurtain._isTap = true;
					}
					_003C_003E2__current = null;
					_003C_003E1__state = 3;
					return true;
				}
				defaultLoadingCurtain.SetState(LoadingState.Closing);
				_003C_003E2__current = defaultLoadingCurtain.Fadeout();
				_003C_003E1__state = 4;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoShowRoutine_003Ed__14(0)
		{
			_003C_003E4__this = this
		};
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
		TweenAlpha.Begin(_regionInfo.Parent.gameObject, 0.5f, 1f).delay = 2f;
		Durango.Logic.Explore.Region.InstantiateIcon(_regionInfo.Icon, region.GetEmblem());
		_regionInfo.Lv.text = T._("{0:lv:} {1}", region.Level, LocalizeUtil.Get(region.Role()));
		_regionInfo.Region.text = region.Name;
		Messages.Archipelago? archipelago = GameManager.Archipelago;
		_regionInfo.Nodes.BeginLoad();
		if (archipelago.HasValue && archipelago.Value.UnstableFactor >= 2)
		{
			_regionInfo.Nodes.GetNext().GetComponent<UILabel>().text = $"<em>[icon=icon_unstable_factor] {archipelago.Value.UnstableFactor}</em>";
			_regionInfo.Nodes.GetNext().GetComponent<UILabel>().text = LocalizeUtil.Get(region.MajorBiome());
			Recommends recommends = SingletonDict<int, Recommends>.Instance.Get(archipelago.Value.UnstableFactor);
			Derived derived = Singleton<Constants>.Instance.Resistance.TypeByBiome.Get(region.MajorBiome(), Derived.Invalid);
			if (derived != Derived.Invalid && recommends != null)
			{
				string text = LocalizeUtil.Get(derived);
				string text2 = LocalizeUtil.FormatLevel(recommends.ResistanceLevel);
				_regionInfo.Nodes.GetNext().GetComponent<UILabel>().text = text + " " + text2;
			}
		}
		else
		{
			_regionInfo.Nodes.GetNext().GetComponent<UILabel>().text = LocalizeUtil.Get(region.MajorBiome());
		}
		_regionInfo.Nodes.EndLoad();
		UIUtility.WidgetsReposition(_regionInfo.Nodes, _regionInfo.SubtitlesWidget, Vector3.right, 18f);
		return true;
	}
}
