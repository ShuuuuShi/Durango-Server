using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Shop;
using Durango.Logic.Social;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PageMotions : MonoBehaviour
{
	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private PlayerPreviewWidget _playerPreview;

	[SerializeField]
	private FavoritesMotionWidget _favoriteShortcut;

	public void Refresh(bool reset = true)
	{
		Emotional emotional = GameSystem<SocialSystem>.Instance().Emotional;
		List<Durango.Logic.Social.Motion> favs = emotional.Motions.Where((Durango.Logic.Social.Motion elem) => elem.IsSubscribe() && elem.Available).ToList();
		List<Durango.Logic.Social.Motion> unfavs = emotional.Motions.Where((Durango.Logic.Social.Motion elem) => !elem.IsSubscribe() && elem.Visible).ToList();
		InitCategory(favs, unfavs);
		_playerPreview.Set(0.6f);
		if (reset)
		{
			PlayMotion(null);
			_scrollView.ResetPosition();
		}
		else
		{
			_scrollView.Reposition();
		}
	}

	private void InitCategory([NotNull] List<Durango.Logic.Social.Motion> favs, [NotNull] List<Durango.Logic.Social.Motion> unfavs)
	{
		_scrollView.Nodes.BeginLoad();
		EmotionContentWidget component = _scrollView.Nodes.GetNext().GetComponent<EmotionContentWidget>();
		string categoryTitle = string.Format("{0} {1}", T._("즐겨찾기"), favs.Count.ToString().ToEncodedColor(PresetColor.UISunglowYellow));
		component.SetGrids(categoryTitle, favs, delegate(FavoritesMotionWidget node, Durango.Logic.Social.Motion data)
		{
			node.Set(data, delegate
			{
				ClickFavorite(data, isFavoriteNow: false);
			}, delegate
			{
				ClickMotion(data);
			});
		});
		component.UpdateLayoutItems();
		EmotionContentWidget component2 = _scrollView.Nodes.GetNext().GetComponent<EmotionContentWidget>();
		component2.SetGrids(T._("감정표현"), unfavs, delegate(FavoritesMotionWidget node, Durango.Logic.Social.Motion data)
		{
			node.Set(data, delegate
			{
				ClickFavorite(data, isFavoriteNow: true);
			}, delegate
			{
				ClickMotion(data);
			});
		});
		Vector2 vector = component2.UpdateLayoutItems();
		if (favs.Count == 0)
		{
			component.SetBlank(T._("즐겨찾기를 등록해 보세요."), vector.y);
		}
		component2.ActivateButton(T._("상점"), delegate
		{
			GameSystem<ShopSystem>.Instance().GetPurchasableCommodities(delegate(List<Commodity> list)
			{
				Commodity commodity = list.Find((Commodity x) => KUtility.GetSize(x.Contents.WeightedMotions) > 0);
				if (commodity != null)
				{
					UIManager.FindScript<ShopGroup>().Open(commodity.Id, select: false);
				}
			});
		});
		_scrollView.Nodes.EndLoad();
	}

	private void PlayMotion([CanBeNull] Durango.Logic.Social.Motion motion)
	{
		if (motion == null)
		{
			_playerPreview.SetModelVisibility(isShow: false);
			_favoriteShortcut.gameObject.SetActive(value: false);
			return;
		}
		_favoriteShortcut.gameObject.SetActive(value: true);
		_playerPreview.SetModelVisibility(isShow: true);
		if (motion.MotionNames.Length != 0)
		{
			int num = Random.Range(0, motion.MotionNames.Length);
			_playerPreview.PlayMotion(motion.MotionNames[num]);
		}
		_favoriteShortcut.Set(motion, delegate
		{
			ClickFavorite(motion, !motion.Favorite);
		}, delegate
		{
			ClickMotion(motion);
		});
	}

	private void ClickMotion(Durango.Logic.Social.Motion data)
	{
		if (!data.Available)
		{
			UIManager.SystemMsg(T._("상점에서 구입할 수 있는 감정표현입니다."));
		}
		PlayMotion(data);
		data.ClearNotification();
	}

	private void ClickFavorite(Durango.Logic.Social.Motion data, bool isFavoriteNow)
	{
		if (!data.Available)
		{
			UIManager.SystemMsg(T._("상점에서 구입할 수 있는 감정표현입니다."));
			return;
		}
		if (isFavoriteNow)
		{
			UIManager.Alarm.ShowNotify(T._("[{0}]{1}[-]{1:-을} 즐겨찾기에 추가했습니다.", NGUIText.EncodeColor(PresetColor.UISunglowYellow), data.Name), "icon_emoticon_001", major: false);
		}
		GameSystem<SocialSystem>.Instance().Emotional.SetMotionFavorite(data.Key, isFavoriteNow);
		PlayMotion(data);
		data.ClearNotification();
	}
}
