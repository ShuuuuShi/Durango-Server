using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MenuBannerList : MenuListWidgetBase
{
	[SerializeField]
	private PromotionBannerWidget _bannerWidgetBase;

	[SerializeField]
	private UIWidget _bannerHolder;

	[SerializeField]
	private Vector2 _bannerLandscapeOffset;

	[SerializeField]
	private Vector2 _bannerPortraitOffset;

	[SerializeField]
	private int _maxBannerCount;

	[SerializeField]
	private Vector2 _bannerSmallSize;

	[SerializeField]
	private Vector2 _bannerNormalSize;

	[SerializeField]
	private UIWidget _menuHolder;

	[SerializeField]
	private Vector2 _menuLandscapeOffset;

	[SerializeField]
	private Vector2 _menuPortraitOffset;

	private readonly List<PromotionLink> _customLinks = new List<PromotionLink>();

	private ListObjectPool<PromotionBannerWidget> _promotionBannerWidgets;

	[ExposedInEditor(null)]
	public void Refresh()
	{
		Init();
		RefreshBanner(GetCustomLiks().Concat(GetPromotionLinks()));
		RefreshMenu(Enumerable.Repeat(MenuType.Screenshot, 1));
	}

	public void Show(bool instant, bool isPortrait)
	{
		Init();
		base.gameObject.SetActive(value: true);
		UpdateHoldersOffset(isPortrait);
		UpdateLayout(isPortrait);
		if (instant)
		{
			this.SetEnable<TweenAlpha>(enable: false);
			alpha = 1f;
		}
		else
		{
			TweenAlpha.Begin(base.gameObject, 0.2f, 1f);
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnInitialized()
	{
		CreateCustomLinks();
		_promotionBannerWidgets = new ListObjectPool<PromotionBannerWidget>();
		_promotionBannerWidgets.BaseObject = _bannerWidgetBase;
		_promotionBannerWidgets.UseBase = true;
		base.OnInitialized();
	}

	private void RefreshBanner(IEnumerable<PromotionLink> links)
	{
		_promotionBannerWidgets.BeginLoad();
		int num = 0;
		foreach (PromotionLink link in links)
		{
			if (PromotionBannerWidget.IsShowPeriod(link))
			{
				PromotionBannerWidget next = _promotionBannerWidgets.GetNext();
				next.Set(link);
			}
			if (_maxBannerCount > 0 && ++num >= _maxBannerCount)
			{
				break;
			}
		}
		_promotionBannerWidgets.EndLoad();
	}

	private void RefreshMenu(IEnumerable<MenuType> types)
	{
		_menuList.BeginLoad();
		foreach (MenuType item in types.Where((MenuType t) => GameSystem<MenuSystem>.Instance().IsEnabled(t)))
		{
			MenuWidget next = _menuList.GetNext();
			next.Set(item);
		}
		_menuList.EndLoad();
	}

	private void UpdateHoldersOffset(bool isPortrait)
	{
		if (isPortrait)
		{
			_bannerHolder.transform.localPosition = _bannerPortraitOffset;
			_menuHolder.transform.localPosition = _menuPortraitOffset;
		}
		else
		{
			_bannerHolder.transform.localPosition = _bannerLandscapeOffset;
			_menuHolder.transform.localPosition = _menuLandscapeOffset;
		}
	}

	private void UpdateLayout(bool isPortrait)
	{
		Vector2 baseNodeSize = ((!isPortrait || _promotionBannerWidgets.Count != 3) ? _bannerNormalSize : _bannerSmallSize);
		UpdateBannerSize((int)baseNodeSize.x, (int)baseNodeSize.y);
		int b = base.width / (int)baseNodeSize.x;
		float num = baseNodeSize.x * (float)Mathf.Min(_promotionBannerWidgets.Count, b);
		UIUtility.WidgetsGridReposition(_promotionBannerWidgets, null, Vector2.up, new Vector3((0f - num) / 2f, 0f, 0f), num, baseNodeSize, 0f, 0f);
		UIUtility.WidgetsReposition(_menuList, _menuHolder, Vector3.left);
	}

	private void UpdateBannerSize(int sizeX, int sizeY)
	{
		if (sizeX <= 0 || sizeY <= 0)
		{
			return;
		}
		foreach (PromotionBannerWidget promotionBannerWidget in _promotionBannerWidgets)
		{
			if (promotionBannerWidget.width != sizeX || promotionBannerWidget.height != sizeY)
			{
				promotionBannerWidget.width = sizeX;
				promotionBannerWidget.height = sizeY;
				UIUtility.UpdateAnchors(promotionBannerWidget.transform);
			}
		}
	}

	private IEnumerable<PromotionLink> GetCustomLiks()
	{
		if (GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.PvpIsland))
		{
			yield return _customLinks[0];
		}
		if (GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Music))
		{
			yield return _customLinks[1];
		}
		if (GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Story))
		{
			yield return _customLinks[2];
		}
	}

	private static IEnumerable<PromotionLink> GetPromotionLinks()
	{
		return (!GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Shop)) ? Enumerable.Empty<PromotionLink>() : Singleton<Commodities>.Instance.PromotionLinks;
	}

	private void CreateCustomLinks()
	{
		_customLinks.Add(new PromotionLink
		{
			HudText = T._("난투섬에\n도전하라!"),
			BackgroundColor = "324453DC",
			Image = "warprush_banner",
			WebLink = "ui://PvpIsland"
		});
		_customLinks.Add(new PromotionLink
		{
			HudText = T._("악기연주"),
			BackgroundColor = "27215BDC",
			Image = "instrument_banner",
			WebLink = "ui://Music"
		});
		_customLinks.Add(new PromotionLink
		{
			HudText = T._("일지"),
			BackgroundColor = "314D52DC",
			Image = "story_banner",
			WebLink = "ui://Story"
		});
	}
}
