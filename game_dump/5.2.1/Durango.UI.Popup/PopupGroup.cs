using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI.Popup;

public class PopupGroup : UIBase
{
	[SerializeField]
	private LoadingIconControl _loadingIcon;

	[SerializeField]
	private LoadingRingWidget _loadingRing;

	[SerializeField]
	private EstateOwnerWidget _estateOwnerWidget;

	private readonly Dictionary<Type, TooltipBase> _tooltipDict = new Dictionary<Type, TooltipBase>();

	private float _loadingIconActiveTime;

	private bool _isLoading;

	public LoadingRingWidget LoadingRing => _loadingRing;

	public EstateOwnerWidget EstateOwnerWidget => _estateOwnerWidget;

	public bool IsLoading
	{
		get
		{
			return _isLoading;
		}
		set
		{
			_isLoading = value;
			TweenAlpha component = _loadingIcon.GetComponent<TweenAlpha>();
			if (value)
			{
				if (!_loadingIcon.gameObject.activeSelf)
				{
					_loadingIcon.gameObject.SetActive(value: true);
				}
				component.delay = 0f;
				component.PlayForward();
				_loadingIconActiveTime = Time.time;
			}
			else
			{
				float num = Time.time - _loadingIconActiveTime;
				if (num < 0.5f)
				{
					component.delay = 0.5f - num;
				}
				else
				{
					component.delay = 0f;
				}
				component.PlayReverse();
				component.ResetToBeginning();
			}
		}
	}

	private void Awake()
	{
		_loadingIcon.GetComponent<TweenAlpha>().SetOnFinished(OnLoadingIconTweenFinished);
		_loadingIcon.gameObject.SetActive(value: false);
		try
		{
			Dictionary<TooltipBase.DepthEnum, GameObject> dictionary = new Dictionary<TooltipBase.DepthEnum, GameObject>();
			foreach (GameObject popup in GetPopupList())
			{
				TooltipBase component = popup.GetComponent<TooltipBase>();
				if (!(component == null))
				{
					int depth = 7000;
					int layer = LayerHelper.UILayer;
					switch (component.Depth)
					{
					case TooltipBase.DepthEnum.Over1:
						depth = 7010;
						break;
					case TooltipBase.DepthEnum.Over2:
						depth = 7020;
						break;
					case TooltipBase.DepthEnum.Over3:
						depth = 7030;
						break;
					case TooltipBase.DepthEnum.Floor:
						depth = -100;
						break;
					case TooltipBase.DepthEnum.OverUI:
						depth = 10100;
						layer = LayerHelper.UIOverLayer;
						break;
					}
					if (!dictionary.TryGetValue(component.Depth, out var value))
					{
						GameObject gameObject = new GameObject(component.Depth.ToString());
						Transform obj = gameObject.transform;
						obj.parent = base.transform;
						obj.localPosition = Vector3.zero;
						obj.localRotation = Quaternion.identity;
						obj.localScale = Vector3.one;
						gameObject.AddComponent<UIPanel>().depth = depth;
						gameObject.layer = layer;
						dictionary[component.Depth] = gameObject;
						value = gameObject;
					}
					UnityEngine.Object.Instantiate(popup, value.transform).GetComponent<TooltipBase>().InitializePanelDepth(depth);
				}
			}
			foreach (KeyValuePair<TooltipBase.DepthEnum, GameObject> item in dictionary)
			{
				NGUITools.SetLayer(item.Value, item.Value.layer);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		LoadingRing.Init();
		_estateOwnerWidget.Hide();
	}

	private void Start()
	{
		SetVisible(visible: false, "Loading");
		UIManager.OnLoadingCurtainHidden(delegate
		{
			SetVisible(visible: true, "Loading");
		});
	}

	protected virtual IEnumerable<GameObject> GetPopupList()
	{
		return from x in Resources.LoadAll<GameObject>("Popup")
			where !x.name.ContainsIgnoreCase("_PC")
			select x;
	}

	public T Tooltip<T>() where T : TooltipBase
	{
		T val;
		if (_tooltipDict.TryGetValue(typeof(T), out var value))
		{
			val = value as T;
		}
		else
		{
			val = GetComponentInChildren<T>(includeInactive: true);
			_tooltipDict.Add(typeof(T), val);
		}
		if (val != null)
		{
			val.MuteOpenCloseSound = true;
			val.Hide();
			val.MuteOpenCloseSound = false;
		}
		return val;
	}

	public T FindTooltip<T>() where T : TooltipBase
	{
		T val;
		if (_tooltipDict.TryGetValue(typeof(T), out var value))
		{
			val = value as T;
		}
		else
		{
			val = GetComponentInChildren<T>(includeInactive: true);
			_tooltipDict.Add(typeof(T), val);
		}
		return val;
	}

	private void OnLoadingIconTweenFinished()
	{
		TweenAlpha component = _loadingIcon.GetComponent<TweenAlpha>();
		if (component.tweenFactor >= 1f)
		{
			component.delay = 5f;
			component.PlayReverse();
			component.ResetToBeginning();
		}
		else
		{
			_loadingIcon.gameObject.SetActive(value: false);
		}
	}
}
