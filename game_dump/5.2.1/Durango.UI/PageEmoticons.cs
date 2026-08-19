using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Social;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PageEmoticons : MonoBehaviour
{
	[SerializeField]
	private KScrollView _scrollView;

	public void Refresh(bool reset = true)
	{
		Emotional emotional = GameSystem<SocialSystem>.Instance().Emotional;
		List<Emoticon> favs = emotional.Emoticons.Where((Emoticon elem) => elem.Available && elem.IsSubscribe()).ToList();
		List<Emoticon> unfavs = emotional.Emoticons.Where((Emoticon elem) => !elem.IsSubscribe() && elem.Visible).ToList();
		UpdateCategory(favs, unfavs);
		_scrollView.Reposition(reset, !reset);
	}

	private void UpdateCategory(List<Emoticon> favs, List<Emoticon> unfavs)
	{
		_scrollView.Nodes.BeginLoad();
		EmotionContentWidget component = _scrollView.Nodes.GetNext().GetComponent<EmotionContentWidget>();
		string categoryTitle = string.Format("{0} {1}", T._("즐겨찾기"), favs.Count.ToString().ToEncodedColor(PresetColor.UISunglowYellow));
		component.SetGrids(categoryTitle, favs, delegate(FavoritesEmoticonWidget node, Emoticon data)
		{
			node.Set(data, delegate
			{
				ClickFavorite(data, isFavorite: false);
			});
		});
		component.UpdateLayoutItems();
		EmotionContentWidget component2 = _scrollView.Nodes.GetNext().GetComponent<EmotionContentWidget>();
		component2.SetGrids(T._("이모티콘"), unfavs, delegate(FavoritesEmoticonWidget node, Emoticon data)
		{
			node.Set(data, delegate
			{
				ClickFavorite(data, isFavorite: true);
			});
		});
		float y = component2.UpdateLayoutItems().y;
		if (favs.Count == 0)
		{
			component.SetBlank(T._("즐겨찾기를 등록해 보세요."), y);
		}
		_scrollView.Nodes.EndLoad();
		_scrollView.UpdateLayout();
	}

	private static void ClickFavorite(EmotionBase data, bool isFavorite)
	{
		if (isFavorite)
		{
			UIManager.Alarm.ShowNotify(T._("이모티콘을 즐겨찾기에 추가했습니다."), "icon_emoticon_001", major: false);
		}
		GameSystem<SocialSystem>.Instance().Emotional.SetEmoticonFavorite(data.Key, isFavorite);
		data.ClearNotification();
	}
}
